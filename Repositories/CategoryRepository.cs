using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

/// <summary>Acceso a categorías y a los libros que agrupan.</summary>
public sealed class CategoryRepository : EfRepository<Category>, ICategoryRepository
{
    /// <summary>Inicializa el repositorio con el contexto de la petición.</summary>
    public CategoryRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <summary>Construye el listado de categorías con el filtro opcional.</summary>
    public Task<List<Category>> SearchAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Categories
            .AsNoTracking()
            .Include(category => category.Books)
            .AsQueryable();

        // La misma caja de búsqueda mira el nombre y la descripción.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(category =>
                EF.Functions.Like(category.Name, $"%{value}%") ||
                (category.Description != null && EF.Functions.Like(category.Description, $"%{value}%")));
        }

        return query
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Carga una categoría con libros y autores para su página de detalle.</summary>
    public Task<Category?> GetDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Context.Categories
            .AsNoTracking()
            .Include(category => category.Books)
            .ThenInclude(book => book.Author)
            .SingleOrDefaultAsync(category => category.Id == id, cancellationToken);
    }

    /// <summary>Recupera categorías existentes para reconstruir un vínculo N:M.</summary>
    public Task<List<Category>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var selectedIds = ids.Distinct().ToArray();
        return Context.Categories
            .Where(category => selectedIds.Contains(category.Id))
            .ToListAsync(cancellationToken);
    }

    /// <summary>Cuenta categorías directamente en la base de datos.</summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Context.Categories.CountAsync(cancellationToken);
    }

    /// <summary>Comprueba el nombre ignorando el registro actual durante una edición.</summary>
    public Task<bool> ExistsByNameAsync(
        string name,
        int? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();
        var query = Context.Categories
            .Where(category => category.Name.ToLower() == normalizedName);

        if (excludingId.HasValue)
        {
            query = query.Where(category => category.Id != excludingId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }
}
