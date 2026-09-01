using BibliotecaAspNet.ViewModels.Cart;

namespace BibliotecaAspNet.Services;

public interface ICartService
{
    Task<CartViewModel> GetAsync(CancellationToken cancellationToken = default);
    Task<CartOperationResult> AddAsync(int bookId, int quantity = 1, CancellationToken cancellationToken = default);
    Task<CartOperationResult> SetQuantityAsync(int bookId, int quantity, CancellationToken cancellationToken = default);
    IReadOnlyDictionary<int, int> GetQuantities();
    int GetTotalQuantity();
    void Remove(int bookId);
    void Clear();
}
