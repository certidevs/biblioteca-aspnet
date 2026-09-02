using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se ha configurado la conexión DefaultConnection.");

// DbContext se registra como scoped: una petición HTTP trabaja con una unidad de contexto.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// El carrito se guarda en la sesión del navegador; el pedido final sí se persiste en SQLite.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Identity gestiona usuarios, cookies de sesión, contraseñas y roles.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
    options.Cookie.Name = "BibliotecaAspNet.Auth";
    options.SlidingExpiration = true;
    options.Events.OnValidatePrincipal = async context =>
    {
        if (context.Principal is null)
        {
            context.RejectPrincipal();
            return;
        }

        var userManager = context.HttpContext.RequestServices
            .GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.Principal);
        if (user is null || !user.IsActive)
        {
            context.RejectPrincipal();
        }
    };
});

// MVC descubre controladores y vistas Razor; el filtro protege los formularios POST.
builder.Services.AddControllersWithViews(options =>
{
    // Protege automáticamente todos los POST/PUT/DELETE de los formularios MVC.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// EF Core ya actúa como repositorio y unidad de trabajo. Se inyectan clases concretas
// solo donde hay una regla útil (carrito, imágenes, checkout o Identity), sin interfaces duplicadas.
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<AuthorService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.Configure<ImageStorageOptions>(
    builder.Configuration.GetSection("FileUploads:Images"));
var maxImageSize = builder.Configuration.GetValue(
    "FileUploads:Images:MaxFileSizeBytes",
    5 * 1024 * 1024);
builder.Services.Configure<FormOptions>(options =>
{
    // Deja un pequeño margen para el multipart alrededor del archivo.
    options.MultipartBodyLengthLimit = maxImageSize + (512 * 1024);
});
builder.Services.AddSingleton<ImageStorage>();

var app = builder.Build();

// En desarrollo se muestran errores detallados; fuera de él se usa una página controlada.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// El orden del pipeline importa: primero recursos/rutas, después sesión y seguridad.
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// En desarrollo se aplican las migraciones y se cargan datos de demo automáticamente.
// La inicialización usa async solo porque las APIs de roles y contraseñas de Identity lo exigen.
var appDataDirectory = Path.Combine(app.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appDataDirectory);

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();
