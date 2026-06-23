using FlightBooking.Data;
using FlightBooking.Infrastructure.Middleware;
using FlightBooking.Models.Domain;
using FlightBooking.Services;
using FlightBooking.Web.Data;
using FlightBooking.Web.Infrastructure.Resilience;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;


// Load environment variables from .env file if it exists in the root directory
var rootDir = Directory.GetCurrentDirectory();
var dotenvPath = Path.Combine(rootDir, ".env");
if (File.Exists(dotenvPath))
{
    foreach (var line in File.ReadAllLines(dotenvPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
            continue;

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();

            // Strip optional wrapping quotes
            if ((value.StartsWith("\"") && value.EndsWith("\"")) || 
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value.Substring(1, value.Length - 2);
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

var builder = WebApplication.CreateBuilder(args);





Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Initializing underlying Flight Booking system core assemblies...");

    // Add services to the container.
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    });

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


    //Asp.net core identity services

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(o =>
    {
        o.Password.RequireDigit           = true;
        o.Password.RequiredLength         = 8;
        o.Password.RequireUppercase       = true;
        o.Password.RequireNonAlphanumeric = true;
        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan  =
            TimeSpan.FromMinutes(10);
        o.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


// Cookie redirect paths
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath       = "/Account/Login";
    o.LogoutPath      = "/Account/Logout";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.SlidingExpiration = true;
    o.ExpireTimeSpan  = TimeSpan.FromMinutes(60);


    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict;

});

builder.Services.AddScoped<ISanitizerService, SanitizerService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<SeatService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddHttpClient("ResilientClient")
    .AddPolicyHandler(PollyPolicyRegistry.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicyRegistry.GetCircuitBreakerPolicy());


builder.Services.AddSession();

// Add in-memory cache (no Redis needed)
builder.Services.AddMemoryCache();

// Register custom cache wrapper utility
builder.Services.AddScoped<ICacheService, CacheService>();

var app = builder.Build();

app.UseMiddleware<FlightBooking.Infrastructure.Middleware.GlobalExceptionMiddleware>();

    // 3. Implement Custom HTTP Security Headers Injection Middleware Block
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff"); // Stops MIME-type sniffing attacks
        context.Response.Headers.Append("X-Frame-Options", "DENY"); // Blocks UI clickjacking vulnerabilities inside frames
        context.Response.Headers.Append("Referrer-Policy", "no-referrer"); // Restricts data context sharing across origins
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block"); // Forces modern browser built-in XSS filters to block threats
        await next();
    });


    using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider;

    // Ensure DB is created/migrated if you want (optional)
    var context = svc.GetRequiredService<AppDbContext>();

    DbSeeder.Seed(context);

    // Roles + admin user seed
    await RoleSeeder.SeedAsync(
        svc.GetRequiredService<RoleManager<IdentityRole>>(),
        svc.GetRequiredService<UserManager<ApplicationUser>>());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseRouting();

// ADD THIS � redirects 404 to your NotFound view
app.UseStatusCodePagesWithReExecute("/Home/NotFound404");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex,"The application runtime host terminated unexpectedly during initial bootstrap configuration loops!");
}
finally
{
    Log.CloseAndFlush();
}










