using Microsoft.AspNetCore.Identity;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Usuario de la aplicación. Identity aporta UserName, Email, PasswordHash y roles.
/// Las colecciones de navegación conectan al usuario con las funcionalidades comunes
/// que cualquier proyecto de grupos puede reutilizar.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>Nombre que se muestra en la interfaz; no tiene por qué coincidir con el login.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Nombre aleatorio del avatar guardado en wwwroot/uploads/avatars.</summary>
    public string? AvatarFileName { get; set; }

    /// <summary>Fecha de alta, útil para auditoría y para mostrar actividad del usuario.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Permite desactivar una cuenta sin borrar su histórico.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Relación N:M: un usuario puede guardar muchos libros y un libro tener muchos favoritos.</summary>
    public ICollection<Book> FavoriteBooks { get; set; } = new List<Book>();

    /// <summary>Relación 1:N: un usuario puede escribir varias reseñas.</summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>Relación 1:N: un usuario puede realizar varios pedidos.</summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
