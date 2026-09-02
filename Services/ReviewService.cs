using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Services;

/// <summary>Operaciones de reseñas y la regla de permiso autor o administrador.</summary>
public sealed class ReviewService
{
    private readonly ApplicationDbContext context;

    public ReviewService(ApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>Lista reseñas con libro y autor, filtrando por puntuación si se pide.</summary>
    public List<Review> Search(int? rating)
    {
        var query = context.Reviews.AsNoTracking()
            .Include(review => review.Book)
            .Include(review => review.User)
            .AsQueryable();
        if (rating.HasValue)
        {
            query = query.Where(review => review.Rating == rating.Value);
        }

        return query.OrderByDescending(review => review.CreatedAt).ToList();
    }

    /// <summary>Carga una reseña y sus relaciones para editarla o comprobar permisos.</summary>
    public Review? GetById(int id) => context.Reviews
        .Include(review => review.Book)
        .Include(review => review.User)
        .SingleOrDefault(review => review.Id == id);

    /// <summary>Asocia la reseña al usuario autenticado, nunca a un ID del formulario.</summary>
    public void Create(Review review, string userId)
    {
        var book = context.Books.Find(review.BookId)
            ?? throw new InvalidOperationException("El libro seleccionado no existe.");

        review.UserId = userId;
        review.Book = book;
        review.CreatedAt = DateTime.UtcNow;
        context.Reviews.Add(review);
        context.SaveChanges();
    }

    /// <summary>Actualiza comentario y puntuación cuando la autorización es correcta.</summary>
    public bool Update(int id, Review review, string userId, bool isAdmin)
    {
        var existing = GetById(id);
        if (existing is null || !CanModify(existing, userId, isAdmin))
        {
            return false;
        }

        existing.Comment = review.Comment;
        existing.Rating = review.Rating;
        context.SaveChanges();
        return true;
    }

    /// <summary>Elimina una reseña solo a su autor o a un administrador.</summary>
    public bool Delete(int id, string userId, bool isAdmin)
    {
        var review = GetById(id);
        if (review is null || !CanModify(review, userId, isAdmin))
        {
            return false;
        }

        context.Reviews.Remove(review);
        context.SaveChanges();
        return true;
    }

    /// <summary>Expresa la regla de autorización en una sola condición legible.</summary>
    public bool CanModify(Review review, string userId, bool isAdmin) => isAdmin || review.UserId == userId;
}
