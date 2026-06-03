using Microsoft.AspNetCore.Mvc;

namespace SkautApp.Controllers
{
    [Route("zmena-hesla")]
    public class ZmenaHeslaController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/ZmenaHesla.cshtml");
        }
    }
}