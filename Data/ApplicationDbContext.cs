using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Data;

/// <summary>
/// Contexto de EF Core. Es el equivalente a un EntityManager de JPA + configuración ORM.
/// </summary>
public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Purchase> Purchases => Set<Purchase>();

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
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(book => book.Title).HasMaxLength(200).IsRequired();
            entity.Property(book => book.Price).HasPrecision(10, 2);
            entity.Property(book => book.Isbn).HasMaxLength(20);
            entity.Property(book => book.Language).HasMaxLength(60);
            entity.Property(book => book.Synopsis).HasMaxLength(5000);
            entity.Property(book => book.CoverImageFileName).HasMaxLength(260);

            entity.HasOne(book => book.Author)
                .WithMany(author => author.Books)
                .HasForeignKey(book => book.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

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
            entity.HasIndex(category => category.Name).IsUnique();
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(review => review.Comment).HasMaxLength(2000).IsRequired();
            entity.Property(review => review.CreatedAt).IsRequired();

            entity.HasOne(review => review.Book)
                .WithMany(book => book.Reviews)
                .HasForeignKey(review => review.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(review => review.User)
                .WithMany(user => user.Reviews)
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.Property(purchase => purchase.PurchasedAt).IsRequired();
            entity.Property(purchase => purchase.PriceAtPurchase).HasPrecision(10, 2);

            entity.HasOne(purchase => purchase.Book)
                .WithMany(book => book.Purchases)
                .HasForeignKey(purchase => purchase.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(purchase => purchase.User)
                .WithMany(user => user.Purchases)
                .HasForeignKey(purchase => purchase.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Many-to-many User ↔ Book para los favoritos.
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
