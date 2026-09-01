using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Cart;

/// <summary>Una línea visual del carrito con el libro y sus unidades.</summary>
public sealed class CartItemViewModel
{
    /// <summary>Libro cargado desde el catálogo, no desde datos enviados por el navegador.</summary>
    public Book Book { get; init; } = null!;

    /// <summary>Unidades solicitadas para este libro.</summary>
    public int Quantity { get; init; }

    /// <summary>Subtotal visual de la línea usando el precio actual.</summary>
    public decimal LineTotal => Book.Price * Quantity;
}
