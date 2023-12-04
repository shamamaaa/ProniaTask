using Microsoft.EntityFrameworkCore;
using ProniaTask.DAL;
using ProniaTask.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<LayoutService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
 
app.UseEndpoints(endpoints =>
endpoints.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=home}/{action=index}/{id?}")
);

app.MapControllerRoute(
    "default", "{controller=home}/{action=index}/{id?}");

app.Run();
