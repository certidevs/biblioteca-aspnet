using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Books;

public sealed class BookListViewModel
{
    public string? Search { get; init; }
    public int? AuthorId { get; init; }
    public int? CategoryId { get; init; }
    public bool? Available { get; init; }
    public bool FavoritesOnly { get; init; }
    public string? CurrentUserId { get; init; }

    public List<Book> Books { get; init; } = new();
    public List<Author> Authors { get; init; } = new();
    public List<Category> Categories { get; init; } = new();
}
