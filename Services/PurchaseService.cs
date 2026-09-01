using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;

namespace BibliotecaAspNet.Services;

public sealed class PurchaseService : IPurchaseService
{
    private readonly IBookRepository books;
    private readonly IPurchaseRepository purchases;

    public PurchaseService(IBookRepository books, IPurchaseRepository purchases)
    {
        this.books = books;
        this.purchases = purchases;
    }

    public async Task<Purchase?> BuyAsync(
        string userId,
        int bookId,
        CancellationToken cancellationToken = default)
    {
        var book = await books.GetByIdAsync(bookId, cancellationToken);
        if (book is null || !book.Available)
        {
            return null;
        }

        var purchase = new Purchase
        {
            UserId = userId,
            BookId = book.Id,
            PriceAtPurchase = book.Price,
            PurchasedAt = DateTime.UtcNow
        };
        await purchases.AddAsync(purchase, cancellationToken);
        await purchases.SaveChangesAsync(cancellationToken);
        return purchase;
    }

    public Task<List<Purchase>> GetForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return purchases.GetForUserAsync(userId, cancellationToken);
    }

    public Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return purchases.CountForUserAsync(userId, cancellationToken);
    }

    public Task<decimal> TotalForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return purchases.TotalForUserAsync(userId, cancellationToken);
    }
}
