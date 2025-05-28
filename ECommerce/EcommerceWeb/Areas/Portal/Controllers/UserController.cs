using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Areas.Portal.Controllers
{
    [Area("Portal")]
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
