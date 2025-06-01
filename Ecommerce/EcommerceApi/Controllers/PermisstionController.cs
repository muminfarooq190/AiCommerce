using EcommerceApi.Attributes;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;

namespace EcommerceApi.Controllers;


/// <summary>
/// Handles permission management endpoints for users and tenants.
/// </summary>
/// <remarks>
/// <b>Error Messages Returned by This Controller:</b> <br></br>
/// <list type="bullet">
///   <item>Invalid permission requested.</item><br></br>
///   <item>User does not exist.</item><br></br>
///   <item>User already has this permission.</item><br></br>
///   <item>You cannot remove permissions from the primary tenant user.</item><br></br>
///   <item>User does not have this permission.</item><br></br>
///   <item>TenantId is missing.</item><br></br>
/// </list>
/// <b>HTTP Status Codes Used:</b>
/// <list type="bullet">
///   <item>200 OK</item>
///   <item>400 BadRequest</item>
///   <item>403 Forbidden</item>
///   <item>409 Conflict</item>
/// </list>
/// </remarks>
[Produces("application/json")]
[ApiController]
public class PermisstionController(AppDbContext appDbContext, IUserProvider userProvider) : ControllerBase
{

	/// <summary>
	/// Gets all permissions as a JSON object.
	/// </summary>
	/// <response code="200">Returns all permissions in JSON format.</response>
	[Produces("application/json")]
	[HttpGet(Endpoints.Permisstion.GetAllPermissionsAsJson)]
	public IActionResult GetAllPermissionsAsJson()
	{
		return Ok(FeatureFactory.GetJsonRepresentation());
	}

	/// <summary>
	/// Gets a flattened list of all permissions.
	/// </summary>
	/// <response code="200">Returns a list of all permissions.</response>
	[Produces("application/json")]
	[HttpGet(Endpoints.Permisstion.GetAllPermissions)]
	public IActionResult GetAllPermissions()
	{
		return Ok(FeatureFactory.GetFlattenedPermissionList());
	}

	/// <summary>
	/// Gets a flattened list of all permissions.
	/// </summary>
	/// <response code="200">Returns a list of all permissions.</response>
	[Produces("application/json")]
	[HttpGet(Endpoints.Permisstion.GetPermissions)]
	[AppAuthorize]
	public async Task<ActionResult<List<GetPermisstionResponse>>> GetPermissions()
	{
		List<GetPermisstionResponse> permisstions = await appDbContext
											.UserPermissions
											.Where(p => p.UserId == userProvider.UserId)
											.Select(p => new GetPermisstionResponse
											{
												PermissionId = p.PermissionId,
												Name = p.Name,
												CreatedAt = p.CreatedAt,
												UpdatedAt = p.UpdatedAt
											})
											.ToListAsync();
		return Ok(permisstions);
	}

	/// <summary>
	/// Assigns a permission to a user.
	/// </summary>
	/// <remarks>
	/// <b>Error Messages:</b><br></br>
	/// <list type="bullet">
	///   <item>Invalid permission requested.</item><br></br>
	///   <item>User does not exist.</item><br></br>
	///   <item>User already has this permission.</item><br></br>
	///   <item>TenantId is missing.</item><br></br>
	/// </list>
	/// </remarks>
	/// <response code="200">Permission assigned successfully.</response>
	/// <response code="400">Invalid permission or user does not exist.</response>
	/// <response code="403">Forbidden.</response>
	/// <response code="409">User already has this permission.</response>
	[Produces("application/json")]
	[HttpPost(Endpoints.Permisstion.GivePermission)]
	[AppAuthorize(FeatureFactory.Permission.CanGivePermisston)]
	public async Task<IActionResult> GivePermission(GivePermisstionRequest request)
	{
		if (!FeatureFactory.GetFlattenedPermissionList().Contains(request.Permission))
		{
			ModelState.AddModelError(nameof(request.Permission), "Invalid permission requested.");
			return this.ApplicationProblem
			(
				detail: "Invalid permission requested.",
				title: "Failed",
				statusCode: StatusCodes.Status400BadRequest,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.InvalidPermission,
				modelState: ModelState
			);
		}

		var excestinguser = await appDbContext.Users
											  .Include(u => u.Permissions)
											  .FirstOrDefaultAsync(u => u.UserId == request.UserId);

		if (excestinguser == null)
		{
			ModelState.AddModelError(nameof(request.UserId), "User does not exist.");
			return this.ApplicationProblem
			(
				detail: "User does not exist.",
				title: "Failed",
				statusCode: StatusCodes.Status404NotFound,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.UserNotFound,
				modelState: ModelState
			);

		}

		if (excestinguser.Permissions.Any(p => p.Name == request.Permission))
		{
			ModelState.AddModelError(nameof(request.Permission), "User already has this permission.");
			return this.ApplicationProblem
			(
				detail: "User already has this permission.",
				title: "Failed",
				statusCode: StatusCodes.Status409Conflict,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.PermissionAlreadyExists,
				modelState: ModelState
			);
		}

		await appDbContext.UserPermissions.AddAsync(Entities.Permission.Create(request.Permission, request.UserId));
		await appDbContext.SaveChangesAsync();
		return Ok();
	}


