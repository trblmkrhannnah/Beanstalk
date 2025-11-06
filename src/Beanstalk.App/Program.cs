using System;
using System.IO;
using System.Linq;
using Beanstalk.App.Components;
using Beanstalk.App.Features.Navigator;
using Beanstalk.App.Features.ProfileDisplay;
using Beanstalk.Database.Data;
using Beanstalk.Database.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to allow larger request bodies
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure form options to allow larger file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
    options.ValueLengthLimit = 10 * 1024 * 1024;
    options.MultipartHeadersLengthLimit = 10 * 1024 * 1024;
});

// Antiforgery configuration (allow HTTP for local dev)
const string AntiCookieName = "Beanstalk.Antiforgery";
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = AntiCookieName;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.Path = "/";
});

// Database
var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                       ?? "Host=localhost;Database=beanstalk;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddCookie(IdentityConstants.ApplicationScheme, options => { options.LoginPath = "/site/login"; });

builder.Services.AddAuthorization();

builder.Services.AddTransient<ProfileDisplayModelFactory>();
builder.Services.AddScoped<Navigator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error", true);
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
// Apply EF Core migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Seed theme palettes if none exist
    if (!db.ThemePalettes.Any())
    {
        var palettes = new[]
        {
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Ocean Breeze",
                IsDefault = true,
                BackgroundGradient1 = "#667eea",
                BackgroundGradient2 = "#764ba2",
                TitleColor = "#ffffff",
                ContentColor = "#f0f0f0",
                ContainerBackground = "#ffffff",
                ContainerForeground = "#667eea"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Sunset Glow",
                IsDefault = false,
                BackgroundGradient1 = "#ff6b6b",
                BackgroundGradient2 = "#feca57",
                TitleColor = "#ffffff",
                ContentColor = "#ffffff",
                ContainerBackground = "#ffffff",
                ContainerForeground = "#ff6b6b"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Forest Deep",
                IsDefault = false,
                BackgroundGradient1 = "#0f4c3a",
                BackgroundGradient2 = "#1e8467",
                TitleColor = "#ffffff",
                ContentColor = "#e0e0e0",
                ContainerBackground = "#ffffff",
                ContainerForeground = "#0f4c3a"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Midnight Sky",
                IsDefault = false,
                BackgroundGradient1 = "#1a1a2e",
                BackgroundGradient2 = "#16213e",
                TitleColor = "#eee",
                ContentColor = "#ddd",
                ContainerBackground = "#0f3460",
                ContainerForeground = "#e94560"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Cherry Blossom",
                IsDefault = false,
                BackgroundGradient1 = "#ff9a9e",
                BackgroundGradient2 = "#fecfef",
                TitleColor = "#5a1f47",
                ContentColor = "#7a3f67",
                ContainerBackground = "#5a1f47",
                ContainerForeground = "#ffffff"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Arctic Ice",
                IsDefault = false,
                BackgroundGradient1 = "#a8edea",
                BackgroundGradient2 = "#fed6e3",
                TitleColor = "#2c3e50",
                ContentColor = "#34495e",
                ContainerBackground = "#2c3e50",
                ContainerForeground = "#ffffff"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Desert Sand",
                IsDefault = false,
                BackgroundGradient1 = "#d4a574",
                BackgroundGradient2 = "#f4e4d7",
                TitleColor = "#3e2723",
                ContentColor = "#5d4037",
                ContainerBackground = "#6d4c41",
                ContainerForeground = "#ffffff"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Neon Nights",
                IsDefault = false,
                BackgroundGradient1 = "#7f00ff",
                BackgroundGradient2 = "#e100ff",
                TitleColor = "#ffffff",
                ContentColor = "#f0f0f0",
                ContainerBackground = "#00ff88",
                ContainerForeground = "#000000"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Autumn Harvest",
                IsDefault = false,
                BackgroundGradient1 = "#c94b4b",
                BackgroundGradient2 = "#f4a261",
                TitleColor = "#ffffff",
                ContentColor = "#fefae0",
                ContainerBackground = "#2a9d8f",
                ContainerForeground = "#ffffff"
            },
            new ThemePalette
            {
                Id = Guid.NewGuid(),
                Name = "Monochrome",
                IsDefault = false,
                BackgroundGradient1 = "#2c2c2c",
                BackgroundGradient2 = "#1a1a1a",
                TitleColor = "#ffffff",
                ContentColor = "#cccccc",
                ContainerBackground = "#ffffff",
                ContainerForeground = "#000000"
            }
        };

        db.ThemePalettes.AddRange(palettes);
        db.SaveChanges();
    }
}

