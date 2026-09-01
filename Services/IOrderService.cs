using BibliotecaAspNet.Models;
using BibliotecaAspNet.ViewModels.Orders;

namespace BibliotecaAspNet.Services;

/// <summary>Casos de uso de checkout y consulta del histórico de pedidos.</summary>
public interface IOrderService
{
    /// <summary>Valida carrito y tarjeta demo y crea Order con sus OrderItem.</summary>
    Task<CheckoutResult> CheckoutAsync(
        string userId,
        IReadOnlyDictionary<int, int> quantities,
        CheckoutViewModel payment,
        CancellationToken cancellationToken = default);

    /// <summary>Lista los pedidos del usuario autenticado.</summary>
    Task<List<Order>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Lista todos los pedidos para un administrador.</summary>
    Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene un pedido aplicando la visibilidad de propietario o administrador.</summary>
    Task<Order?> GetDetailsAsync(int id, string? userId, bool includeAllUsers, CancellationToken cancellationToken = default);

    /// <summary>Cuenta pedidos de un usuario.</summary>
    Task<int> CountForUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Suma el gasto de pedidos pagados de un usuario.</summary>
    Task<decimal> TotalForUserAsync(string userId, CancellationToken cancellationToken = default);
}
