using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Profile;

public sealed class ProfileViewModel
{
    public ApplicationUser User { get; init; } = null!;
    public List<Book> FavoriteBooks { get; init; } = new();
    public List<Review> Reviews { get; init; } = new();
    public List<Order> Orders { get; init; } = new();
    public decimal TotalSpent { get; init; }
}
