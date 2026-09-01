using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetDetailsAsync(int id, string? userId, bool includeAllUsers, CancellationToken cancellationToken = default);
    Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<decimal> TotalForUserAsync(string userId, CancellationToken cancellationToken = default);
}
