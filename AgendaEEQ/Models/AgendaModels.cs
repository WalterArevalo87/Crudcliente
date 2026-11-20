namespace AgendaEEQ.Models
{
    /// <summary>Fila de la hoja "SOLICITUDES".</summary>
    public class Solicitud
    {
        public int Fila { get; set; }
        public string Numero { get; set; }
        public string Mes { get; set; }
        public int? Anio { get; set; }
        public string Requerimiento { get; set; }
        public string Solicitante { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string Tramite { get; set; }
        public string Canton { get; set; }
        public string Parroquia { get; set; }
        public string Estado { get; set; }
        public string TipoRequerimiento { get; set; }
        public string Responsable { get; set; }
        public string Atendido { get; set; }
        public string Observaciones { get; set; }

        /// <summary>Primera fecha de respuesta al cliente (Sección, Dpto., DDZC o GD).</summary>
        public DateTime? FechaRespuesta { get; set; }
        public string DocumentoRespuesta { get; set; }

        public bool EsAtendida => string.Equals(Atendido, "Atendido", StringComparison.OrdinalIgnoreCase);
        public bool EsNoAtendida => string.Equals(Atendido, "No atendido", StringComparison.OrdinalIgnoreCase);

        /// <summary>Días calendario entre el ingreso y la primera respuesta (null si falta alguna fecha).</summary>
        public int? DiasRespuesta =>
            FechaIngreso.HasValue && FechaRespuesta.HasValue
                ? (int)(FechaRespuesta.Value.Date - FechaIngreso.Value.Date).TotalDays
                : null;

        /// <summary>Respuesta con fecha anterior al ingreso: dato inconsistente.</summary>
        public bool FechaInconsistente => DiasRespuesta < 0;
    }

    /// <summary>Fila de la hoja "DISEÑOS".</summary>
    public class Diseno
    {
        public int Fila { get; set; }
        public string Numero { get; set; }
        public string Mes { get; set; }
        public int? Anio { get; set; }
        public string Barrio { get; set; }
        public string Canton { get; set; }
        public string Parroquia { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaInicioDiseno { get; set; }
        public string Responsable { get; set; }
        public string NombreProyecto { get; set; }
        public double? Avance { get; set; }
        public string Tramite { get; set; }
        public string Id { get; set; }
        public double? Viviendas { get; set; }
        public double? KmMedioVoltaje { get; set; }
        public double? KmBajoVoltaje { get; set; }
        public double? KmAlumbrado { get; set; }
        public double? Luminarias { get; set; }
        public double? PresupuestoTotal { get; set; }
        public string PlanObra { get; set; }
        public string EstadoContractual { get; set; }

        public bool EsConcluido => Estado is "Diseñado" or "Enviado a construcción";
        public bool EsEnviadoConstruccion => Estado == "Enviado a construcción";
        public bool EsDescartado => Estado is "No viable" or "Sin trazado vial";

        public int? DiasHastaInicio =>
            FechaIngreso.HasValue && FechaInicioDiseno.HasValue
                ? (int)(FechaInicioDiseno.Value.Date - FechaIngreso.Value.Date).TotalDays
                : null;
    }

    /// <summary>Fila de la hoja "CONSTRUCCIÓN".</summary>
    public class Obra
    {
        public int Fila { get; set; }
        public string Proyecto { get; set; }
        public string Canton { get; set; }
        public string Parroquia { get; set; }
        public string Disenador { get; set; }
        public string Estado { get; set; }
        public string TipoProyecto { get; set; }
        public string Plan { get; set; }
        public string Id { get; set; }
        public double? PresupuestoTotal { get; set; }
        public string Memorando { get; set; }
        public DateTime? FechaMemorando { get; set; }
        public int? Anio => FechaMemorando?.Year;
    }

    /// <summary>Fila de la hoja "INSPECCIONES Y REUNIONES".</summary>
    public class Inspeccion
    {
        public int Fila { get; set; }
        public string Numero { get; set; }
        public string Asunto { get; set; }
        public DateTime? Fecha { get; set; }
        public int? Anio => Fecha?.Year;
        public string Resumen { get; set; }
        public string Canton { get; set; }
        public string Parroquia { get; set; }
        public string Asistentes { get; set; }
    }

    public class AgendaData
    {
        public string NombreArchivo { get; set; }
        public DateTime FechaCarga { get; set; }
        public List<Solicitud> Solicitudes { get; set; } = new();
        public List<Diseno> Disenos { get; set; } = new();
        public List<Obra> Obras { get; set; } = new();
        public List<Inspeccion> Inspecciones { get; set; } = new();
        /// <summary>Hojas esperadas que no se encontraron o columnas faltantes.</summary>
        public List<string> Advertencias { get; set; } = new();
    }
}
