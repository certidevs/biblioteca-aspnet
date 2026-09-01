using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;

namespace BibliotecaAspNet.Services;

/// <summary>Orquesta las operaciones de autores entre MVC y el repositorio.</summary>
public sealed class AuthorService : IAuthorService
{
    private readonly IAuthorRepository authors;

    /// <summary>Recibe el repositorio mediante inyección de dependencias.</summary>
    public AuthorService(IAuthorRepository authors)
    {
        this.authors = authors;
    }

    /// <summary>Delega la búsqueda al repositorio.</summary>
    public Task<List<Author>> SearchAsync(string? search, CancellationToken cancellationToken = default)
    {
        return authors.SearchAsync(search, cancellationToken);
    }

    /// <summary>Obtiene el detalle de un autor.</summary>
    public Task<Author?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return authors.GetDetailsAsync(id, cancellationToken);
    }

    /// <summary>Inserta el autor y confirma la unidad de trabajo.</summary>
    public async Task CreateAsync(Author author, CancellationToken cancellationToken = default)
    {
        await authors.AddAsync(author, cancellationToken);
        await authors.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Busca el autor existente y copia solo los campos editables del formulario.</summary>
    public async Task<bool> UpdateAsync(
        int id,
        Author author,
        CancellationToken cancellationToken = default)
    {
        var existing = await authors.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.Name = author.Name;
        existing.Bio = author.Bio;
        existing.BirthDate = author.BirthDate;
        existing.Nationality = author.Nationality;
        await authors.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Elimina el autor si existe.</summary>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var author = await authors.GetByIdAsync(id, cancellationToken);
        if (author is null)
        {
            return false;
        }

        authors.Delete(author);
        await authors.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Devuelve el total de autores.</summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return authors.CountAsync(cancellationToken);
    }
}
