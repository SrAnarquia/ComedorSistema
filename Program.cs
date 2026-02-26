using ComedorSistema.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// MVC + API
builder.Services.AddControllers(); // Esto permite API Controllers
builder.Services.AddControllersWithViews(); // Esto mantiene tus vistas

// Habilitar CORS para pruebas desde Postman u otras apps
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(); // Permitir CORS
app.UseAuthorization();

// Mapear rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Mapear rutas API
app.MapControllers(); // Esto es clave para que Postman pueda llamar a tus API Controllers

app.Run();