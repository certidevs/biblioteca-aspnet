using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

/// <summary>Casos de uso del catálogo de autores para el controlador MVC.</summary>
public interface IAuthorService
{
    /// <summary>Busca autores para el listado público.</summary>
    Task<List<Author>> SearchAsync(string? search, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un autor con sus libros.</summary>
    Task<Author?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Crea un autor y guarda su fotografía opcional.</summary>
    Task CreateAsync(Author author, IFormFile? photo, CancellationToken cancellationToken = default);

    /// <summary>Actualiza un autor existente y sustituye o elimina su fotografía.</summary>
    Task<bool> UpdateAsync(
        int id,
        Author author,
        IFormFile? photo,
        bool removePhoto,
        CancellationToken cancellationToken = default);

    /// <summary>Elimina un autor y sus relaciones configuradas en EF Core.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Cuenta autores para el dashboard.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
