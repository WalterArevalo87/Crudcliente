using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Cliente.Models.Kpi;
using ClosedXML.Excel;

namespace Cliente.Services
{
    /// <summary>
    /// Lee el libro "AGENDA DE TRABAJO" y lo convierte en listas tipadas.
    /// Las columnas se ubican por el texto del encabezado (no por posición),
    /// así el sistema sigue funcionando si se insertan o mueven columnas.
    /// </summary>
    public class AgendaExcelReader
    {
        public AgendaData Leer(Stream stream, string nombreArchivo)
        {
            using var wb = new XLWorkbook(stream);
            var data = new AgendaData { NombreArchivo = nombreArchivo, FechaCarga = DateTime.Now };

            LeerHoja(wb, "SOLICITUDES", "No.", data, (t, r, f) => data.Solicitudes.Add(LeerSolicitud(t, r, f)),
                t => t.Texto("No.") != null || t.Texto("Requerimiento") != null);
            LeerHoja(wb, "DISEÑOS", "No.", data, (t, r, f) => data.Disenos.Add(LeerDiseno(t, r, f)),
                t => t.Texto("Nombre del barrio / sector") != null &&
                     (t.Texto("No.") != null || t.Texto("Mes") != null || t.Texto("Estado") != null));
            LeerHoja(wb, "CONSTRUCCIÓN", "Proyecto", data, (t, r, f) => data.Obras.Add(LeerObra(t, r, f)),
                t => t.Texto("Proyecto") != null);
            LeerHoja(wb, "INSPECCIONES Y REUNIONES", "ASUNTO", data, (t, r, f) => data.Inspecciones.Add(LeerInspeccion(t, r, f)),
                t => t.Texto("No.") != null && t.Texto("ASUNTO") != null);

            NormalizarResponsables(data);
            return data;
        }

        private delegate void Lector(Fila t, IXLRow row, int numFila);

        private static void LeerHoja(XLWorkbook wb, string nombreHoja, string encabezadoClave, AgendaData data,
            Lector lector, Func<Fila, bool> esFilaValida)
        {
            var ws = wb.Worksheets.FirstOrDefault(w => Norm(w.Name) == Norm(nombreHoja));
            if (ws == null)
            {
                data.Advertencias.Add($"No se encontró la hoja \"{nombreHoja}\".");
                return;
            }

            // El encabezado puede no estar en la fila 1 (p. ej. INSPECCIONES tiene un título arriba).
            int filaEnc = 0;
            Dictionary<string, int> columnas = null;
            for (int r = 1; r <= 6 && columnas == null; r++)
            {
                var cols = new Dictionary<string, int>();
                foreach (var c in ws.Row(r).CellsUsed())
                {
                    var key = Norm(Valor(c).ToString());
                    if (key.Length > 0 && !cols.ContainsKey(key)) cols[key] = c.Address.ColumnNumber;
                }
                if (cols.ContainsKey(Norm(encabezadoClave))) { columnas = cols; filaEnc = r; }
            }
            if (columnas == null)
            {
                data.Advertencias.Add($"La hoja \"{nombreHoja}\" no tiene la columna \"{encabezadoClave}\" en las primeras filas.");
                return;
            }

            var ultima = ws.LastRowUsed()?.RowNumber() ?? filaEnc;
            for (int r = filaEnc + 1; r <= ultima; r++)
            {
                var row = ws.Row(r);
                var fila = new Fila(row, columnas);
                if (!esFilaValida(fila)) continue;
                lector(fila, row, r);
            }
        }

