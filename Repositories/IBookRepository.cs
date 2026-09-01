using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

/// <summary>Consultas específicas del catálogo de libros.</summary>
public interface IBookRepository : IRepository<Book>
{
    /// <summary>Aplica filtros de catálogo y, opcionalmente, limita el resultado a favoritos del usuario.</summary>
    Task<List<Book>> SearchAsync(
        string? search,
        int? authorId,
        int? categoryId,
        bool? available,
        bool favoritesOnly,
        string? userId,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene un libro con autor, categorías, favoritos y reseñas para la ficha completa.</summary>
    Task<Book?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un libro con las relaciones necesarias para editarlo.</summary>
    Task<Book?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Recupera varios libros por sus IDs, por ejemplo los del carrito.</summary>
    Task<List<Book>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    /// <summary>Cuenta libros para las estadísticas del dashboard.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Comprueba si un libro pertenece a los favoritos de un usuario.</summary>
    Task<bool> IsFavoriteAsync(int bookId, string userId, CancellationToken cancellationToken = default);

    /// <summary>Añade o quita el favorito y devuelve el nuevo estado.</summary>
    Task<bool> ToggleFavoriteAsync(int bookId, string userId, CancellationToken cancellationToken = default);
}
