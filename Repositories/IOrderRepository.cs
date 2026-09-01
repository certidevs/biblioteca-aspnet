using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

/// <summary>Consultas de pedidos con control de visibilidad por usuario o administrador.</summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Lista únicamente los pedidos del usuario autenticado.</summary>
    Task<List<Order>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Lista todos los pedidos con sus líneas para el panel de administración.</summary>
    Task<List<Order>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene un pedido y evita que un usuario vea el de otra cuenta.</summary>
    Task<Order?> GetDetailsAsync(int id, string? userId, bool includeAllUsers, CancellationToken cancellationToken = default);

    /// <summary>Cuenta los pedidos de un usuario para su perfil.</summary>
    Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Suma solo pedidos pagados para mostrar el gasto acumulado.</summary>
    Task<decimal> TotalForUserAsync(string userId, CancellationToken cancellationToken = default);
}