        private static Solicitud LeerSolicitud(Fila t, IXLRow _, int r)
        {
            var s = new Solicitud
            {
                Fila = r,
                Numero = t.Texto("No."),
                Mes = t.Texto("Mes"),
                Requerimiento = t.Texto("Requerimiento"),
                Solicitante = t.Texto("Persona que solicita"),
                FechaIngreso = t.Fecha("Fecha de Ingreso a la Sección"),
                Tramite = t.Texto("Trámite"),
                Canton = Mayus(t.Texto("Cantón")),
                Parroquia = Mayus(t.Texto("Parroquia")),
                Estado = t.Texto("Estado"),
                TipoRequerimiento = t.Texto("Tipo de Requerimiento"),
                Responsable = t.Texto("Responsable"),
                Atendido = t.Texto("Atendido/No atendido"),
                Observaciones = t.Texto("Observaciones"),
            };

            foreach (var nivel in new[] { "Sección", "Dpto.", "DDZC", "GD" })
            {
                var f = t.Fecha($"Fecha Respuesta al Cliente ({nivel})");
                if (f.HasValue && (!s.FechaRespuesta.HasValue || f < s.FechaRespuesta))
                {
                    s.FechaRespuesta = f;
                    s.DocumentoRespuesta = t.Texto($"Documento Respuesta al Cliente ({nivel})");
                }
            }

            s.Anio = s.FechaIngreso?.Year ?? AnioDeTexto(s.Numero);
            return s;
        }

        private static Diseno LeerDiseno(Fila t, IXLRow _, int r)
        {
            var d = new Diseno
            {
                Fila = r,
                Numero = t.Texto("No."),
                Mes = t.Texto("Mes"),
                Barrio = t.Texto("Nombre del barrio / sector"),
                Canton = Mayus(t.Texto("Cantón")),
                Parroquia = Mayus(t.Texto("Parroquia")),
                Estado = t.Texto("Estado"),
                FechaIngreso = t.Fecha("Fecha de Ingreso a la Sección"),
                FechaInicioDiseno = t.Fecha("Fecha de Inicio de diseño"),
                Responsable = t.Texto("Responsable"),
                NombreProyecto = t.Texto("Nombre del Proyecto"),
                Avance = t.Numero("Avance %"),
                Tramite = t.Texto("Trámite"),
                Id = t.Texto("ID"),
                Viviendas = t.Numero("Vtot (#)"),
                KmMedioVoltaje = t.Numero("MV (km)"),
                KmBajoVoltaje = t.Numero("BV (km)"),
                KmAlumbrado = t.Numero("AP (km)"),
                Luminarias = t.Numero("Luminarias (#)"),
                PresupuestoTotal = t.Numero("Presupuesto Total"),
                PlanObra = t.Texto("Plan de Obra"),
                EstadoContractual = t.Texto("Estado contractual"),
            };
            if (d.Responsable == "Ninguno") d.Responsable = null;
            d.Anio = AnioDeTexto(d.Mes) ?? d.FechaIngreso?.Year;
            return d;
        }

        private static Obra LeerObra(Fila t, IXLRow _, int r)
        {
            var o = new Obra
            {
                Fila = r,
                Proyecto = t.Texto("Proyecto"),
                Canton = Mayus(t.Texto("Cantón")),
                Parroquia = Mayus(t.Texto("Parroquia")),
                Disenador = t.Texto("Diseñador"),
                Estado = t.Texto("Estado"),
                TipoProyecto = t.Texto("Tipo de Proyecto"),
                Plan = t.Texto("Plan"),
                Id = t.Texto("ID"),
                PresupuestoTotal = t.Numero("Presupuesto Total"),
                Memorando = t.Texto("Memortando envío a construcción") ?? t.Texto("Memorando envío a construcción"),
                FechaMemorando = t.Fecha("Fecha memorando envío a construcción"),
            };
            if (o.Disenador == "#N/A") o.Disenador = null;
            return o;
        }

        private static Inspeccion LeerInspeccion(Fila t, IXLRow _, int r) => new()
        {
            Fila = r,
            Numero = t.Texto("No."),
            Asunto = t.Texto("ASUNTO"),
            Fecha = t.Fecha("FECHA"),
            Resumen = t.Texto("RESUMEN DE TEMAS TRATADOS"),
            Canton = Mayus(t.Texto("CANTÓN")),
            Parroquia = Mayus(t.Texto("PARROQUIA")),
            Asistentes = t.Texto("ASISTENTES"),
        };

