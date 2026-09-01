using BibliotecaAspNet.Models;
using BibliotecaAspNet.ViewModels.Orders;

namespace BibliotecaAspNet.Services;

public interface IOrderService
{
    Task<CheckoutResult> CheckoutAsync(
        string userId,
        IReadOnlyDictionary<int, int> quantities,
        CheckoutViewModel payment,
        CancellationToken cancellationToken = default);

    Task<List<Order>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetDetailsAsync(int id, string? userId, bool includeAllUsers, CancellationToken cancellationToken = default);
    Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<decimal> TotalForUserAsync(string userId, CancellationToken cancellationToken = default);
}
