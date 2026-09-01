using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Profile;

/// <summary>Resumen de un usuario con sus relaciones de favoritos, reseñas y pedidos.</summary>
public sealed class ProfileViewModel
{
    /// <summary>Usuario de Identity que se está mostrando.</summary>
    public ApplicationUser User { get; init; } = null!;

    /// <summary>Libros favoritos del usuario: relación N:M.</summary>
    public List<Book> FavoriteBooks { get; init; } = new();

    /// <summary>Reseñas escritas por el usuario: relación 1:N.</summary>
    public List<Review> Reviews { get; init; } = new();

    /// <summary>Pedidos del usuario: relación 1:N.</summary>
    public List<Order> Orders { get; init; } = new();

    /// <summary>Suma de pedidos pagados.</summary>
    public decimal TotalSpent { get; init; }
}
