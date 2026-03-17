using Microsoft.AspNetCore.Mvc;

namespace PingMonitorApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard Monitor";
            return View();
        }

        public IActionResult Devices()
        {
            ViewData["Title"] = "Gestión de Dispositivos";
            return View();
        }
    }
}
