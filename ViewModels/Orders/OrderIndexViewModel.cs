using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Orders;

/// <summary>Pedidos y contexto de la página: propio o administrativo.</summary>
public sealed class OrderIndexViewModel
{
    public bool IsAdminView { get; init; }
    public List<Order> Orders { get; init; } = [];
}
