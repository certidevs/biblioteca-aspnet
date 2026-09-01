using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Home;

public sealed class DashboardViewModel
{
    public int BookCount { get; init; }
    public int AuthorCount { get; init; }
    public int CategoryCount { get; init; }
    public List<Book> FeaturedBooks { get; init; } = new();
}
