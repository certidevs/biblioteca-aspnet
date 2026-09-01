using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

public interface IBookService
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
    Task CreateAsync(
        Book book,
        IEnumerable<int> categoryIds,
        IFormFile? coverImage,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        Book book,
        IEnumerable<int> categoryIds,
        IFormFile? coverImage,
        bool removeCoverImage,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ToggleFavoriteAsync(int bookId, string userId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
