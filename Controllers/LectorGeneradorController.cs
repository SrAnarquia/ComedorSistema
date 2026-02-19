using ComedorSistema.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ComedorSistema.Controllers
{
    public class LectorGeneradorController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Mis solicitudes
        }

        public IActionResult Lectura()
        {
            return View(); // Cámara abierta
        }

        [HttpPost]
        public IActionResult RegistrarLectura([FromBody] LecturaQrVm vm)
        {
            if (string.IsNullOrWhiteSpace(vm.CodigoQr))
                return BadRequest("QR inválido");

            // 🔹 AQUÍ IRÁ EL GUARDADO A BD (Entity Framework)
            // _context.Lecturas.Add(...)
            // _context.SaveChanges();

            return Ok(new { mensaje = "QR leído correctamente", data = vm });
        }

        public IActionResult Crear()
        {
            return View(); // Generar QR (futuro)
        }
    }
}
