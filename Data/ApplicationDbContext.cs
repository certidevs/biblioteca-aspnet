using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Data;

/// <summary>
/// Contexto de EF Core. Es el equivalente a un EntityManager de JPA + configuración ORM.
/// </summary>
public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// EF Core inyecta estas opciones desde <c>Program.cs</c> para saber qué proveedor
    /// usar y cómo conectarse a SQLite.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>Tabla de autores y punto de entrada para consultas LINQ.</summary>
    public DbSet<Author> Authors => Set<Author>();

    /// <summary>Tabla de libros y punto de entrada para consultas LINQ.</summary>
    public DbSet<Book> Books => Set<Book>();

    /// <summary>Tabla de categorías y punto de entrada para consultas LINQ.</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>Tabla de reseñas y punto de entrada para consultas LINQ.</summary>
    public DbSet<Review> Reviews => Set<Review>();

    /// <summary>Tabla de pedidos y punto de entrada para consultas LINQ.</summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>Tabla de líneas de pedido y punto de entrada para consultas LINQ.</summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Configura restricciones de columnas y cardinalidades que no se expresan
    /// completamente con atributos de DataAnnotations.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(100);
            entity.Property(user => user.AvatarFileName).HasMaxLength(260);
            entity.Property(user => user.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(author => author.Name).HasMaxLength(150).IsRequired();
            entity.Property(author => author.Bio).HasMaxLength(2000);
            entity.Property(author => author.Nationality).HasMaxLength(80);
            entity.Property(author => author.PhotoFileName).HasMaxLength(260);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(book => book.Title).HasMaxLength(200).IsRequired();
            entity.Property(book => book.Price).HasPrecision(10, 2);
            entity.Property(book => book.Isbn).HasMaxLength(20);
            entity.Property(book => book.Language).HasMaxLength(60);
            entity.Property(book => book.Synopsis).HasMaxLength(5000);
            entity.Property(book => book.CoverImageFileName).HasMaxLength(260);

            // Un autor puede tener muchos libros; borrar el autor borra su catálogo.
            entity.HasOne(book => book.Author)
                .WithMany(author => author.Books)
                .HasForeignKey(book => book.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // EF crea BookCategories para resolver la relación N:M sin una entidad extra.
            entity.HasMany(book => book.Categories)
                .WithMany(category => category.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "BookCategories",
                    right => right.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                    left => left.HasOne<Book>().WithMany().HasForeignKey("BookId"),
                    join =>
                    {
                        join.HasKey("BookId", "CategoryId");
                        join.ToTable("BookCategories");
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(category => category.Name).HasMaxLength(100).IsRequired();
            entity.Property(category => category.Description).HasMaxLength(1000);
            entity.Property(category => category.Color).HasMaxLength(20);
            // Evita dos categorías con el mismo nombre aunque lleguen por peticiones distintas.
            entity.HasIndex(category => category.Name).IsUnique();
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(review => review.Comment).HasMaxLength(2000).IsRequired();
            entity.Property(review => review.CreatedAt).IsRequired();

            // Al borrar un libro o usuario también se eliminan sus reseñas.
            entity.HasOne(review => review.Book)
                .WithMany(book => book.Reviews)
                .HasForeignKey(review => review.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(review => review.User)
                .WithMany(user => user.Reviews)
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.CreatedAt).IsRequired();
            entity.Property(order => order.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(order => order.Total).HasPrecision(10, 2);
            entity.Property(order => order.PaymentMethod).HasMaxLength(40).IsRequired();
            entity.Property(order => order.PaymentLastFour).HasMaxLength(4);

            // Un usuario conserva todos sus pedidos; su histórico se consulta desde aquí.
            entity.HasOne(order => order.User)
                .WithMany(user => user.Orders)
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasPrecision(10, 2);
            entity.Property(item => item.BookTitle).HasMaxLength(200).IsRequired();

            // Borrar la cabecera borra sus líneas porque una línea no tiene sentido sin pedido.
            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Si se borra un libro, la línea conserva BookTitle y UnitPrice como histórico.
            entity.HasOne(item => item.Book)
                .WithMany(book => book.OrderItems)
                .HasForeignKey(item => item.BookId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Many-to-many User ↔ Book para los favoritos, mediante UserFavorites.
        modelBuilder.Entity<ApplicationUser>()
            .HasMany(user => user.FavoriteBooks)
            .WithMany(book => book.FavoriteUsers)
            .UsingEntity<Dictionary<string, object>>(
                "UserFavorites",
                right => right.HasOne<Book>().WithMany().HasForeignKey("BookId"),
                left => left.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId"),
                join =>
                {
                    join.HasKey("UserId", "BookId");
                    join.ToTable("UserFavorites");
                });
    }
}
