using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

public sealed class AuthorRepository : EfRepository<Author>, IAuthorRepository
{
    public AuthorRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<List<Author>> SearchAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Authors
            .AsNoTracking()
            .Include(author => author.Books)
            .AsQueryable();

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

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Context.Authors.CountAsync(cancellationToken);
    }
}
