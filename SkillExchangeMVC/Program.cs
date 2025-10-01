//Startup.cs/Program.cs
// Program.cs
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SkillExchangeMVC.Models.Context;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SkillExchangeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/CreateLogin";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.IsEssential = true;
        options.Cookie.HttpOnly = true;
    });
// Add session services
builder.Services.AddDistributedMemoryCache(); // Required for in-memory session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// ✅ Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireTeacherRole", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("RequireStudentRole", policy => policy.RequireRole("Student"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // ✅ Session Middleware
app.UseAuthentication();
app.UseAuthorization();

// ✅ Middleware: Root URL এ role অনুযায়ী redirect
app.Use(async (context, next) =>
{
    if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
    {
        var role = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (context.Request.Path == "/")
        {
            if (role == "Admin")
            {
                context.Response.Redirect("/Home/Privacy");
                return;
            }
            else 
            {
                context.Response.Redirect("/Home/Index"); // default
                return;
            }
        }
    }

    await next();
});

// ✅ Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