// Development-only: log POST form keys to simplify form-name debugging
if (app.Environment.IsDevelopment())
    app.Use(async (context, next) =>
    {
        if (string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
            && context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();
            var keys = string.Join(",", form.Keys);
            app.Logger.LogInformation("POST {Path} form keys: {Keys}", context.Request.Path, keys);
            var antiCookiePresent = context.Request.Cookies.ContainsKey(AntiCookieName);
            app.Logger.LogInformation("Antiforgery cookie {Name} present: {Present}", AntiCookieName, antiCookiePresent);
        }

        await next();
    });

app.UseAuthentication();
app.UseAuthorization();


app.UseAntiforgery();

app.MapStaticAssets();
var razorComponents = app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (app.Environment.IsDevelopment()) razorComponents.WithMetadata(new IgnoreAntiforgeryTokenAttribute());

// Profile Images API Endpoints

// GET /api/profile-images/{id} - Serve image from database
app.MapGet("/api/profile-images/{id:guid}", async (Guid id, ApplicationDbContext db) =>
{
    var image = await db.ProfileImages.FindAsync(id);
    if (image == null) return Results.NotFound();

    return Results.File(image.ImageData, image.ContentType);
});

// POST /api/profile-images/upload - Upload cropped image
app.MapPost("/api/profile-images/upload", async (HttpContext http, ApplicationDbContext db, UserManager<ApplicationUser> userManager) =>
{
    if (!http.User.Identity?.IsAuthenticated ?? true) return Results.Unauthorized();

    var user = await userManager.GetUserAsync(http.User);
    if (user == null) return Results.Unauthorized();

    var profile = await db.UserProfiles
        .Include(p => p.Images)
        .FirstOrDefaultAsync(p => p.UserId == user.Id);

    if (profile == null)
    {
        profile = new UserProfile { Id = Guid.NewGuid(), UserId = user.Id, CreatedUtc = DateTime.UtcNow };
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();
    }

    // Check max images limit
    var maxImagesSetting = await db.AppSettings.FindAsync("MaxImagesPerUser");
    var maxImages = 10;
    if (maxImagesSetting != null && int.TryParse(maxImagesSetting.Value, out var parsedMax)) maxImages = parsedMax;

    if (profile.Images.Count >= maxImages) return Results.BadRequest(new { error = $"Maximum of {maxImages} images allowed." });

    if (!http.Request.HasFormContentType || http.Request.Form.Files.Count == 0) return Results.BadRequest(new { error = "No file uploaded." });

    var file = http.Request.Form.Files[0];
    if (file.Length == 0) return Results.BadRequest(new { error = "Empty file." });

    if (file.Length > 10 * 1024 * 1024) return Results.BadRequest(new { error = "File too large. Maximum 10MB allowed." });

    var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
    if (contentType != "image/jpeg" && contentType != "image/png" && contentType != "image/webp") return Results.BadRequest(new { error = "Unsupported image type. Use JPEG, PNG, or WebP." });

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

    db.ProfileImages.Add(stored);

    // Set as default if no default image exists
    if (!profile.SelectedImageId.HasValue) profile.SelectedImageId = stored.Id;

    await db.SaveChangesAsync();

    return Results.Ok(new { id = stored.Id, url = $"/api/profile-images/{stored.Id}" });
});

// DELETE /api/profile-images/{id} - Delete image
app.MapDelete("/api/profile-images/{id:guid}", async (Guid id, HttpContext http, ApplicationDbContext db, UserManager<ApplicationUser> userManager) =>
{
    if (!http.User.Identity?.IsAuthenticated ?? true) return Results.Unauthorized();

    var user = await userManager.GetUserAsync(http.User);
    if (user == null) return Results.Unauthorized();

    var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
    if (profile == null) return Results.NotFound();

    var image = await db.ProfileImages.FindAsync(id);
    if (image == null || image.ProfileId != profile.Id) return Results.NotFound();

    // If this is the selected image, unset it
    if (profile.SelectedImageId == id) profile.SelectedImageId = null;

    db.ProfileImages.Remove(image);
    await db.SaveChangesAsync();

    return Results.Ok(new { success = true });
});

// POST /api/profile-images/{id}/set-default - Set as default profile image
app.MapPost("/api/profile-images/{id:guid}/set-default", async (Guid id, HttpContext http, ApplicationDbContext db, UserManager<ApplicationUser> userManager) =>
{
    if (!http.User.Identity?.IsAuthenticated ?? true) return Results.Unauthorized();

    var user = await userManager.GetUserAsync(http.User);
    if (user == null) return Results.Unauthorized();

    var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
    if (profile == null) return Results.NotFound();

    var image = await db.ProfileImages.FindAsync(id);
    if (image == null || image.ProfileId != profile.Id) return Results.NotFound();

    profile.SelectedImageId = id;
    await db.SaveChangesAsync();

    return Results.Ok(new { success = true });
});

app.Run();