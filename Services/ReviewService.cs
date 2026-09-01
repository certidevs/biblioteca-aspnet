using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;

namespace BibliotecaAspNet.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository reviews;
    private readonly IBookRepository books;

    public ReviewService(IReviewRepository reviews, IBookRepository books)
    {
        this.reviews = reviews;
        this.books = books;
    }

    public Task<List<Review>> SearchAsync(int? rating, CancellationToken cancellationToken = default)
    {
        return reviews.SearchAsync(rating, cancellationToken);
    }

    public Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return reviews.GetByIdWithRelationsAsync(id, cancellationToken);
    }

    public async Task CreateAsync(
        Review review,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var book = await books.GetByIdAsync(review.BookId, cancellationToken)
            ?? throw new InvalidOperationException("El libro seleccionado no existe.");

        review.UserId = userId;
        review.Book = book;
        review.CreatedAt = DateTime.UtcNow;
        await reviews.AddAsync(review, cancellationToken);
        await reviews.SaveChangesAsync(cancellationToken);
    }

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

    public bool CanModify(Review review, string userId, bool isAdmin)
    {
        return isAdmin || review.UserId == userId;
    }
}
