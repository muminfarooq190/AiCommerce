using EcommerceWeb.Entities;
using EcommerceWeb.Models;
using EcommerceWeb.Patterns.Results;
using EcommerceWeb.Services.Contarcts;
using Microsoft.AspNetCore.Mvc;
using Models.RequestModels;
using Models.ResponseModels;

namespace EcommerceWeb.Controllers;

public class AuthenticationController(IApiClient apiClient, ILogger<AuthenticationController> logger,AppDbContext appDbContext) : Controller
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

        UserRegisterRequest modelReqister = new UserRegisterRequest
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Password = model.Password,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber,
            CompanyName = model.CompanyName
        };

        Result<UserRegisterResponse> response = await apiClient.PostAsync<UserRegisterRequest, UserRegisterResponse>(Endpoints.AuthenticationEndpoints.RegisterTenant, modelReqister);

        if (!response.IsValid)
        {
            logger.LogError("Registration failed: {Errors}", string.Join(",", response.Errors));
            return View(model);
        }
        var newtc = new TenantConfigEntity
        {
            TenantId = response.Value.TenantId,
            TempUrlPrefix = response.Value.CompanyName.ToLower().Replace(" ", ""),

        };

        await appDbContext.TenantConfigs.AddAsync(newtc);

        await appDbContext.SaveChangesAsync();

        string tempUrl = $"{Request.Scheme}://{newtc.TempUrlPrefix}.{Request.Host}/Authentication?message=we have send you link via email please verify";

        return Redirect(tempUrl);
    }
}
