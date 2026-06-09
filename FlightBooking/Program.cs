using FlightBooking.Data;
using FlightBooking.Web.Data;
using FlightBooking.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
               .GetConnectionString("DefaultConnection")));



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
    o.ExpireTimeSpan  = TimeSpan.FromHours(8);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddSession();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider;

    // Ensure DB is created/migrated if you want (optional)
    var context = svc.GetRequiredService<AppDbContext>();

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

app.UseRouting();

// ADD THIS � redirects 404 to your NotFound view
app.UseStatusCodePagesWithReExecute("/Home/NotFound404");

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
