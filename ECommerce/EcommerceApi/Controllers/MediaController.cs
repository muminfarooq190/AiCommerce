using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Models.ResponseModels;

namespace EcommerceApi.Controllers;

[ApiController]
public sealed class MediaController(
        AppDbContext db,
        IUserProvider userProvider,
        IWebHostEnvironment env) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IWebHostEnvironment _env = env;

    private const string UploadDir = "uploads";

    /// <summary>
    /// Upload a file and get back its MediaFileId.
    /// </summary>
    [AppAuthorize(FeatureFactory.Media.CanAddMedia)]
    [HttpPost]
    [Route(Endpoints.Media.Upload)] 
    public async Task<ActionResult<MediaFileDto>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError("file", "No file provided or file is empty.");
            return this.ApplicationProblem(
                detail: "No file provided or file is empty.",
                title: "File Upload Error",
                statusCode: StatusCodes.Status400BadRequest,
                modelState: ModelState,
                errorCode: ErrorCodes.ValidationFailed,
                instance: HttpContext.Request.Path);
        }

        Directory.CreateDirectory(Path.Combine(_env.WebRootPath, UploadDir));
        var ext = Path.GetExtension(file.FileName);
        var mediaId = Guid.NewGuid();
        var newName = $"{mediaId}{ext}";
        var fullPath = Path.Combine(_env.WebRootPath, UploadDir, newName);

        await using (var fs = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(fs, ct);
        }

        var media = new MediaFile
        {
            MediaFileId = mediaId,
            FileName = file.FileName,
            MimeType = file.ContentType,
            Uri = $"/{UploadDir}/{newName}",
            TenantId = userProvider.TenantId
        };
        _db.MediaFiles.Add(media);
        await _db.SaveChangesAsync(ct);

        var dto = new MediaFileDto(media.MediaFileId, media.Uri);
        return CreatedAtAction(nameof(GetById), new { id = dto.MediaFileId }, dto);
    }


    [AppAuthorize(FeatureFactory.Media.CanGetMedia)]
    [HttpGet]
    [Route(Endpoints.Media.Get)]
    public async Task<ActionResult<MediaFileDto>> GetById(Guid id, CancellationToken ct)
    {
        var media = await _db.MediaFiles.AsNoTracking().FirstOrDefaultAsync(m => m.MediaFileId == id, ct);
        if (media is null)
        {
            ModelState.AddModelError(nameof(id), "Media file not found.");
            return this.ApplicationProblem(
                detail: $"Media file '{id}' not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        return Ok(new MediaFileDto(media.MediaFileId, media.Uri));
    }
}

