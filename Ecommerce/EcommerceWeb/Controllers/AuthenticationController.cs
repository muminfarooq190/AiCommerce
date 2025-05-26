using EcommerceWeb.Models;
using EcommerceWeb.Services.Contarcts;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers;

public class AuthenticationController(IApiClient apiClient) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var res = await apiClient.PostAsync<RegisterViewModel, RegisterViewModel>("api/Authentication/RegisterTenant", model);

        if(res.IsValid)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }
}
