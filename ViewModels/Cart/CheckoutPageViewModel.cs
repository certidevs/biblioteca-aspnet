using BibliotecaAspNet.ViewModels.Orders;

namespace BibliotecaAspNet.ViewModels.Cart;

/// <summary>Une el resumen del carrito y el formulario de pago en una misma página.</summary>
public sealed class CheckoutPageViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public CheckoutViewModel Payment { get; set; } = new();
}
