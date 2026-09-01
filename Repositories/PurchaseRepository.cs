using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

public sealed class PurchaseRepository : EfRepository<Purchase>, IPurchaseRepository
{
    public PurchaseRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<List<Purchase>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Purchases
            .AsNoTracking()
            .Include(purchase => purchase.Book)
            .Where(purchase => purchase.UserId == userId)
            .OrderByDescending(purchase => purchase.PurchasedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Purchases.CountAsync(purchase => purchase.UserId == userId, cancellationToken);
    }

    public async Task<decimal> TotalForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Purchases
            .Where(purchase => purchase.UserId == userId)
            .SumAsync(purchase => (decimal?)purchase.PriceAtPurchase, cancellationToken)
            ?? 0m;
    }
}
