using System;
using System.IO;
using System.Threading.Tasks;
using Beanstalk.App.Features;
using Beanstalk.Database.Data;
using Beanstalk.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using DbImage = Beanstalk.Database.Entities.Image;

namespace Beanstalk.App.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    private readonly BeanstalkConfig _config;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ImagesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, BeanstalkConfig config)
    {
        _db = db;
        _userManager = userManager;
        _config = config;
    }

    /// <summary>
    ///     GET /api/images/{id} - Serve full resolution image from database
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var image = await _db.Images.FindAsync(id);
        if (image == null) return NotFound();

        return File(image.ImageData, image.ContentType);
    }

    /// <summary>
    ///     GET /api/images/{id}/thumbnail - Serve thumbnail image from database
    /// </summary>
    [HttpGet("{id:guid}/thumbnail")]
    public async Task<IActionResult> GetImageThumbnail(Guid id)
    {
        var image = await _db.Images.FindAsync(id);
        if (image == null) return NotFound();

        // Check if thumbnail exists, if not generate it (for legacy images)
        if (image.ThumbnailData == null || image.ThumbnailData.Length == 0)
        {
            // Generate thumbnail from full image
            using var inputStream = new MemoryStream(image.ImageData);
            using var outputStream = new MemoryStream();
            using var img = await SixLabors.ImageSharp.Image.LoadAsync(inputStream);
            
            img.Mutate(x => x.Resize(128, 128));
            await img.SaveAsync(outputStream, new JpegEncoder { Quality = 85 });
            
            image.ThumbnailData = outputStream.ToArray();
            
            // Save the generated thumbnail to database
            _db.Images.Update(image);
            await _db.SaveChangesAsync();
        }

        return File(image.ThumbnailData, image.ContentType);
    }

    /// <summary>
    ///     POST /api/images/upload - Upload cropped image
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var profile = await _db.UserProfiles
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (profile == null)
        {
            profile = new UserProfile { Id = Guid.NewGuid(), UserId = user.Id, CreatedUtc = DateTime.UtcNow };
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }

        var maxImages = await _config.MaxImagesPerUser.Get();
        var maxImageSizeBytes = await _config.MaxImageSizeBytes.Get();

        if (profile.Images.Count >= maxImages)
            return BadRequest(new { error = $"Maximum of {maxImages} images allowed." });

        if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
            return BadRequest(new { error = "No file uploaded." });

        var file = Request.Form.Files[0];

        if (file.Length == 0)
            return BadRequest(new { error = "Empty file." });

        if (file.Length > maxImageSizeBytes)
            return BadRequest(new { error = "File too large. Maximum 10MB allowed." });

        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        if (contentType != "image/jpeg" && contentType != "image/png" && contentType != "image/webp")
            return BadRequest(new { error = "Unsupported image type. Use JPEG, PNG, or WebP." });

        // Read and process image data
        await using var uploadStream = new MemoryStream();
        await file.CopyToAsync(uploadStream);
        var imageData = uploadStream.ToArray();

        // Generate thumbnail
        byte[] thumbnailData;
        using (var inputStream = new MemoryStream(imageData))
        using (var outputStream = new MemoryStream())
        {
            using var img = await SixLabors.ImageSharp.Image.LoadAsync(inputStream);
            img.Mutate(x => x.Resize(128, 128));
            await img.SaveAsync(outputStream, new JpegEncoder { Quality = 85 });
            thumbnailData = outputStream.ToArray();
        }

        var imageId = Guid.NewGuid();
        var stored = new DbImage
        {
            Id = imageId,
            ProfileId = profile.Id,
            ImageData = imageData,
            ThumbnailData = thumbnailData,
            ContentType = contentType,
            CreatedUtc = DateTime.UtcNow
        };

        _db.Images.Add(stored);

        // Set as default if no default image exists
        if (!profile.SelectedImageId.HasValue)
            profile.SelectedImageId = stored.Id;

        await _db.SaveChangesAsync();

        return Ok(new { id = stored.Id, url = $"/api/images/{stored.Id}" });
    }

    /// <summary>
    ///     DELETE /api/images/{id} - Delete image
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (profile == null)
            return NotFound();

        var image = await _db.Images.FindAsync(id);

        if (image == null || image.ProfileId != profile.Id)
            return NotFound();

        // If this is the selected image, unset it
        if (profile.SelectedImageId == id)
            profile.SelectedImageId = null;

        _db.Images.Remove(image);
        await _db.SaveChangesAsync();

        return Ok(new { success = true });
    }

    /// <summary>
    ///     POST /api/images/{id}/set-default - Set as default profile image
    /// </summary>
    [HttpPost("{id:guid}/set-default")]
    public async Task<IActionResult> SetDefaultImage(Guid id)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (profile == null)
            return NotFound();

        var image = await _db.Images.FindAsync(id);

        if (image == null || image.ProfileId != profile.Id)
            return NotFound();

        profile.SelectedImageId = id;
        await _db.SaveChangesAsync();

        return Ok(new { success = true });
    }
}

