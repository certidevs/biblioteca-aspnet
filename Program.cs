using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;
using BibliotecaAspNet.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se ha configurado la conexión DefaultConnection.");

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

// Identity es la alternativa de ASP.NET Core a Spring Security.
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

builder.Services.AddControllersWithViews(options =>
{
    // Protege automáticamente todos los POST/PUT/DELETE de los formularios MVC.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Repositorios: la capa equivalente a Spring Data JPA.
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Servicios: lógica de negocio entre controladores y persistencia.
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
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
builder.Services.AddSingleton<IImageStorage, ImageStorage>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
var appDataDirectory = Path.Combine(app.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appDataDirectory);

await using (var scope = app.Services.CreateAsyncScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();
