using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;

namespace BibliotecaAspNet.Services;

public sealed class AuthorService : IAuthorService
{
    private readonly IAuthorRepository authors;

    public AuthorService(IAuthorRepository authors)
    {
        this.authors = authors;
    }

    public Task<List<Author>> SearchAsync(string? search, CancellationToken cancellationToken = default)
    {
        return authors.SearchAsync(search, cancellationToken);
    }

    public Task<Author?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return authors.GetDetailsAsync(id, cancellationToken);
    }

    public async Task CreateAsync(Author author, CancellationToken cancellationToken = default)
    {
        await authors.AddAsync(author, cancellationToken);
        await authors.SaveChangesAsync(cancellationToken);
    }

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

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return authors.CountAsync(cancellationToken);
    }
}
