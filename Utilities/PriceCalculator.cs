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

    /// <summary>Calcula total aplicando descuento por volumen y fidelidad.</summary>
    public static decimal CalculateTotal(
        IEnumerable<Book> books,
        int previousOrders = 0)
    {
        var bookList = books.ToList();
        var total = bookList.Sum(book => book.Price);

        if (bookList.Count >= 3)
        {
            total *= 1 - VolumeDiscountRate;
        }

        if (previousOrders >= LoyaltyThreshold)
        {
            total *= 1 - LoyaltyDiscountRate;
        }

        return Round(total);
    }

    /// <summary>Calcula el impuesto didáctico de libros y redondea a dos decimales.</summary>
    public static decimal CalculateTax(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        return Round(price * BookTaxRate);
    }

    /// <summary>Devuelve un precio sumando el impuesto calculado.</summary>
    public static decimal PriceWithTax(decimal price)
    {
        return Round(price + CalculateTax(price));
    }

    /// <summary>Calcula cuánto se ahorra frente a comprar sin descuentos.</summary>
    public static decimal Savings(IEnumerable<Book> books)
    {
        var bookList = books.ToList();
        return Round(bookList.Sum(book => book.Price) - CalculateTotal(bookList));
    }

    /// <summary>Centraliza el redondeo monetario del ejemplo.</summary>
    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
