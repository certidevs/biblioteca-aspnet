using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

public interface IBookRepository : IRepository<Book>
{
    Task<List<Book>> SearchAsync(
        string? search,
        int? authorId,
        int? categoryId,
        bool? available,
        bool favoritesOnly,
        string? userId,
        CancellationToken cancellationToken = default);

    Task<Book?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Book?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Book>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteAsync(int bookId, string userId, CancellationToken cancellationToken = default);
    Task<bool> ToggleFavoriteAsync(int bookId, string userId, CancellationToken cancellationToken = default);
}
