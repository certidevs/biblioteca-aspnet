using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Services;

public interface ICategoryService
{
    Task<List<Category>> SearchAsync(string? search, CancellationToken cancellationToken = default);
    Task<Category?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, Category category, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
