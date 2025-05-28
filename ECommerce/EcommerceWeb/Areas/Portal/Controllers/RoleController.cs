using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Areas.Portal.Controllers
{
    [Area("Portal")]
    public class RoleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
