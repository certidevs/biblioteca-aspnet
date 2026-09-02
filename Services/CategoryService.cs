using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Services;

/// <summary>Operaciones de categorías con la regla de nombre único.</summary>
public sealed class CategoryService
{
    private readonly ApplicationDbContext context;

    public CategoryService(ApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>Busca por nombre o descripción y carga el contador de libros.</summary>
    public List<Category> Search(string? search)
    {
        var query = context.Categories.AsNoTracking().Include(category => category.Books).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(category =>
                EF.Functions.Like(category.Name, $"%{value}%") ||
                (category.Description != null && EF.Functions.Like(category.Description, $"%{value}%")));
        }

        return query.OrderBy(category => category.Name).ToList();
    }

    /// <summary>Carga la categoría con los libros y autores de su detalle.</summary>
    public Category? GetDetails(int id) => context.Categories
        .AsNoTracking()
        .Include(category => category.Books)
        .ThenInclude(book => book.Author)
        .SingleOrDefault(category => category.Id == id);

    /// <summary>Inserta una categoría solo si no existe ese nombre.</summary>
    public void Create(Category category)
    {
        EnsureUniqueName(category.Name);
        context.Categories.Add(category);
        context.SaveChanges();
    }

    /// <summary>Actualiza la categoría y vuelve a comprobar su nombre único.</summary>
    public bool Update(int id, Category category)
    {
        var existing = context.Categories.Find(id);
        if (existing is null)
        {
            return false;
        }

        EnsureUniqueName(category.Name, id);
        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.Color = category.Color;
        context.SaveChanges();
        return true;
    }

    /// <summary>Elimina la categoría si existe.</summary>
    public bool Delete(int id)
    {
        var category = context.Categories.Find(id);
        if (category is null)
        {
            return false;
        }

        context.Categories.Remove(category);
        context.SaveChanges();
        return true;
    }

    /// <summary>Cuenta categorías para el panel inicial.</summary>
    public int Count() => context.Categories.Count();

    /// <summary>Centraliza la regla de negocio sin crear una capa de repositorio.</summary>
    private void EnsureUniqueName(string name, int? excludingId = null)
    {
        var normalizedName = name.Trim().ToLower();
        var exists = context.Categories.Any(category =>
            category.Name.ToLower() == normalizedName &&
            (!excludingId.HasValue || category.Id != excludingId.Value));
        if (exists)
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
        }
    }
}
