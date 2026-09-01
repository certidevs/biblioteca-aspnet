using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

/// <summary>Acceso a autores con sus libros y categorías relacionadas.</summary>
public sealed class AuthorRepository : EfRepository<Author>, IAuthorRepository
{
    /// <summary>Inicializa el repositorio con el contexto de la petición.</summary>
    public AuthorRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <summary>Construye el listado de autores y aplica el filtro opcional.</summary>
    public Task<List<Author>> SearchAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Authors
            .AsNoTracking()
            .Include(author => author.Books)
            .AsQueryable();

        // El filtro se añade solo cuando el usuario ha escrito algo en el buscador.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(author =>
                EF.Functions.Like(author.Name, $"%{value}%") ||
                (author.Nationality != null && EF.Functions.Like(author.Nationality, $"%{value}%")));
        }

        return query
            .OrderBy(author => author.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Carga el autor y las relaciones que necesita la vista de detalle.</summary>
    public Task<Author?> GetDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Context.Authors
            .AsNoTracking()
            .Include(author => author.Books)
            .ThenInclude(book => book.Categories)
            .SingleOrDefaultAsync(author => author.Id == id, cancellationToken);
    }

    /// <summary>Cuenta autores sin cargar sus libros en memoria.</summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Context.Authors.CountAsync(cancellationToken);
    }
}
