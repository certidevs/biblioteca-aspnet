using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Services;

/// <summary>Resultado del checkout: pedido creado o mensaje de validación.</summary>
public sealed record CheckoutResult(Order? Order = null, string? Error = null)
{
    /// <summary>Indica que la operación produjo un pedido persistido.</summary>
    public bool Succeeded => Order is not null;
}
