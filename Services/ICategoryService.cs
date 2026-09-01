using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Services;

/// <summary>Casos de uso del catálogo de categorías.</summary>
public interface ICategoryService
{
    /// <summary>Busca categorías para el listado.</summary>
    Task<List<Category>> SearchAsync(string? search, CancellationToken cancellationToken = default);

    /// <summary>Obtiene una categoría con sus libros.</summary>
    Task<Category?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Crea una categoría respetando su nombre único.</summary>
    Task CreateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>Actualiza una categoría y comprueba que no duplique otro nombre.</summary>
    Task<bool> UpdateAsync(int id, Category category, CancellationToken cancellationToken = default);

    /// <summary>Elimina una categoría.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Cuenta categorías para el dashboard.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
