using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;

namespace BibliotecaAspNet.Services;

/// <summary>Orquesta reseñas y centraliza la regla autor/administrador.</summary>
public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository reviews;
    private readonly IBookRepository books;

    /// <summary>Recibe los repositorios necesarios para reseñas y libros.</summary>
    public ReviewService(IReviewRepository reviews, IBookRepository books)
    {
        this.reviews = reviews;
        this.books = books;
    }

    /// <summary>Devuelve las reseñas filtradas.</summary>
    public Task<List<Review>> SearchAsync(int? rating, CancellationToken cancellationToken = default)
    {
        return reviews.SearchAsync(rating, cancellationToken);
    }

    /// <summary>Obtiene una reseña con sus relaciones.</summary>
    public Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return reviews.GetByIdWithRelationsAsync(id, cancellationToken);
    }

    /// <summary>Comprueba el libro y asocia la reseña al usuario autenticado.</summary>
    public async Task CreateAsync(
        Review review,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var book = await books.GetByIdAsync(review.BookId, cancellationToken)
            ?? throw new InvalidOperationException("El libro seleccionado no existe.");

        // El UserId viene de Claims, nunca del formulario, para evitar suplantaciones.
        review.UserId = userId;
        review.Book = book;
        review.CreatedAt = DateTime.UtcNow;
        await reviews.AddAsync(review, cancellationToken);
        await reviews.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Actualiza solo comentario y puntuación tras comprobar permisos.</summary>
    public async Task<bool> UpdateAsync(
        int id,
        Review review,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var existing = await reviews.GetByIdWithRelationsAsync(id, cancellationToken);
        if (existing is null || !CanModify(existing, userId, isAdmin))
        {
            return false;
        }

        existing.Comment = review.Comment;
        existing.Rating = review.Rating;
        await reviews.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Elimina la reseña tras comprobar permisos.</summary>
    public async Task<bool> DeleteAsync(
        int id,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var review = await reviews.GetByIdWithRelationsAsync(id, cancellationToken);
        if (review is null || !CanModify(review, userId, isAdmin))
        {
            return false;
        }

        reviews.Delete(review);
        await reviews.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Devuelve si el usuario es autor de la reseña o administrador.</summary>
    public bool CanModify(Review review, string userId, bool isAdmin)
    {
        return isAdmin || review.UserId == userId;
    }
}
