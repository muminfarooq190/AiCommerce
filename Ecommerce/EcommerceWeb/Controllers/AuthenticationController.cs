using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
