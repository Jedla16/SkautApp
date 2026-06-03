using Microsoft.AspNetCore.Mvc;

namespace SkautApp.Controllers
{
    [Route("")]
    public class UvodController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Uvod.cshtml");
        }
    }
}
