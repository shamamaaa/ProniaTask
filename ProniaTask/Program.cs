using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProniaTask.DAL;
using ProniaTask.Interfaces;
using ProniaTask.Middlewares;
using ProniaTask.Models;
using ProniaTask.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddIdentity<AppUser, IdentityRole>(option =>
{
    option.Password.RequiredLength = 8;
    option.Password.RequireNonAlphanumeric = false;

    option.User.RequireUniqueEmail = true;
    option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
}
).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


builder.Services.AddScoped<LayoutService>();
builder.Services.AddScoped<IEmailService,EmailService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddSession(options =>
options.IdleTimeout = TimeSpan.FromSeconds(60)
);

var app = builder.Build();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseEndpoints(endpoints =>
endpoints.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=home}/{action=index}/{id?}")
);

app.MapControllerRoute(
    "default", "{controller=home}/{action=index}/{id?}");

app.Run();
