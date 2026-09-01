using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Services;

public sealed record CheckoutResult(Order? Order = null, string? Error = null)
{
    public bool Succeeded => Order is not null;
}
