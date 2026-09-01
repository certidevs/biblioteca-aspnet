using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Utilities;

/// <summary>
/// Ejemplo de lógica pura de dominio que no necesita conocer MVC ni EF Core.
/// </summary>
public sealed record BookStatistics(
    int ReviewCount,
    decimal AverageRating,
    int FiveStarReviews,
    decimal MinimumPrice,
    decimal MaximumPrice)
{
    /// <summary>Calcula estadísticas sin depender de MVC ni de EF Core.</summary>
    public static BookStatistics From(IEnumerable<Book> books)
    {
        var bookList = books.ToList();
        var reviews = bookList.SelectMany(book => book.Reviews).ToList();
        var prices = bookList.Select(book => book.Price).ToList();

        return new BookStatistics(
            ReviewCount: reviews.Count,
            AverageRating: reviews.Count == 0 ? 0m : Math.Round((decimal)reviews.Average(review => review.Rating), 2),
            FiveStarReviews: reviews.Count(review => review.Rating == 5),
            MinimumPrice: prices.Count == 0 ? 0m : prices.Min(),
            MaximumPrice: prices.Count == 0 ? 0m : prices.Max());
    }
}
