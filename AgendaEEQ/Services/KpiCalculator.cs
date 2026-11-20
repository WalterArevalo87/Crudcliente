using System.Globalization;
using AgendaEEQ.Models;
using Microsoft.Extensions.Options;

namespace AgendaEEQ.Services
{
    /// <summary>Calcula los indicadores del tablero a partir de la agenda cargada.</summary>
    public class KpiCalculator
    {
        private static readonly CultureInfo Es = new("es-EC");
        private static readonly string[] NombresMes =
            { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
        private static readonly string[] MesesLargos =
            { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };

        private readonly KpiMetas _m;

        public KpiCalculator(IOptions<KpiMetas> metas) => _m = metas.Value;

        public KpiMetas Metas => _m;

        // ---------- Filtros ----------

        private static bool Coincide(string valor, string filtro) =>
            string.IsNullOrEmpty(filtro) || AgendaExcelReader.Norm(valor) == AgendaExcelReader.Norm(filtro);

        private static bool CoincideResp(string valor, string filtro) =>
            string.IsNullOrEmpty(filtro) || AgendaExcelReader.ClaveNombre(valor) == AgendaExcelReader.ClaveNombre(filtro);

        public IEnumerable<Solicitud> Solicitudes(AgendaData a, KpiFiltro f) => a.Solicitudes.Where(s =>
            (!f.Anio.HasValue || s.Anio == f.Anio) && CoincideResp(s.Responsable, f.Responsable) && Coincide(s.Parroquia, f.Parroquia));

        public IEnumerable<Diseno> Disenos(AgendaData a, KpiFiltro f) => a.Disenos.Where(d =>
            (!f.Anio.HasValue || d.Anio == f.Anio) && CoincideResp(d.Responsable, f.Responsable) && Coincide(d.Parroquia, f.Parroquia));

        public IEnumerable<Obra> Obras(AgendaData a, KpiFiltro f) => a.Obras.Where(o =>
            (!f.Anio.HasValue || o.Anio == f.Anio) && CoincideResp(o.Disenador, f.Responsable) && Coincide(o.Parroquia, f.Parroquia));

        public IEnumerable<Inspeccion> Inspecciones(AgendaData a, KpiFiltro f) => a.Inspecciones.Where(i =>
            (!f.Anio.HasValue || i.Anio == f.Anio) && Coincide(i.Parroquia, f.Parroquia) &&
            (string.IsNullOrEmpty(f.Responsable) ||
             AgendaExcelReader.Norm(i.Asistentes).Contains(AgendaExcelReader.ClaveNombre(f.Responsable))));

        public bool DentroPlazo(Solicitud s) => s.DiasRespuesta is >= 0 && s.DiasRespuesta <= _m.PlazoRespuestaDias;
        public bool FueraPlazo(Solicitud s) => s.DiasRespuesta > _m.PlazoRespuestaDias;
        public bool InicioTardio(Diseno d) => d.DiasHastaInicio > _m.InicioDisenoMeta;

        public static bool DatosCompletos(Solicitud s) =>
            s.FechaIngreso.HasValue && s.Responsable != null && s.Atendido != null && s.Estado != null && !s.FechaInconsistente;

        // ---------- Tablero ----------

        public KpiDashboardViewModel Tablero(AgendaData a, KpiFiltro f)
        {
            var vm = new KpiDashboardViewModel { Agenda = a, Filtro = f, Metas = _m };

            vm.Anios = a.Solicitudes.Select(s => s.Anio).Concat(a.Disenos.Select(d => d.Anio))
                .Concat(a.Inspecciones.Select(i => i.Anio)).Concat(a.Obras.Select(o => o.Anio))
                .Where(x => x.HasValue).Select(x => x.Value).Distinct().OrderByDescending(x => x).ToList();
            vm.Responsables = a.Solicitudes.Select(s => s.Responsable).Concat(a.Disenos.Select(d => d.Responsable))
                .Where(x => x != null).Distinct().OrderBy(x => x).ToList();
            vm.Parroquias = a.Solicitudes.Select(s => s.Parroquia).Concat(a.Disenos.Select(d => d.Parroquia))
                .Where(x => x != null).Distinct().OrderBy(x => x).ToList();

            var sol = Solicitudes(a, f).ToList();
            var dis = Disenos(a, f).ToList();
            var obras = Obras(a, f).ToList();
            var insp = Inspecciones(a, f).ToList();

            CalcularSolicitudes(vm, sol);
            CalcularDisenos(vm, dis);
            CalcularGestion(vm, obras, insp);
            CalcularGraficos(vm, f, sol, dis, obras, insp);
            vm.PorResponsable = PorResponsable(sol, dis);
            return vm;
        }

        private void CalcularSolicitudes(KpiDashboardViewModel vm, List<Solicitud> sol)
        {
            int total = sol.Count;
            int atendidas = sol.Count(s => s.EsAtendida);
            int noAtendidas = sol.Count(s => !s.EsAtendida);
            var conTiempo = sol.Where(s => s.DiasRespuesta is >= 0).ToList();
            int dentro = conTiempo.Count(DentroPlazo);
            int completos = sol.Count(DatosCompletos);

            vm.IndicadoresSolicitudes.Add(new KpiIndicador
            {
                Nombre = "Solicitudes registradas",
                Valor = total.ToString("N0", Es),
                Detalle = $"{sol.Count(s => s.EsNoAtendida)} marcadas \"No atendido\", {sol.Count(s => s.Atendido == null)} sin estado",
                Estado = Semaforo.Info,
                Formula = "Número de filas de la hoja SOLICITUDES que cumplen los filtros.",
                VistaDetalle = "solicitudes",
            });

            vm.IndicadoresSolicitudes.Add(Porcentaje("Solicitudes atendidas", atendidas, total,
                _m.AtencionMeta, _m.AtencionAlerta, mayorEsMejor: true,
                "Atendidas ÷ total de solicitudes (columna \"Atendido/No atendido\").",
                $"{noAtendidas} no atendidas o sin estado", "solicitudes-no-atendidas"));

            var promedio = conTiempo.Count > 0 ? conTiempo.Average(s => s.DiasRespuesta.Value) : (double?)null;
            var mediana = Mediana(conTiempo.Select(s => (double)s.DiasRespuesta.Value));
            vm.IndicadoresSolicitudes.Add(new KpiIndicador
            {
                Nombre = "Tiempo promedio de respuesta",
                Valor = promedio.HasValue ? $"{promedio:N1} días" : "—",
                Detalle = mediana.HasValue ? $"Mediana {mediana:N0} días · {conTiempo.Count} solicitudes con fechas" : "Sin fechas de respuesta",
                Meta = $"≤ {_m.TiempoRespuestaMeta:N0} días",
                Estado = SemaforoValor(promedio, _m.TiempoRespuestaMeta, _m.TiempoRespuestaAlerta, mayorEsMejor: false),
                Formula = "Promedio de (primera fecha de respuesta al cliente − fecha de ingreso a la sección), en días calendario. " +
                          "Se toma la fecha más temprana entre Sección, Dpto., DDZC y GD. Se excluyen fechas inconsistentes.",
                VistaDetalle = "solicitudes-fuera-plazo",
            });

            vm.IndicadoresSolicitudes.Add(Porcentaje($"Respondidas en ≤ {_m.PlazoRespuestaDias} días", dentro, conTiempo.Count,
                _m.DentroPlazoMeta, _m.DentroPlazoAlerta, mayorEsMejor: true,
                $"Solicitudes respondidas en {_m.PlazoRespuestaDias} días o menos ÷ solicitudes con fecha de ingreso y de respuesta.",
                $"{conTiempo.Count - dentro} fuera de plazo · {sol.Count(s => s.FechaRespuesta == null)} sin fecha de respuesta",
                "solicitudes-fuera-plazo"));

            vm.IndicadoresSolicitudes.Add(Porcentaje("Calidad de datos", completos, total,
                _m.CalidadDatosMeta, _m.CalidadDatosAlerta, mayorEsMejor: true,
                "Solicitudes con fecha de ingreso, responsable, estado y atendido/no atendido llenos, y sin respuesta anterior al ingreso.",
                $"{total - completos} registros incompletos · {sol.Count(s => s.FechaInconsistente)} con fechas inconsistentes",
                "solicitudes-inconsistentes"));
        }

        private void CalcularDisenos(KpiDashboardViewModel vm, List<Diseno> dis)
        {
            int total = dis.Count;
            var vigentes = dis.Where(d => !d.EsDescartado).ToList();
            int concluidos = vigentes.Count(d => d.EsConcluido);
            var conAvance = dis.Where(d => d.Avance.HasValue).ToList();
            var avance = conAvance.Count > 0 ? conAvance.Average(d => d.Avance.Value) * 100 : (double?)null;
            var conInicio = dis.Where(d => d.DiasHastaInicio is >= 0).ToList();
            var inicio = conInicio.Count > 0 ? conInicio.Average(d => d.DiasHastaInicio.Value) : (double?)null;

            vm.IndicadoresDisenos.Add(new KpiIndicador
            {
                Nombre = "Proyectos en diseño",
                Valor = total.ToString("N0", Es),
                Detalle = $"{dis.Count(d => d.EsEnviadoConstruccion)} enviados a construcción · {dis.Count(d => d.EsDescartado)} no viables / sin trazado",
                Estado = Semaforo.Info,
                Formula = "Número de filas de la hoja DISEÑOS que cumplen los filtros.",
                VistaDetalle = "disenos",
            });

            vm.IndicadoresDisenos.Add(Porcentaje("Diseños concluidos", concluidos, vigentes.Count,
                _m.DisenosConcluidosMeta, _m.DisenosConcluidosAlerta, mayorEsMejor: true,
                "(Diseñado + Enviado a construcción) ÷ proyectos vigentes (se excluyen \"No viable\" y \"Sin trazado vial\").",
                $"{vigentes.Count - concluidos} pendientes (recibidos o en proceso)", "disenos-pendientes"));

            vm.IndicadoresDisenos.Add(new KpiIndicador
            {
                Nombre = "Avance promedio",
                Valor = avance.HasValue ? $"{avance:N1} %" : "—",
                Detalle = $"{conAvance.Count} proyectos con avance registrado",
                Meta = $"≥ {_m.AvanceMeta:N0} %",
                Estado = SemaforoValor(avance, _m.AvanceMeta, _m.AvanceAlerta, mayorEsMejor: true),
                Formula = "Promedio de la columna \"Avance %\".",
                VistaDetalle = "disenos-pendientes",
            });

            vm.IndicadoresDisenos.Add(new KpiIndicador
            {
                Nombre = "Días hasta iniciar diseño",
                Valor = inicio.HasValue ? $"{inicio:N1} días" : "—",
                Detalle = $"Mediana {Mediana(conInicio.Select(d => (double)d.DiasHastaInicio.Value)):N0} días · " +
                          $"{conInicio.Count} proyectos con ambas fechas · {conInicio.Count(InicioTardio)} sobre la meta",
                Meta = $"≤ {_m.InicioDisenoMeta:N0} días",
                Estado = SemaforoValor(inicio, _m.InicioDisenoMeta, _m.InicioDisenoAlerta, mayorEsMejor: false),
                Formula = "Promedio de (fecha de inicio de diseño − fecha de ingreso a la sección).",
                VistaDetalle = "disenos-inicio-tardio",
            });

            vm.IndicadoresDisenos.Add(new KpiIndicador
            {
                Nombre = "Presupuesto diseñado",
                Valor = Dinero(dis.Sum(d => d.PresupuestoTotal ?? 0)),
                Detalle = $"Enviado a construcción: {Dinero(dis.Where(d => d.EsEnviadoConstruccion).Sum(d => d.PresupuestoTotal ?? 0))}",
                Estado = Semaforo.Info,
                Formula = "Suma de la columna \"Presupuesto Total\" (incluye IVA).",
                VistaDetalle = "disenos",
            });

            vm.IndicadoresDisenos.Add(new KpiIndicador
            {
                Nombre = "Viviendas beneficiadas",
                Valor = dis.Sum(d => d.Viviendas ?? 0).ToString("N0", Es),
                Detalle = $"MV {dis.Sum(d => d.KmMedioVoltaje ?? 0):N2} km · BV {dis.Sum(d => d.KmBajoVoltaje ?? 0):N2} km · " +
                          $"{dis.Sum(d => d.Luminarias ?? 0):N0} luminarias",
                Estado = Semaforo.Info,
                Formula = "Suma de \"Vtot (#)\", \"MV (km)\", \"BV (km)\" y \"Luminarias (#)\".",
                VistaDetalle = "disenos",
            });
        }

        private void CalcularGestion(KpiDashboardViewModel vm, List<Obra> obras, List<Inspeccion> insp)
        {
            vm.IndicadoresGestion.Add(new KpiIndicador
            {
                Nombre = "Inspecciones técnicas",
                Valor = insp.Count(i => AgendaExcelReader.Norm(i.Asunto).StartsWith("inspeccion")).ToString("N0", Es),
                Detalle = $"{insp.Count(i => AgendaExcelReader.Norm(i.Asunto).StartsWith("reunion"))} reuniones",
                Estado = Semaforo.Info,
                Formula = "Filas de INSPECCIONES Y REUNIONES por asunto.",
                VistaDetalle = "inspecciones",
            });
            vm.IndicadoresGestion.Add(new KpiIndicador
            {
                Nombre = "Obras construidas",
                Valor = obras.Count(o => o.Estado == "CONSTRUIDO").ToString("N0", Es),
                Detalle = $"Presupuesto: {Dinero(obras.Sum(o => o.PresupuestoTotal ?? 0))}",
                Estado = Semaforo.Info,
                Formula = "Filas de la hoja CONSTRUCCIÓN con estado CONSTRUIDO (año = fecha del memorando de envío).",
                VistaDetalle = "obras",
            });
        }

        private void CalcularGraficos(KpiDashboardViewModel vm, KpiFiltro f, List<Solicitud> sol, List<Diseno> dis,
            List<Obra> obras, List<Inspeccion> insp)
        {
            if (f.Anio.HasValue)
            {
                vm.EtiquetaPeriodo = "mes";
                var porMes = sol.GroupBy(MesSolicitud).ToDictionary(g => g.Key, g => g.ToList());
                vm.SolicitudesPorPeriodo = new Serie
                {
                    Etiquetas = NombresMes.ToList(),
                    Valores = Enumerable.Range(1, 12).Select(m => (double)(porMes.GetValueOrDefault(m)?.Count ?? 0)).ToList(),
                };
                vm.TiempoRespuestaPorPeriodo = new Serie
                {
                    Etiquetas = NombresMes.ToList(),
                    Valores = Enumerable.Range(1, 12).Select(m => PromedioDias(porMes.GetValueOrDefault(m))).ToList(),
                };
            }
            else
            {
                vm.EtiquetaPeriodo = "año";
                var anios = sol.Where(s => s.Anio.HasValue).GroupBy(s => s.Anio.Value).OrderBy(g => g.Key).ToList();
                vm.SolicitudesPorPeriodo = new Serie
                {
                    Etiquetas = anios.Select(g => g.Key.ToString()).ToList(),
                    Valores = anios.Select(g => (double)g.Count()).ToList(),
                };
                vm.TiempoRespuestaPorPeriodo = new Serie
                {
                    Etiquetas = anios.Select(g => g.Key.ToString()).ToList(),
                    Valores = anios.Select(g => PromedioDias(g.ToList())).ToList(),
                };
            }

            vm.SolicitudesPorEstado = Conteo(sol.Select(s => s.Estado), 10);
            vm.SolicitudesPorTipo = Conteo(sol.Select(s => s.TipoRequerimiento), 8);
            vm.DisenosPorEstado = Conteo(dis.Select(d => d.Estado), 10);
            vm.DisenosPorEstadoContractual = Conteo(dis.Select(d => d.EstadoContractual), 10);
            vm.ObrasPorTipo = Conteo(obras.Select(o => o.TipoProyecto), 10);

            var planes = dis.GroupBy(d => d.PlanObra ?? "Sin plan")
                .Select(g => (g.Key, Suma: g.Sum(d => d.PresupuestoTotal ?? 0)))
                .Where(x => x.Suma > 0).OrderByDescending(x => x.Suma).Take(10).ToList();
            vm.PresupuestoPorPlan = new Serie
            {
                Etiquetas = planes.Select(p => p.Key).ToList(),
                Valores = planes.Select(p => Math.Round(p.Suma, 2)).ToList(),
            };

            var inspMes = insp.Where(i => i.Fecha.HasValue)
                .GroupBy(i => new DateTime(i.Fecha.Value.Year, i.Fecha.Value.Month, 1)).OrderBy(g => g.Key).ToList();
            vm.InspeccionesPorMes = new Serie
            {
                Etiquetas = inspMes.Select(g => $"{NombresMes[g.Key.Month - 1]} {g.Key.Year}").ToList(),
                Valores = inspMes.Select(g => (double)g.Count()).ToList(),
            };
        }

        private List<FilaResponsable> PorResponsable(List<Solicitud> sol, List<Diseno> dis)
        {
            var nombres = sol.Select(s => s.Responsable).Concat(dis.Select(d => d.Responsable))
                .Where(n => n != null).Distinct();
            return nombres.Select(n =>
            {
                var s = sol.Where(x => x.Responsable == n).ToList();
                var d = dis.Where(x => x.Responsable == n).ToList();
                var conTiempo = s.Where(x => x.DiasRespuesta is >= 0).ToList();
                return new FilaResponsable
                {
                    Responsable = n,
                    Solicitudes = s.Count,
                    Atendidas = s.Count(x => x.EsAtendida),
                    PorcentajeAtendidas = s.Count > 0 ? 100.0 * s.Count(x => x.EsAtendida) / s.Count : null,
                    DiasPromedioRespuesta = conTiempo.Count > 0 ? conTiempo.Average(x => x.DiasRespuesta.Value) : null,
                    PorcentajeDentroPlazo = conTiempo.Count > 0 ? 100.0 * conTiempo.Count(DentroPlazo) / conTiempo.Count : null,
                    Disenos = d.Count,
                    DisenosConcluidos = d.Count(x => x.EsConcluido),
                    PresupuestoDisenos = d.Sum(x => x.PresupuestoTotal ?? 0),
                };
            }).OrderByDescending(r => r.Solicitudes + r.Disenos).ToList();
        }

        // ---------- Detalle (verificación registro por registro) ----------

        public KpiDetalleViewModel Detalle(AgendaData a, KpiFiltro f, string vista)
        {
            var vm = new KpiDetalleViewModel { Filtro = f, Vista = vista };
            vm.Titulo = KpiDetalleViewModel.Vistas.FirstOrDefault(v => v.Vista == vista).Nombre ?? "Solicitudes – todas";

            switch (vista)
            {
                case "solicitudes-no-atendidas":
                    TablaSolicitudes(vm, Solicitudes(a, f).Where(s => !s.EsAtendida));
                    vm.Descripcion = "Solicitudes marcadas \"No atendido\" o con la columna \"Atendido/No atendido\" vacía.";
                    break;
                case "solicitudes-fuera-plazo":
                    TablaSolicitudes(vm, Solicitudes(a, f).Where(FueraPlazo).OrderByDescending(s => s.DiasRespuesta));
                    vm.Descripcion = $"Solicitudes cuya primera respuesta tardó más de {_m.PlazoRespuestaDias} días desde el ingreso.";
                    break;
                case "solicitudes-sin-respuesta":
                    TablaSolicitudes(vm, Solicitudes(a, f).Where(s => s.FechaRespuesta == null));
                    vm.Descripcion = "Solicitudes sin ninguna fecha de respuesta al cliente (Sección, Dpto., DDZC ni GD).";
                    break;
                case "solicitudes-inconsistentes":
                    TablaSolicitudes(vm, Solicitudes(a, f).Where(s => !DatosCompletos(s)), conProblemas: true);
                    vm.Descripcion = "Registros a corregir en el Excel: falta un dato obligatorio o la respuesta es anterior al ingreso.";
                    break;
                case "disenos":
                    TablaDisenos(vm, Disenos(a, f));
                    vm.Descripcion = "Todos los proyectos de la hoja DISEÑOS que cumplen los filtros.";
                    break;
                case "disenos-pendientes":
                    TablaDisenos(vm, Disenos(a, f).Where(d => !d.EsConcluido && !d.EsDescartado));
                    vm.Descripcion = "Proyectos vigentes que aún no están diseñados ni enviados a construcción.";
                    break;
                case "disenos-inicio-tardio":
                    TablaDisenos(vm, Disenos(a, f).Where(InicioTardio).OrderByDescending(d => d.DiasHastaInicio));
                    vm.Descripcion = $"Proyectos cuyo diseño empezó más de {_m.InicioDisenoMeta:N0} días después del ingreso.";
                    break;
                case "obras":
                    vm.Columnas = new() { "Fila", "Proyecto", "Parroquia", "Tipo de proyecto", "Plan", "ID", "Diseñador", "Estado", "Memorando", "Fecha memorando", "Presupuesto" };
                    foreach (var o in Obras(a, f))
                    {
                        vm.Filas.Add(new() { o.Fila.ToString(), o.Proyecto, o.Parroquia, o.TipoProyecto, o.Plan, o.Id, o.Disenador, o.Estado, o.Memorando, F(o.FechaMemorando), Dinero(o.PresupuestoTotal) });
                        vm.ClasesFila.Add("");
                    }
                    vm.Descripcion = "Proyectos de la hoja CONSTRUCCIÓN.";
                    break;
                case "inspecciones":
                    vm.Columnas = new() { "Fila", "No.", "Asunto", "Fecha", "Resumen", "Cantón", "Parroquia", "Asistentes" };
                    foreach (var i in Inspecciones(a, f))
                    {
                        vm.Filas.Add(new() { i.Fila.ToString(), i.Numero, i.Asunto, F(i.Fecha), i.Resumen, i.Canton, i.Parroquia, i.Asistentes });
                        vm.ClasesFila.Add("");
                    }
                    vm.Descripcion = "Actividades de la hoja INSPECCIONES Y REUNIONES.";
                    break;
                default:
                    vm.Vista = "solicitudes";
                    TablaSolicitudes(vm, Solicitudes(a, f));
                    vm.Descripcion = "Todas las solicitudes de la hoja SOLICITUDES que cumplen los filtros.";
                    break;
            }
            return vm;
        }

        private void TablaSolicitudes(KpiDetalleViewModel vm, IEnumerable<Solicitud> lista, bool conProblemas = false)
        {
            vm.Columnas = new() { "Fila", "No.", "Requerimiento", "Parroquia", "Estado", "Tipo", "Responsable", "Ingreso", "Respuesta", "Documento", "Días", "Atendido" };
            if (conProblemas) vm.Columnas.Add("Problema");
            foreach (var s in lista)
            {
                var fila = new List<string>
                {
                    s.Fila.ToString(), s.Numero, s.Requerimiento, s.Parroquia, s.Estado, s.TipoRequerimiento, s.Responsable,
                    F(s.FechaIngreso), F(s.FechaRespuesta), s.DocumentoRespuesta, s.DiasRespuesta?.ToString(), s.Atendido,
                };
                if (conProblemas) fila.Add(Problemas(s));
                vm.Filas.Add(fila);
                vm.ClasesFila.Add(s.FechaInconsistente || !s.EsAtendida ? "table-danger"
                    : FueraPlazo(s) ? "table-warning" : "");
            }
        }

        private static string Problemas(Solicitud s)
        {
            var p = new List<string>();
            if (!s.FechaIngreso.HasValue) p.Add("Sin fecha de ingreso");
            if (s.Responsable == null) p.Add("Sin responsable");
            if (s.Estado == null) p.Add("Sin estado");
            if (s.Atendido == null) p.Add("Sin Atendido/No atendido");
            if (s.FechaInconsistente) p.Add("Respuesta anterior al ingreso");
            return string.Join("; ", p);
        }

        private void TablaDisenos(KpiDetalleViewModel vm, IEnumerable<Diseno> lista)
        {
            vm.Columnas = new() { "Fila", "No.", "Mes", "Barrio / sector", "Parroquia", "Estado", "Responsable", "Ingreso", "Inicio diseño", "Días", "Avance", "ID", "Plan de obra", "Est. contractual", "Viviendas", "Presupuesto" };
            foreach (var d in lista)
            {
                vm.Filas.Add(new()
                {
                    d.Fila.ToString(), d.Numero, d.Mes, d.Barrio, d.Parroquia, d.Estado, d.Responsable,
                    F(d.FechaIngreso), F(d.FechaInicioDiseno), d.DiasHastaInicio?.ToString(),
                    d.Avance.HasValue ? $"{d.Avance * 100:N0} %" : null, d.Id, d.PlanObra, d.EstadoContractual,
                    d.Viviendas?.ToString("N0", Es), Dinero(d.PresupuestoTotal),
                });
                vm.ClasesFila.Add(d.EsDescartado ? "table-secondary" : InicioTardio(d) ? "table-warning" : "");
            }
        }

        // ---------- Utilidades ----------

        private static int MesSolicitud(Solicitud s)
        {
            if (s.FechaIngreso.HasValue) return s.FechaIngreso.Value.Month;
            var i = Array.IndexOf(MesesLargos, AgendaExcelReader.Norm(s.Mes));
            return i >= 0 ? i + 1 : 0;
        }

        private static double PromedioDias(List<Solicitud> lista)
        {
            var d = lista?.Where(s => s.DiasRespuesta is >= 0).Select(s => s.DiasRespuesta.Value).ToList();
            return d is { Count: > 0 } ? Math.Round(d.Average(), 1) : 0;
        }

        private static Serie Conteo(IEnumerable<string> valores, int max)
        {
            var g = valores.Select(v => v ?? "(vacío)").GroupBy(v => v).OrderByDescending(x => x.Count()).ToList();
            var top = g.Take(max).ToList();
            var serie = new Serie
            {
                Etiquetas = top.Select(x => x.Key).ToList(),
                Valores = top.Select(x => (double)x.Count()).ToList(),
            };
            var resto = g.Skip(max).Sum(x => x.Count());
            if (resto > 0) { serie.Etiquetas.Add("Otros"); serie.Valores.Add(resto); }
            return serie;
        }

        private static double? Mediana(IEnumerable<double> valores)
        {
            var v = valores.OrderBy(x => x).ToList();
            if (v.Count == 0) return null;
            return v.Count % 2 == 1 ? v[v.Count / 2] : (v[v.Count / 2 - 1] + v[v.Count / 2]) / 2;
        }

        private KpiIndicador Porcentaje(string nombre, int num, int den, double meta, double alerta, bool mayorEsMejor,
            string formula, string detalle, string vista)
        {
            double? pct = den > 0 ? 100.0 * num / den : null;
            return new KpiIndicador
            {
                Nombre = nombre,
                Valor = pct.HasValue ? $"{pct:N1} %" : "—",
                Detalle = $"{num:N0} de {den:N0} · {detalle}",
                Meta = $"{(mayorEsMejor ? "≥" : "≤")} {meta:N0} %",
                Estado = SemaforoValor(pct, meta, alerta, mayorEsMejor),
                Formula = formula,
                VistaDetalle = vista,
            };
        }

        private static Semaforo SemaforoValor(double? valor, double meta, double alerta, bool mayorEsMejor)
        {
            if (!valor.HasValue) return Semaforo.SinDatos;
            var v = valor.Value;
            if (mayorEsMejor) return v >= meta ? Semaforo.Verde : v >= alerta ? Semaforo.Amarillo : Semaforo.Rojo;
            return v <= meta ? Semaforo.Verde : v <= alerta ? Semaforo.Amarillo : Semaforo.Rojo;
        }

        public static string Dinero(double? v) => v.HasValue ? v.Value.ToString("C2", Es) : null;
        private static string F(DateTime? d) => d?.ToString("dd/MM/yyyy");
    }
}
