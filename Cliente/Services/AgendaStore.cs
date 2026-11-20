using Cliente.Models.Kpi;

namespace Cliente.Services
{
    /// <summary>
    /// Mantiene en memoria la última agenda cargada y guarda una copia del Excel en App_Data,
    /// de modo que los KPI siguen disponibles después de reiniciar la aplicación.
    /// </summary>
    public class AgendaStore
    {
        private readonly AgendaExcelReader _reader;
        private readonly ILogger<AgendaStore> _logger;
        private readonly string _rutaArchivo;
        private readonly string _rutaNombre;
        private readonly object _lock = new();
        private AgendaData _data;
        private bool _intentoCarga;

        public AgendaStore(AgendaExcelReader reader, IWebHostEnvironment env, ILogger<AgendaStore> logger)
        {
            _reader = reader;
            _logger = logger;
            var dir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dir);
            _rutaArchivo = Path.Combine(dir, "agenda.xlsx");
            _rutaNombre = Path.Combine(dir, "agenda.nombre.txt");
        }

        public AgendaData Actual
        {
            get
            {
                lock (_lock)
                {
                    if (_data == null && !_intentoCarga)
                    {
                        _intentoCarga = true;
                        CargarDesdeDisco();
                    }
                    return _data;
                }
            }
        }

        /// <summary>Lee el archivo subido; si es válido reemplaza la agenda actual y lo guarda en disco.</summary>
        public AgendaData Cargar(Stream archivo, string nombre)
        {
            using var ms = new MemoryStream();
            archivo.CopyTo(ms);
            ms.Position = 0;
            var data = _reader.Leer(ms, nombre); // lanza excepción si el archivo no es un Excel válido

            lock (_lock)
            {
                File.WriteAllBytes(_rutaArchivo, ms.ToArray());
                File.WriteAllText(_rutaNombre, nombre);
                _data = data;
                _intentoCarga = true;
            }
            return data;
        }

        private void CargarDesdeDisco()
        {
            if (!File.Exists(_rutaArchivo)) return;
            try
            {
                using var fs = File.OpenRead(_rutaArchivo);
                var nombre = File.Exists(_rutaNombre) ? File.ReadAllText(_rutaNombre) : "agenda.xlsx";
                _data = _reader.Leer(fs, nombre);
                _data.FechaCarga = File.GetLastWriteTime(_rutaArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo leer la agenda guardada en {Ruta}", _rutaArchivo);
            }
        }
    }
}
