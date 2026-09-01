using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Línea de pedido: libro, unidades y precio congelado en el momento del checkout.
/// BookTitle permite conservar el histórico aunque un administrador elimine el libro.
/// </summary>
public sealed class OrderItem
{
    /// <summary>Identificador generado por la base de datos.</summary>
    public int Id { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;

    [Range(0, 999999999.99)]
    public decimal UnitPrice { get; set; }

    [Required, StringLength(200)]
    public string BookTitle { get; set; } = string.Empty;

    /// <summary>FK obligatoria hacia la cabecera del pedido.</summary>
    public int OrderId { get; set; }

    /// <summary>Pedido al que pertenece la línea; navegación inversa de <see cref="Order.Items"/>.</summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Nullable para poder borrar un libro del catálogo sin perder el histórico del pedido.
    /// </summary>
    public int? BookId { get; set; }

    /// <summary>Libro original, si todavía existe en el catálogo.</summary>
    public Book? Book { get; set; }

    /// <summary>Importe de la línea: precio congelado multiplicado por unidades.</summary>
    public decimal LineTotal => UnitPrice * Quantity;
}