        /// <summary>
        /// Unifica variantes del mismo nombre ("Ing. Carlos Sanchez" / "Carlos Sánchez").
        /// Se conserva la variante con tildes.
        /// </summary>
        private static void NormalizarResponsables(AgendaData data)
        {
            var nombres = data.Solicitudes.Select(s => s.Responsable)
                .Concat(data.Disenos.Select(d => d.Responsable))
                .Concat(data.Obras.Select(o => o.Disenador))
                .Where(n => n != null)
                .Select(QuitarTitulo)
                .Distinct()
                .GroupBy(ClaveNombre)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(n => n.Count(ch => ch > 127)).First());

            string Canon(string n) => n == null ? null : nombres.GetValueOrDefault(ClaveNombre(QuitarTitulo(n)), n);

            foreach (var s in data.Solicitudes) s.Responsable = Canon(s.Responsable);
            foreach (var d in data.Disenos) d.Responsable = Canon(d.Responsable);
            foreach (var o in data.Obras) o.Disenador = Canon(o.Disenador);
        }

        public static string ClaveNombre(string nombre) => Norm(QuitarTitulo(nombre ?? ""));

        private static string QuitarTitulo(string n) =>
            Regex.Replace(n, @"^(\s*Ing\.?\s*)+", "", RegexOptions.IgnoreCase).Trim();

        private static int? AnioDeTexto(string texto)
        {
            if (texto == null) return null;
            var m = Regex.Match(texto, @"(19|20)\d{2}");
            return m.Success ? int.Parse(m.Value) : null;
        }

        private static string Mayus(string s) => s?.Trim().ToUpper(new CultureInfo("es-EC"));

        /// <summary>Normaliza un texto para compararlo: minúsculas, sin tildes, espacios simples.</summary>
        public static string Norm(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var d = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in d)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(ch);
            return Regex.Replace(sb.ToString(), @"\s+", " ");
        }

        // Para celdas con fórmula se usa el valor guardado en el archivo (no se recalcula).
        private static XLCellValue Valor(IXLCell c)
        {
            try { return c.HasFormula ? c.CachedValue : c.Value; }
            catch { return Blank.Value; }
        }

        /// <summary>Acceso a las celdas de una fila por nombre de encabezado.</summary>
        private class Fila
        {
            private readonly IXLRow _row;
            private readonly Dictionary<string, int> _cols;

            public Fila(IXLRow row, Dictionary<string, int> cols) { _row = row; _cols = cols; }

            private XLCellValue? Celda(string encabezado) =>
                _cols.TryGetValue(Norm(encabezado), out var col) ? Valor(_row.Cell(col)) : (XLCellValue?)null;

            public string Texto(string encabezado)
            {
                var v = Celda(encabezado);
                if (v == null) return null;
                var val = v.Value;
                string s = val.Type switch
                {
                    XLDataType.Blank or XLDataType.Error => null,
                    XLDataType.Number => val.GetNumber().ToString(CultureInfo.InvariantCulture),
                    XLDataType.DateTime => val.GetDateTime().ToString("yyyy-MM-dd"),
                    _ => val.ToString(),
                };
                s = s?.Trim();
                return string.IsNullOrEmpty(s) || s == "-" ? null : s;
            }

            public double? Numero(string encabezado)
            {
                var v = Celda(encabezado);
                if (v == null) return null;
                var val = v.Value;
                if (val.IsNumber) return val.GetNumber();
                if (val.IsText && double.TryParse(val.GetText().Replace(",", "."), NumberStyles.Any,
                        CultureInfo.InvariantCulture, out var n)) return n;
                return null;
            }

            public DateTime? Fecha(string encabezado)
            {
                var v = Celda(encabezado);
                if (v == null) return null;
                var val = v.Value;
                if (val.IsDateTime) return val.GetDateTime();
                // Fechas guardadas como número de serie (1990-2100).
                if (val.IsNumber && val.GetNumber() is > 32874 and < 73051) return DateTime.FromOADate(val.GetNumber());
                if (val.IsText && DateTime.TryParseExact(val.GetText().Trim(),
                        new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" },
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var f)) return f;
                return null;
            }
        }
    }
}
