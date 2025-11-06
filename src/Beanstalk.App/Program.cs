using System;
using System.Linq;
using Beanstalk.App.Components;
using Beanstalk.App.Features;
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
builder.Services.AddControllers();
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
builder.Services.AddScoped<BeanstalkNavigator>();
builder.Services.AddScoped<BeanstalkConfig>();

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
app.MapControllers();
var razorComponents = app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (app.Environment.IsDevelopment())
    razorComponents.WithMetadata(new IgnoreAntiforgeryTokenAttribute());

app.Run();