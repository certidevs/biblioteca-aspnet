using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Services;

/// <summary>Casos de uso de reseñas, incluyendo la autorización de sus autores.</summary>
public interface IReviewService
{
    /// <summary>Lista reseñas públicas y permite filtrarlas por puntuación.</summary>
    Task<List<Review>> SearchAsync(int? rating, CancellationToken cancellationToken = default);

    /// <summary>Obtiene una reseña con libro y usuario.</summary>
    Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Crea una reseña asociándola al usuario autenticado.</summary>
    Task CreateAsync(Review review, string userId, CancellationToken cancellationToken = default);

    /// <summary>Actualiza una reseña si el usuario es su autor o administrador.</summary>
    Task<bool> UpdateAsync(int id, Review review, string userId, bool isAdmin, CancellationToken cancellationToken = default);

    /// <summary>Elimina una reseña si el usuario es su autor o administrador.</summary>
    Task<bool> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken cancellationToken = default);

    /// <summary>Centraliza la regla de permisos de edición y borrado.</summary>
    bool CanModify(Review review, string userId, bool isAdmin);
}
