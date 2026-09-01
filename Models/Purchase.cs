namespace BibliotecaAspNet.Models;

public sealed class Purchase
{
    public int Id { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public decimal PriceAtPurchase { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
}
