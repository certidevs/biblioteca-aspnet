using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Services;

public interface IPurchaseService
{
    Task<Purchase?> BuyAsync(string userId, int bookId, CancellationToken cancellationToken = default);
    Task<List<Purchase>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<decimal> TotalForUserAsync(string userId, CancellationToken cancellationToken = default);
}
