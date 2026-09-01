using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

/// <summary>Consultas específicas de categorías.</summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>Busca categorías por nombre o descripción e incluye el número de libros.</summary>
    Task<List<Category>> SearchAsync(string? search, CancellationToken cancellationToken = default);

    /// <summary>Obtiene una categoría con sus libros y autores.</summary>
    Task<Category?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Recupera categorías por IDs para reconstruir una relación N:M.</summary>
    Task<List<Category>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    /// <summary>Cuenta categorías para las estadísticas del dashboard.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Comprueba el nombre único, ignorando el ID que se está editando.</summary>
    Task<bool> ExistsByNameAsync(string name, int? excludingId = null, CancellationToken cancellationToken = default);
}
