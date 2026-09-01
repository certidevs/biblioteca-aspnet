using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Cabecera de una compra. Un pedido pertenece a un usuario y agrupa varias líneas.
/// Es el equivalente didáctico de Order en el proyecto de restaurantes.
/// </summary>
public sealed class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Range(0, 999999999.99)]
    public decimal Total { get; set; }

    [StringLength(40)]
    public string PaymentMethod { get; set; } = "Tarjeta demo";

    /// <summary>
    /// Solo se almacena el último bloque de la tarjeta ficticia. Nunca se guarda la tarjeta completa ni el CVV.
    /// </summary>
    [StringLength(4)]
    public string? PaymentLastFour { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
