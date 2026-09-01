using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> SearchAsync(string? search, CancellationToken cancellationToken = default);
    Task<Category?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Category>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, int? excludingId = null, CancellationToken cancellationToken = default);
}
