using BibliotecaAspNet.ViewModels.Orders;

namespace BibliotecaAspNet.ViewModels.Cart;

public sealed class CheckoutPageViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public CheckoutViewModel Payment { get; set; } = new();
}
