namespace BibliotecaAspNet.ViewModels.Cart;

/// <summary>Modelo de la página del carrito y sus totales calculados.</summary>
public sealed class CartViewModel
{
    /// <summary>Líneas válidas recuperadas de la base de datos.</summary>
    public List<CartItemViewModel> Items { get; init; } = new();

    /// <summary>Total de unidades, usado también como contador de navegación.</summary>
    public int ItemCount => Items.Sum(item => item.Quantity);

    /// <summary>Suma de subtotales de las líneas.</summary>
    public decimal Total => Items.Sum(item => item.LineTotal);

    /// <summary>Indica si la vista debe mostrar el estado vacío.</summary>
    public bool IsEmpty => Items.Count == 0;
}
