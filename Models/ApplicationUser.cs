using Microsoft.AspNetCore.Identity;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Usuario de la aplicación. Identity aporta UserName, Email, PasswordHash y roles.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? AvatarFileName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Book> FavoriteBooks { get; set; } = new List<Book>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
