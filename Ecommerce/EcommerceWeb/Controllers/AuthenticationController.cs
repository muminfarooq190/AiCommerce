using EcommerceWeb.Entities;
using EcommerceWeb.Extensions;
using EcommerceWeb.Models;
using EcommerceWeb.Services.Contarcts;
using EcommerceWeb.Utilities.ApiResult;
using EcommerceWeb.Utilities.ApiResult.Enums;
using Microsoft.AspNetCore.Mvc;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;

namespace EcommerceWeb.Controllers;

public class AuthenticationController(IApiClient apiClient, ILogger<AuthenticationController> logger, AppDbContext appDbContext) : Controller
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

        ApiResult<UserRegisterResponse> response = await apiClient.PostAsync<UserRegisterRequest, UserRegisterResponse>(Endpoints.AuthenticationEndpoints.RegisterTenant, modelReqister);
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
