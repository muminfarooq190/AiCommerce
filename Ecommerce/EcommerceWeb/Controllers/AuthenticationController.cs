using EcommerceWeb.Entities;
using EcommerceWeb.Extentions;
using EcommerceWeb.Models;
using EcommerceWeb.Services.Contarcts;
using EcommerceWeb.Utilities.ApiResult;
using EcommerceWeb.Utilities.ApiResult.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Security.Claims;

namespace EcommerceWeb.Controllers;

[Authorize]
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

		var tempprefix = Request.Host.Host.Split('.').FirstOrDefault()?.ToLower() ?? string.Empty;

		TenantConfigEntity? tenantConfig = await appDbContext
										.TenantConfigs
									.FirstOrDefaultAsync(x => x.TempUrlPrefix == tempprefix || x.Url == Request.Host.Host.ToLower());

		if (tenantConfig == null)
		{
			ModelState.AddModelError(nameof(request.Email), "something went wrong!");
			logger.LogError("Tenant configuration not found for prefix: {Prefix}", tempprefix);
			return View(request);
		}

		var userLoginRequest = new UserLoginRequest
		{
			TenantId = tenantConfig.TenantId,
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
			new Claim(ClaimTypes.GroupSid, tenantConfig.TenantId.ToString()),
			new Claim("IsPrimaryTanent", user.IsPrimaryTanent.ToString()),
			new Claim("Token", user.Token.ToString()),
		};

		if (!user.IsPrimaryTanent)
		{
			//add user permisstons to claims
		}

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
