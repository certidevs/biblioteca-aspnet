using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

/// <summary>Consultas específicas de reseñas y sus relaciones.</summary>
public interface IReviewRepository : IRepository<Review>
{
    /// <summary>Lista reseñas y permite filtrar por puntuación.</summary>
    Task<List<Review>> SearchAsync(int? rating, CancellationToken cancellationToken = default);

    /// <summary>Obtiene una reseña con el libro y usuario necesarios para editarla.</summary>
    Task<Review?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Lista las reseñas de un libro ordenadas por fecha.</summary>
    Task<List<Review>> GetForBookAsync(int bookId, CancellationToken cancellationToken = default);

    /// <summary>Lista las reseñas que ha escrito un usuario.</summary>
    Task<List<Review>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
}
