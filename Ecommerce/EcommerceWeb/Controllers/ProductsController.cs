using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
