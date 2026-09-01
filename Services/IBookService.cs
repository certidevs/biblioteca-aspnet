using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

/// <summary>Casos de uso del catálogo: CRUD, favoritos y portadas.</summary>
public interface IBookService
{
    /// <summary>Busca libros aplicando los filtros elegidos en la UI.</summary>
    Task<List<Book>> SearchAsync(
        string? search,
        int? authorId,
        int? categoryId,
        bool? available,
        bool favoritesOnly,
        string? userId,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene la ficha pública de un libro.</summary>
    Task<Book?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un libro y sus categorías para rellenar el formulario de edición.</summary>
    Task<Book?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Valida relaciones, guarda la portada y crea el libro.</summary>
    Task CreateAsync(
        Book book,
        IEnumerable<int> categoryIds,
        IFormFile? coverImage,
        CancellationToken cancellationToken = default);

    /// <summary>Actualiza datos, categorías y portada, limpiando la imagen anterior si procede.</summary>
    Task<bool> UpdateAsync(
        int id,
        Book book,
        IEnumerable<int> categoryIds,
        IFormFile? coverImage,
        bool removeCoverImage,
        CancellationToken cancellationToken = default);

    /// <summary>Elimina el libro y su portada almacenada.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Añade o quita un libro de los favoritos del usuario.</summary>
    Task<bool> ToggleFavoriteAsync(int bookId, string userId, CancellationToken cancellationToken = default);

    /// <summary>Cuenta libros para el dashboard.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
