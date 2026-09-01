using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;

namespace BibliotecaAspNet.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository categories;

    public CategoryService(ICategoryRepository categories)
    {
        this.categories = categories;
    }

    public Task<List<Category>> SearchAsync(string? search, CancellationToken cancellationToken = default)
    {
        return categories.SearchAsync(search, cancellationToken);
    }

    public Task<Category?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return categories.GetDetailsAsync(id, cancellationToken);
    }

    public async Task CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        if (await categories.ExistsByNameAsync(category.Name, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
        }

        await categories.AddAsync(category, cancellationToken);
        await categories.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        int id,
        Category category,
        CancellationToken cancellationToken = default)
    {
        var existing = await categories.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        if (await categories.ExistsByNameAsync(category.Name, id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otra categoría con ese nombre.");
        }

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.Color = category.Color;
        await categories.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        categories.Delete(category);
        await categories.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return categories.CountAsync(cancellationToken);
    }
}
