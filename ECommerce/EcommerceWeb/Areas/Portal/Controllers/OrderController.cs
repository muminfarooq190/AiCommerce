using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Areas.Portal.Controllers
{
    [Area("Portal")]
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
