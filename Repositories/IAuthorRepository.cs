using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.Repositories;

/// <summary>Consultas de autores que no forman parte del CRUD común.</summary>
public interface IAuthorRepository : IRepository<Author>
{
    /// <summary>Busca autores por nombre o nacionalidad e incluye sus libros.</summary>
    Task<List<Author>> SearchAsync(string? search, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un autor con libros y categorías para su página de detalle.</summary>
    Task<Author?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Cuenta autores para las estadísticas del dashboard.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
