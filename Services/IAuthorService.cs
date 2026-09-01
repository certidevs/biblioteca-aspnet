using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Services;

public interface IAuthorService
{
    Task<List<Author>> SearchAsync(string? search, CancellationToken cancellationToken = default);
    Task<Author?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(Author author, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, Author author, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
