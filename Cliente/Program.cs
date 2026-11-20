using Cliente.Models;
using Cliente.Models.Kpi;
using Cliente.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var cn= builder.Configuration.GetConnectionString("cn");
builder.Services.AddDbContext<clientedbcontext>(opciones=> opciones.UseSqlServer(cn));
// Add services to the container.
builder.Services.AddControllersWithViews();

// Módulo de KPI (Agenda de Trabajo Z4)
builder.Services.Configure<KpiMetas>(builder.Configuration.GetSection("KpiMetas"));
builder.Services.AddSingleton<AgendaExcelReader>();
builder.Services.AddSingleton<AgendaStore>();
builder.Services.AddSingleton<KpiCalculator>();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
