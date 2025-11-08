using System;
using System.Linq;
using Beanstalk.App.Components;
using Beanstalk.App.Features;
using Beanstalk.App.Features.ProfileDisplay;
using Beanstalk.Database.Data;
using Beanstalk.Database.Entities;
using Beanstalk.Database.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

// Use pooled DbContextFactory for Blazor Server to avoid concurrency issues
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add a scoped DbContext for Identity operations
builder.Services.AddScoped<ApplicationDbContext>(provider =>
{
    var factory = provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    return factory.CreateDbContext();
});

// Use standard UserStore and RoleStore with scoped DbContext
builder.Services.AddScoped<IUserStore<ApplicationUser>, UserStore<ApplicationUser, IdentityRole<Guid>, ApplicationDbContext, Guid>>();
builder.Services.AddScoped<IRoleStore<IdentityRole<Guid>>, RoleStore<IdentityRole<Guid>, ApplicationDbContext, Guid>>();

// Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddCookie(IdentityConstants.ApplicationScheme, options => { options.LoginPath = "/site/login"; })
    .AddCookie(IdentityConstants.TwoFactorUserIdScheme, options =>
    {
        options.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    });

builder.Services.AddAuthorization();

builder.Services.AddTransient<ProfileDisplayModelFactory>();
builder.Services.AddScoped<BeanstalkNavigator>();
builder.Services.AddScoped<BeanstalkConfig>();

// Configure forwarded headers for reverse proxy (Cloudflare Tunnel)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Trust all proxies - Cloudflare Tunnel uses localhost
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Use forwarded headers from reverse proxy (must be before other middleware)
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error", true);
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
// Apply EF Core migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    db.Database.Migrate();

    // Seed data if required.
    new ThemePaletteSeeder().SeedIfRequired(db);
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