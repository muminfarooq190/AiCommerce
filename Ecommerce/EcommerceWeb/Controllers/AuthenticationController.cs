using EcommerceWeb.Entities;
using EcommerceWeb.Extensions;
using EcommerceWeb.Models;
using EcommerceWeb.Services.Contarcts;
using EcommerceWeb.Utilities.ApiResult;
using EcommerceWeb.Utilities.ApiResult.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Security.Claims;

namespace EcommerceWeb.Controllers;

public class AuthenticationController(IApiClient apiClient, ILogger<AuthenticationController> logger, AppDbContext appDbContext) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(LoginViewModel request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var userLoginRequest = new UserLoginRequest
        {
            TenantId = Guid.NewGuid(),
            Email = request.Email,
            Password = request.Password
        };

        var apiResponce = await apiClient.PostAsync<UserLoginRequest, UserLoginResponse>(
            Endpoints.Authentication.Login,
            userLoginRequest
        );

        ModelState.AddApiResult(apiResponce);

        if (apiResponce.ResultType != ResultType.Success)
        {
            logger.LogError("Login failed: {Errors}", apiResponce.Errors.ToString());
            return View(request);
        }

        var user = apiResponce.Data;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            // i will add User permisstons here
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        return Redirect("Portal");
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

        ApiResult<UserRegisterResponse> response = await apiClient.PostAsync<UserRegisterRequest, UserRegisterResponse>(Endpoints.Authentication.RegisterTenant, modelReqister);
        ModelState.AddApiResult(response);

        if (response.ResultType != ResultType.Success)
        {
            logger.LogError("Registration failed: {Errors}", response.Errors.ToString());
            return View(model);
        }

        var newtc = new TenantConfigEntity
        {
            TenantId = response.Data.TenantId,
            TempUrlPrefix = response.Data.CompanyName.ToLower().Replace(" ", ""),

        };

        await appDbContext.TenantConfigs.AddAsync(newtc);

        await appDbContext.SaveChangesAsync();

        string tempUrl = $"{Request.Scheme}://{newtc.TempUrlPrefix}.{Request.Host}/Authentication?message=we have send you link via email please verify";

        return Redirect(tempUrl);
    }
}
