using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

public sealed class OrderRepository : EfRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<List<Order>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Order>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .Include(order => order.User)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Order?> GetDetailsAsync(
        int id,
        string? userId,
        bool includeAllUsers,
        CancellationToken cancellationToken = default)
    {
        var query = QueryWithDetails();
        if (!includeAllUsers)
        {
            query = query.Where(order => order.UserId == userId);
        }

        return query
            .Include(order => order.User)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public Task<int> CountForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Orders.CountAsync(order => order.UserId == userId, cancellationToken);
    }

    public async Task<decimal> TotalForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Orders
            .Where(order => order.UserId == userId && order.Status == OrderStatus.Paid)
            .SumAsync(order => (decimal?)order.Total, cancellationToken)
            ?? 0m;
    }

    private IQueryable<Order> QueryWithDetails()
    {
        return Context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .ThenInclude(item => item.Book)
            .AsSplitQuery();
    }
}
