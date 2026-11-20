using System.Globalization;
using AgendaEEQ.Models;
using AgendaEEQ.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgendaEEQ.Controllers
{
    /// <summary>Tablero de KPI de la Agenda de Trabajo Z4.</summary>
    public class KpiController : Controller
    {
        private const long TamanoMaximo = 50 * 1024 * 1024;
        private readonly AgendaStore _store;
        private readonly KpiCalculator _calc;

        public KpiController(AgendaStore store, KpiCalculator calc)
        {
            _store = store;
            _calc = calc;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Formato de números, moneda y fechas de Ecuador solo para este módulo.
            CultureInfo.CurrentCulture = new CultureInfo("es-EC");
            base.OnActionExecuting(context);
        }

        public IActionResult Index(KpiFiltro filtro)
        {
            var agenda = _store.Actual;
            if (agenda == null) return RedirectToAction(nameof(Cargar));
            return View(_calc.Tablero(agenda, filtro));
        }

        public IActionResult Detalle(KpiFiltro filtro, string vista)
        {
            var agenda = _store.Actual;
            if (agenda == null) return RedirectToAction(nameof(Cargar));
            return View(_calc.Detalle(agenda, filtro, vista));
        }

        [HttpGet]
        public IActionResult Cargar() => View(_store.Actual);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(TamanoMaximo)]
        [RequestFormLimits(MultipartBodyLengthLimit = TamanoMaximo)]
        public IActionResult Cargar(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                ModelState.AddModelError("", "Seleccione el archivo Excel de la agenda de trabajo.");
                return View(_store.Actual);
            }
            if (!archivo.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
                !archivo.FileName.EndsWith(".xlsm", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "El archivo debe ser un libro de Excel (.xlsx o .xlsm).");
                return View(_store.Actual);
            }

            AgendaData data;
            try
            {
                using var s = archivo.OpenReadStream();
                data = _store.Cargar(s, Path.GetFileName(archivo.FileName));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"No se pudo leer el archivo: {ex.Message}");
                return View(_store.Actual);
            }

            TempData["Mensaje"] = $"Archivo cargado: {data.Solicitudes.Count} solicitudes, {data.Disenos.Count} diseños, " +
                                  $"{data.Obras.Count} obras y {data.Inspecciones.Count} inspecciones/reuniones.";
            return RedirectToAction(nameof(Index));
        }
    }
}
