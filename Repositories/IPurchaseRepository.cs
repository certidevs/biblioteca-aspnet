using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

public interface IPurchaseRepository : IRepository<Purchase>
{
    Task<List<Purchase>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<decimal> TotalForUserAsync(string userId, CancellationToken cancellationToken = default);
}
