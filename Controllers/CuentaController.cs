using Microsoft.AspNetCore.Mvc;

namespace ComedorSistema.Controllers
{
    public class CuentaController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
