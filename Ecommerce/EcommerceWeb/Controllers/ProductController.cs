using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
