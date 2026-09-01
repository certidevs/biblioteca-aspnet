using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

public sealed class CategoryRepository : EfRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<List<Category>> SearchAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Categories
            .AsNoTracking()
            .Include(category => category.Books)
            .AsQueryable();

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

    public Task<List<Category>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var selectedIds = ids.Distinct().ToArray();
        return Context.Categories
            .Where(category => selectedIds.Contains(category.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Context.Categories.CountAsync(cancellationToken);
    }

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
