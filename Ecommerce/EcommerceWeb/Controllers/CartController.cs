using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
