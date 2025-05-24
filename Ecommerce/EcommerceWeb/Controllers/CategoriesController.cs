using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
