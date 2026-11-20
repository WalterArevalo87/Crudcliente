namespace AgendaEEQ.Models
{
    /// <summary>Metas y umbrales del semáforo. Se configuran en appsettings.json, sección "KpiMetas".</summary>
    public class KpiMetas
    {
        public double AtencionMeta { get; set; } = 95;
        public double AtencionAlerta { get; set; } = 85;

        public int PlazoRespuestaDias { get; set; } = 15;
        public double TiempoRespuestaMeta { get; set; } = 15;
        public double TiempoRespuestaAlerta { get; set; } = 30;
        public double DentroPlazoMeta { get; set; } = 80;
        public double DentroPlazoAlerta { get; set; } = 60;

        public double DisenosConcluidosMeta { get; set; } = 70;
        public double DisenosConcluidosAlerta { get; set; } = 50;
        public double AvanceMeta { get; set; } = 80;
        public double AvanceAlerta { get; set; } = 60;
        public double InicioDisenoMeta { get; set; } = 30;
        public double InicioDisenoAlerta { get; set; } = 60;

        public double CalidadDatosMeta { get; set; } = 90;
        public double CalidadDatosAlerta { get; set; } = 75;
    }

    public enum Semaforo { Verde, Amarillo, Rojo, Info, SinDatos }

    public class KpiIndicador
    {
        public string Nombre { get; set; }
        public string Valor { get; set; }
        public string Detalle { get; set; }
        public string Meta { get; set; }
        public Semaforo Estado { get; set; }
        public string Formula { get; set; }
        /// <summary>Vista de detalle para verificar el indicador registro por registro.</summary>
        public string VistaDetalle { get; set; }
    }

    public class KpiFiltro
    {
        public int? Anio { get; set; }
        public string Responsable { get; set; }
        public string Parroquia { get; set; }

        public Dictionary<string, string> RouteValues(string vista = null)
        {
            var d = new Dictionary<string, string>();
            if (Anio.HasValue) d["anio"] = Anio.Value.ToString();
            if (!string.IsNullOrEmpty(Responsable)) d["responsable"] = Responsable;
            if (!string.IsNullOrEmpty(Parroquia)) d["parroquia"] = Parroquia;
            if (vista != null) d["vista"] = vista;
            return d;
        }
    }

    public class Serie
    {
        public List<string> Etiquetas { get; set; } = new();
        public List<double> Valores { get; set; } = new();
    }

    public class FilaResponsable
    {
        public string Responsable { get; set; }
        public int Solicitudes { get; set; }
        public int Atendidas { get; set; }
        public double? PorcentajeAtendidas { get; set; }
        public double? DiasPromedioRespuesta { get; set; }
        public double? PorcentajeDentroPlazo { get; set; }
        public int Disenos { get; set; }
        public int DisenosConcluidos { get; set; }
        public double PresupuestoDisenos { get; set; }
    }

    public class KpiDashboardViewModel
    {
        public AgendaData Agenda { get; set; }
        public KpiFiltro Filtro { get; set; }
        public KpiMetas Metas { get; set; }
        public List<int> Anios { get; set; } = new();
        public List<string> Responsables { get; set; } = new();
        public List<string> Parroquias { get; set; } = new();

        public List<KpiIndicador> IndicadoresSolicitudes { get; set; } = new();
        public List<KpiIndicador> IndicadoresDisenos { get; set; } = new();
        public List<KpiIndicador> IndicadoresGestion { get; set; } = new();

        public Serie SolicitudesPorPeriodo { get; set; }
        public string EtiquetaPeriodo { get; set; }
        public Serie TiempoRespuestaPorPeriodo { get; set; }
        public Serie SolicitudesPorEstado { get; set; }
        public Serie SolicitudesPorTipo { get; set; }
        public Serie DisenosPorEstado { get; set; }
        public Serie PresupuestoPorPlan { get; set; }
        public Serie DisenosPorEstadoContractual { get; set; }
        public Serie InspeccionesPorMes { get; set; }
        public Serie ObrasPorTipo { get; set; }

        public List<FilaResponsable> PorResponsable { get; set; } = new();
    }

    public class KpiDetalleViewModel
    {
        public KpiFiltro Filtro { get; set; }
        public string Vista { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public List<string> Columnas { get; set; } = new();
        public List<List<string>> Filas { get; set; } = new();
        /// <summary>Clase CSS opcional por fila (para resaltar registros fuera de meta).</summary>
        public List<string> ClasesFila { get; set; } = new();
        public static readonly (string Vista, string Nombre)[] Vistas =
        {
            ("solicitudes", "Solicitudes – todas"),
            ("solicitudes-no-atendidas", "Solicitudes no atendidas / sin estado"),
            ("solicitudes-fuera-plazo", "Solicitudes respondidas fuera de plazo"),
            ("solicitudes-sin-respuesta", "Solicitudes sin fecha de respuesta"),
            ("solicitudes-inconsistentes", "Solicitudes con datos incompletos o inconsistentes"),
            ("disenos", "Diseños – todos"),
            ("disenos-pendientes", "Diseños pendientes"),
            ("disenos-inicio-tardio", "Diseños con inicio tardío"),
            ("obras", "Obras construidas / en construcción"),
            ("inspecciones", "Inspecciones y reuniones"),
        };
    }
}
