namespace BibliotecaAspNet.ViewModels.Cart;

public sealed class CartViewModel
{
    public List<CartItemViewModel> Items { get; init; } = new();
    public int ItemCount => Items.Sum(item => item.Quantity);
    public decimal Total => Items.Sum(item => item.LineTotal);
    public bool IsEmpty => Items.Count == 0;
}
