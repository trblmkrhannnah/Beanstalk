using System;
using System.IO;
using System.Threading.Tasks;
using Beanstalk.App.Features;
using Beanstalk.Database.Data;
using Beanstalk.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Beanstalk.App.Controllers;

[ApiController]
[Route("api/profile-images")]
public class ProfileImagesController : ControllerBase
{
    private readonly BeanstalkConfig _config;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileImagesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, BeanstalkConfig config)
    {
        _db = db;
        _userManager = userManager;
        _config = config;
    }

    /// <summary>
    ///     GET /api/profile-images/{id} - Serve image from database
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var image = await _db.ProfileImages.FindAsync(id);
        if (image == null) return NotFound();

        return File(image.ImageData, image.ContentType);
    }

    /// <summary>
    ///     POST /api/profile-images/upload - Upload cropped image
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

        // Read image data
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var imageData = memoryStream.ToArray();

        var imageId = Guid.NewGuid();
        var stored = new ProfileImage
        {
            Id = imageId,
            ProfileId = profile.Id,
            ImageData = imageData,
            ContentType = contentType,
            CreatedUtc = DateTime.UtcNow
        };

        _db.ProfileImages.Add(stored);

        // Set as default if no default image exists
        if (!profile.SelectedImageId.HasValue)
            profile.SelectedImageId = stored.Id;

        await _db.SaveChangesAsync();

        return Ok(new { id = stored.Id, url = $"/api/profile-images/{stored.Id}" });
    }

    /// <summary>
    ///     DELETE /api/profile-images/{id} - Delete image
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

        var image = await _db.ProfileImages.FindAsync(id);

        if (image == null || image.ProfileId != profile.Id)
            return NotFound();

        // If this is the selected image, unset it
        if (profile.SelectedImageId == id)
            profile.SelectedImageId = null;

        _db.ProfileImages.Remove(image);
        await _db.SaveChangesAsync();

        return Ok(new { success = true });
    }

    /// <summary>
    ///     POST /api/profile-images/{id}/set-default - Set as default profile image
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

        var image = await _db.ProfileImages.FindAsync(id);

        if (image == null || image.ProfileId != profile.Id)
            return NotFound();

        profile.SelectedImageId = id;
        await _db.SaveChangesAsync();

        return Ok(new { success = true });
    }
}