using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Areas.Portal.Controllers;

[Area("Portal")]
public class CategoryController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
