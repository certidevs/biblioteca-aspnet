using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

public interface IAuthorRepository : IRepository<Author>
{
    Task<List<Author>> SearchAsync(string? search, CancellationToken cancellationToken = default);
    Task<Author?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
