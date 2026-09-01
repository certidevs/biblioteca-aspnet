using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

public sealed class ReviewRepository : EfRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<List<Review>> SearchAsync(
        int? rating,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Reviews
            .AsNoTracking()
            .Include(review => review.Book)
            .Include(review => review.User)
            .AsQueryable();

        if (rating.HasValue)
        {
            query = query.Where(review => review.Rating == rating.Value);
        }

        return query
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Review?> GetByIdWithRelationsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Context.Reviews
            .Include(review => review.Book)
            .Include(review => review.User)
            .SingleOrDefaultAsync(review => review.Id == id, cancellationToken);
    }

    public Task<List<Review>> GetForBookAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        return Context.Reviews
            .AsNoTracking()
            .Include(review => review.User)
            .Where(review => review.BookId == bookId)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Review>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Reviews
            .AsNoTracking()
            .Include(review => review.Book)
            .Where(review => review.UserId == userId)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
