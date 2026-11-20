using AgendaEEQ.Models;
using AgendaEEQ.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<KpiMetas>(builder.Configuration.GetSection("KpiMetas"));
builder.Services.AddSingleton<AgendaExcelReader>();
builder.Services.AddSingleton<AgendaStore>();
builder.Services.AddSingleton<KpiCalculator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Kpi/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Kpi}/{action=Index}/{id?}");

app.Run();
