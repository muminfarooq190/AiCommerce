using EcommerceWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers;

public class AuthenticationController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Index(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(ModelState);
        }

        // TODO: Add your logic to create the user, hash the password, etc.

        // Simulate success for now
        return View();
    }
}
