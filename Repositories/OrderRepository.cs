using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

/// <summary>Acceso a pedidos con sus líneas y reglas de visibilidad.</summary>
public sealed class OrderRepository : EfRepository<Order>, IOrderRepository
{
    /// <summary>Inicializa el repositorio con el contexto de la petición.</summary>
    public OrderRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <summary>Lista el histórico del usuario autenticado.</summary>
    public Task<List<Order>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Lista todos los pedidos para la administración.</summary>
    public Task<List<Order>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .Include(order => order.User)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Obtiene un pedido y aplica la autorización de propietario en la consulta.</summary>
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

    /// <summary>Cuenta pedidos del usuario para su perfil.</summary>
    public Task<int> CountForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Orders.CountAsync(order => order.UserId == userId, cancellationToken);
    }

    /// <summary>Suma pedidos pagados; un pedido pendiente no cuenta como gasto.</summary>
    public async Task<decimal> TotalForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Orders
            .Where(order => order.UserId == userId && order.Status == OrderStatus.Paid)
            .SumAsync(order => (decimal?)order.Total, cancellationToken)
            ?? 0m;
    }

    /// <summary>Consulta base reutilizada para no repetir los Includes de pedido y libro.</summary>
    private IQueryable<Order> QueryWithDetails()
    {
        return Context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .ThenInclude(item => item.Book)
            .AsSplitQuery();
    }
}
