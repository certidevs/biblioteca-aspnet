namespace BibliotecaAspNet.Models;

/// <summary>
/// Estado del pedido. El checkout de esta aplicación de referencia marca el pedido
/// como pagado porque el proveedor de pago es ficticio.
/// </summary>
public enum OrderStatus
{
    /// <summary>Pedido creado pero todavía no confirmado.</summary>
    Pending,

    /// <summary>Pago ficticio aceptado y pedido confirmado.</summary>
    Paid,

    /// <summary>Pedido cancelado.</summary>
    Cancelled
}
