using Microsoft.EntityFrameworkCore;
using proyecto_mejoradoMy_pet.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configurar sesiones con opciones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Sesión expira después de 30 minutos de inactividad
    options.Cookie.HttpOnly = true; // Protección contra XSS
    options.Cookie.IsEssential = true; // Necesaria para el funcionamiento de la app
});

builder.Services.AddDbContext<BdMypetv3Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Conexion")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // ✅ Ya lo tienes aquí - IMPORTANTE: debe ir antes de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Privacy}/{id?}");

app.Run();