	/// <summary>
	/// Removes a permission from a user by permission name.
	/// </summary>
	/// <remarks>
	/// <b>Error Messages:</b>
	/// <list type="bullet">
	///   <item>Invalid permission requested.</item><br></br>
	///   <item>User does not exist.</item><br></br>
	///   <item>You cannot remove permissions from the primary tenant user.</item><br></br>
	///   <item>User does not have this permission.</item><br></br>
	///   <item>TenantId is missing.</item><br></br>
	/// </list>
	/// </remarks>
	/// <response code="200">Permission removed successfully.</response>
	/// <response code="400">Invalid permission or user does not exist.</response>
	/// <response code="403">Cannot remove from primary tenant user.</response>
	/// <response code="409">User does not have this permission.</response>
	[Produces("application/json")]
	[HttpPost(Endpoints.Permisstion.RemovePermissionByName)]
	[AppAuthorize(FeatureFactory.Permission.CanRemovePermisston)]
	public async Task<IActionResult> RemovePermissionByName(RemovePermisstionByNameRequest request)
	{
		if (!FeatureFactory.GetFlattenedPermissionList().Contains(request.Permission))
		{
			ModelState.AddModelError(nameof(request.Permission), "Invalid permission requested.");
			return this.ApplicationProblem
			(
				detail: "Invalid permission requested.",
				title: "Failed",
				statusCode: StatusCodes.Status400BadRequest,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.InvalidPermission,
				modelState: ModelState
			);
		}

		var tenantid = userProvider.TenantId;
		var excestinguser = await appDbContext.Users.Include(u => u.Permissions).FirstOrDefaultAsync(u => u.UserId == request.UserId);

		if (excestinguser == null)
		{
			ModelState.AddModelError(nameof(request.UserId), "User does not exist.");
			return this.ApplicationProblem
			(
				detail: "User does not exist.",
				title: "Failed",
				statusCode: StatusCodes.Status404NotFound,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.UserNotFound,
				modelState: ModelState
			);

		}

		if (excestinguser.IsTenantPrimary)
		{
			ModelState.AddModelError(nameof(excestinguser.IsTenantPrimary), "You cannot remove permissions of the primary tenant user.");
			return this.ApplicationProblem
			(
				detail: "You cannot remove permissions of the primary tenant user.",
				title: "Failed",
				statusCode: StatusCodes.Status403Forbidden,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.CantRemoveAccessToPrimary,
				modelState: ModelState
			);
		}

		if (excestinguser.RemovePermission(request.Permission))
		{
			ModelState.AddModelError(nameof(request.Permission), "User does not have this permission.");
			return this.ApplicationProblem
			(
				detail: "User does not have this permission.",
				title: "Failed",
				statusCode: StatusCodes.Status404NotFound,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.PermissionNotExist,
				modelState: ModelState
			);

		}

		await appDbContext.SaveChangesAsync();
		return Ok();
	}

	/// <summary>
	/// Removes a permission from a user by permission ID.
	/// </summary>
	/// <remarks>
	/// <b>Error Messages:</b>
	/// <list type="bullet">
	///   <item>User does not exist.</item><br></br>
	///   <item>You cannot remove permissions from the primary tenant user.</item><br></br>
	///   <item>User does not have this permission.</item><br></br>
	///   <item>TenantId is missing.</item><br></br>
	/// </list>
	/// </remarks>
	/// <response code="200">Permission removed successfully.</response>
	/// <response code="400">User does not exist.</response>
	/// <response code="403">Cannot remove from primary tenant user.</response>
	/// <response code="409">User does not have this permission.</response>
	[Produces("application/json")]
	[HttpPost(Endpoints.Permisstion.RemovePermission)]
	[AppAuthorize(FeatureFactory.Permission.CanRemovePermisston)]
	public async Task<IActionResult> RemovePermission(RemovePermisstionRequest request)
	{
		var tenantid = userProvider.TenantId;
		var excestinguser = await appDbContext
			.Users
			.Include(u => u.Permissions)
			.FirstOrDefaultAsync(u => u.UserId == request.UserId);

		if (excestinguser == null)
		{
			ModelState.AddModelError(nameof(request.UserId), "User does not exist.");
			return this.ApplicationProblem
			(
				detail: "User does not exist.",
				title: "Failed",
				statusCode: StatusCodes.Status404NotFound,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.UserNotFound,
				modelState: ModelState
			);
		}

		if (excestinguser.IsTenantPrimary)
		{
			ModelState.AddModelError(nameof(excestinguser.IsTenantPrimary), "You cannot remove permissions from the primary tenant user.");
			return this.ApplicationProblem
			(
				detail: "You cannot remove permissions from the primary tenant user.",
				title: "Failed",
				statusCode: StatusCodes.Status403Forbidden,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.CantRemoveAccessToPrimary,
				modelState: ModelState
			);
		}

		if (excestinguser.RemovePermission(request.PermissionId))
		{
			ModelState.AddModelError(nameof(request.PermissionId), "User does not have this permission.");
			return this.ApplicationProblem
			(
				detail: "User does not have this permission.",
				title: "Failed",
				statusCode: StatusCodes.Status409Conflict,
				instance: HttpContext.Request.Path,
				errorCode: ErrorCodes.PermissionNotExist,
				modelState: ModelState
			);
		}

		await appDbContext.SaveChangesAsync();
		return Ok();
	}
}
