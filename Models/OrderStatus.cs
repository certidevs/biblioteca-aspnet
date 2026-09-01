namespace BibliotecaAspNet.Models;

/// <summary>
/// Estado del pedido. El checkout de esta aplicación de referencia marca el pedido
/// como pagado porque el proveedor de pago es ficticio.
/// </summary>
public enum OrderStatus
{
    Pending,
    Paid,
    Cancelled
}
