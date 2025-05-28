using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Areas.Portal.Controllers
{
    [Area("Portal")]
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
