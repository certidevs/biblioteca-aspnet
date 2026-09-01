using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Cart;

public sealed class CartItemViewModel
{
    public Book Book { get; init; } = null!;
    public int Quantity { get; init; }
    public decimal LineTotal => Book.Price * Quantity;
}
