using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

/// <summary>Acceso a reseñas junto con el libro y usuario que las contextualizan.</summary>
public sealed class ReviewRepository : EfRepository<Review>, IReviewRepository
{
    /// <summary>Inicializa el repositorio con el contexto de la petición.</summary>
    public ReviewRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <summary>Lista reseñas y filtra por puntuación cuando se solicita.</summary>
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

    /// <summary>Carga una reseña con sus relaciones para editarla o autorizarla.</summary>
    public Task<Review?> GetByIdWithRelationsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Context.Reviews
            .Include(review => review.Book)
            .Include(review => review.User)
            .SingleOrDefaultAsync(review => review.Id == id, cancellationToken);
    }

    /// <summary>Lista las reseñas de un libro, de más nueva a más antigua.</summary>
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

    /// <summary>Lista las reseñas escritas por un usuario.</summary>
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
