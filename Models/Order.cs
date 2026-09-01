using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Cabecera de una compra. Un pedido pertenece a un usuario y agrupa varias líneas.
/// Es el equivalente didáctico de Order en el proyecto de restaurantes.
/// </summary>
public sealed class Order
{
    /// <summary>Identificador generado por la base de datos.</summary>
    public int Id { get; set; }

    /// <summary>Momento de creación del pedido, almacenado en UTC.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Estado actual del pedido; el checkout demo lo deja en Paid.</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>Total calculado a partir de sus líneas, congelado al pagar.</summary>
    [Range(0, 999999999.99)]
    public decimal Total { get; set; }

    [StringLength(40)]
    public string PaymentMethod { get; set; } = "Tarjeta demo";

    /// <summary>
    /// Solo se almacena el último bloque de la tarjeta ficticia. Nunca se guarda la tarjeta completa ni el CVV.
    /// </summary>
    [StringLength(4)]
    public string? PaymentLastFour { get; set; }

    /// <summary>FK obligatoria: cada pedido pertenece a un usuario.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Usuario propietario del pedido; relación N:1.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Líneas compradas; relación 1:N con OrderItem.</summary>
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
