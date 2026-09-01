using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Utilities;

/// <summary>
/// Ejemplo de reglas de precio reutilizables para practicar lógica de negocio.
/// </summary>
public static class PriceCalculator
{
    private const decimal BookTaxRate = 0.04m;
    private const decimal VolumeDiscountRate = 0.10m;
    private const decimal LoyaltyDiscountRate = 0.05m;
    private const int LoyaltyThreshold = 5;

    public static decimal CalculateTotal(
        IEnumerable<Book> books,
        int previousPurchases = 0)
    {
        var bookList = books.ToList();
        var total = bookList.Sum(book => book.Price);

        if (bookList.Count >= 3)
        {
            total *= 1 - VolumeDiscountRate;
        }

        if (previousPurchases >= LoyaltyThreshold)
        {
            total *= 1 - LoyaltyDiscountRate;
        }

        return Round(total);
    }

    public static decimal CalculateTax(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        return Round(price * BookTaxRate);
    }

    public static decimal PriceWithTax(decimal price)
    {
        return Round(price + CalculateTax(price));
    }

    public static decimal Savings(IEnumerable<Book> books)
    {
        var bookList = books.ToList();
        return Round(bookList.Sum(book => book.Price) - CalculateTotal(bookList));
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
