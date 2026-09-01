using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Data;

/// <summary>
/// Crea el esquema mediante migraciones y carga datos idempotentes de demostración.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var configuration = services.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("SeedData:Enabled", true))
        {
            return;
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await EnsureRoleAsync(roleManager, RoleNames.User);
        await EnsureRoleAsync(roleManager, RoleNames.Admin);

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await EnsureUserAsync(
            userManager,
            username: "admin",
            email: "admin@biblioteca.local",
            displayName: "Administrador",
            password: "Admin123!",
            role: RoleNames.Admin);
        var user = await EnsureUserAsync(
            userManager,
            username: "user",
            email: "user@biblioteca.local",
            displayName: "Usuario de demo",
            password: "User123!",
            role: RoleNames.User);

        if (await context.Authors.AnyAsync())
        {
            await EnsureMinimumDemoBooksAsync(context);

            // Si se parte de una BD creada antes de introducir pedidos y no había
            // compras históricas, deja igualmente una compra demo disponible.
            if (!await context.Orders.AnyAsync())
            {
                var demoBook = await context.Books
                    .OrderBy(book => book.Id)
                    .FirstOrDefaultAsync();
                if (demoBook is not null)
                {
                    context.Orders.Add(CreateDemoOrder(user.Id, demoBook));
                    await context.SaveChangesAsync();
                }
            }

            return;
        }

        var garciaMarquez = new Author
        {
            Name = "Gabriel García Márquez",
            Nationality = "Colombiana",
            BirthDate = new DateTime(1927, 3, 6),
            Bio = "Escritor colombiano y uno de los grandes representantes del realismo mágico."
        };
        var austen = new Author
        {
            Name = "Jane Austen",
            Nationality = "Británica",
            BirthDate = new DateTime(1775, 12, 16),
            Bio = "Novelista inglesa conocida por sus historias sobre la sociedad y las relaciones humanas."
        };
        var cervantes = new Author
        {
            Name = "Miguel de Cervantes",
            Nationality = "Española",
            BirthDate = new DateTime(1547, 9, 29),
            Bio = "Autor de una de las obras fundamentales de la literatura universal."
        };

        var novela = new Category
        {
            Name = "Novela",
            Description = "Historias extensas de ficción narrativa.",
            Color = "#2563eb"
        };
        var clasico = new Category
        {
            Name = "Clásico",
            Description = "Obras que forman parte del patrimonio literario.",
            Color = "#7c3aed"
        };
        var realismoMagico = new Category
        {
            Name = "Realismo mágico",
            Description = "Narraciones donde lo extraordinario convive con lo cotidiano.",
            Color = "#db2777"
        };
        var aventuras = new Category
        {
            Name = "Aventuras",
            Description = "Viajes, retos y descubrimientos.",
            Color = "#059669"
        };

        context.Authors.AddRange(garciaMarquez, austen, cervantes);
        context.Categories.AddRange(novela, clasico, realismoMagico, aventuras);
        await context.SaveChangesAsync();

        var books = new[]
        {
            new Book
            {
                Title = "Cien años de soledad",
                Price = 18.90m,
                Available = true,
                PublishDate = new DateTime(1967, 5, 30),
                Isbn = "9780307474728",
                Pages = 496,
                Language = "Español",
                Synopsis = "La historia de la familia Buendía a lo largo de varias generaciones en Macondo.",
                Author = garciaMarquez,
                Categories = new List<Category> { novela, realismoMagico, clasico }
            },
            new Book
            {
                Title = "Orgullo y prejuicio",
                Price = 14.50m,
                Available = true,
                PublishDate = new DateTime(1813, 1, 28),
                Isbn = "9780141439518",
                Pages = 432,
                Language = "Español",
                Synopsis = "Elizabeth Bennet y Fitzwilliam Darcy deben superar sus primeras impresiones.",
                Author = austen,
                Categories = new List<Category> { novela, clasico }
            },
            new Book
            {
                Title = "Don Quijote de la Mancha",
                Price = 22.00m,
                Available = true,
                PublishDate = new DateTime(1605, 1, 16),
                Isbn = "9788420412146",
                Pages = 1056,
                Language = "Español",
                Synopsis = "Las aventuras del ingenioso hidalgo que decide convertirse en caballero andante.",
                Author = cervantes,
                Categories = new List<Category> { novela, clasico, aventuras }
            },
            new Book
            {
                Title = "La biblioteca de los sueños",
                Price = 16.75m,
                Available = false,
                PublishDate = new DateTime(2024, 4, 23),
                Isbn = "9780000000000",
                Pages = 288,
                Language = "Español",
                Synopsis = "Una novela contemporánea sobre libros, memoria y el poder de las historias.",
                Author = austen,
                Categories = new List<Category> { novela }
            },
            new Book
            {
                Title = "El amor en los tiempos del cólera",
                Price = 19.50m,
                Available = true,
                PublishDate = new DateTime(1985, 9, 1),
                Isbn = "9780307389732",
                Pages = 368,
                Language = "Español",
                Synopsis = "Una historia de amor, espera y segundas oportunidades a lo largo de varias décadas.",
                Author = garciaMarquez,
                Categories = new List<Category> { novela, realismoMagico }
            },
            new Book
            {
                Title = "Sentido y sensibilidad",
                Price = 15.25m,
                Available = true,
                PublishDate = new DateTime(1811, 10, 30),
                Isbn = "9780141439662",
                Pages = 384,
                Language = "Español",
                Synopsis = "Las hermanas Dashwood afrontan el amor, la pérdida y las normas de la sociedad de su tiempo.",
                Author = austen,
                Categories = new List<Category> { novela, clasico }
            }
        };

        context.Books.AddRange(books);
        await context.SaveChangesAsync();

        user.FavoriteBooks.Add(books[0]);
        context.Reviews.Add(new Review
        {
            Comment = "Una lectura imprescindible y una magnífica puerta de entrada al realismo mágico.",
            Rating = 5,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UserId = user.Id,
            BookId = books[0].Id
        });
        context.Reviews.Add(new Review
        {
            Comment = "Una novela divertida, inteligente y sorprendentemente actual.",
            Rating = 4,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UserId = admin.Id,
            BookId = books[1].Id
        });
        context.Orders.Add(CreateDemoOrder(user.Id, books[0], DateTime.UtcNow.AddDays(-2)));
        await context.SaveChangesAsync();
    }

    private static async Task EnsureMinimumDemoBooksAsync(ApplicationDbContext context)
    {
        if (await context.Books.CountAsync() >= 6)
        {
            return;
        }

        var garciaMarquez = await context.Authors
            .FirstOrDefaultAsync(author => author.Name == "Gabriel García Márquez");
        var austen = await context.Authors
            .FirstOrDefaultAsync(author => author.Name == "Jane Austen");
        var novela = await context.Categories
            .FirstOrDefaultAsync(category => category.Name == "Novela");
        var realismoMagico = await context.Categories
            .FirstOrDefaultAsync(category => category.Name == "Realismo mágico");
        var clasico = await context.Categories
            .FirstOrDefaultAsync(category => category.Name == "Clásico");

        if (garciaMarquez is null || austen is null || novela is null || realismoMagico is null || clasico is null)
        {
            return;
        }

        var additionalBooks = new List<Book>();
        if (!await context.Books.AnyAsync(book => book.Title == "El amor en los tiempos del cólera"))
        {
            additionalBooks.Add(new Book
            {
                Title = "El amor en los tiempos del cólera",
                Price = 19.50m,
                Available = true,
                PublishDate = new DateTime(1985, 9, 1),
                Isbn = "9780307389732",
                Pages = 368,
                Language = "Español",
                Synopsis = "Una historia de amor, espera y segundas oportunidades a lo largo de varias décadas.",
                Author = garciaMarquez,
                Categories = new List<Category> { novela, realismoMagico }
            });
        }

        if (!await context.Books.AnyAsync(book => book.Title == "Sentido y sensibilidad"))
        {
            additionalBooks.Add(new Book
            {
                Title = "Sentido y sensibilidad",
                Price = 15.25m,
                Available = true,
                PublishDate = new DateTime(1811, 10, 30),
                Isbn = "9780141439662",
                Pages = 384,
                Language = "Español",
                Synopsis = "Las hermanas Dashwood afrontan el amor, la pérdida y las normas de la sociedad de su tiempo.",
                Author = austen,
                Categories = new List<Category> { novela, clasico }
            });
        }

        if (additionalBooks.Count > 0)
        {
            context.Books.AddRange(additionalBooks);
            await context.SaveChangesAsync();
        }
    }

    private static Order CreateDemoOrder(
        string userId,
        Book book,
        DateTime? createdAt = null)
    {
        return new Order
        {
            UserId = userId,
            CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-2),
            Status = OrderStatus.Paid,
            Total = book.Price,
            PaymentMethod = "Tarjeta demo",
            PaymentLastFour = "4242",
            Items = new List<OrderItem>
            {
                new()
                {
                    BookId = book.Id,
                    BookTitle = book.Title,
                    Quantity = 1,
                    UnitPrice = book.Price
                }
            }
        };
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            EnsureSucceeded(result, $"No se pudo crear el rol {role}.");
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string email,
        string displayName,
        string password,
        string role)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                DisplayName = displayName,
                EmailConfirmed = true,
                IsActive = true
            };
            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult, $"No se pudo crear el usuario {username}.");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(roleResult, $"No se pudo asignar el rol {role} al usuario {username}.");
        }

        return user;
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"{message} {errors}");
        }
    }
}
