using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

public sealed class BookRepository : EfRepository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<List<Book>> SearchAsync(
        string? search,
        int? authorId,
        int? categoryId,
        bool? available,
        bool favoritesOnly,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .Include(book => book.Categories)
            .Include(book => book.FavoriteUsers)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(book =>
                EF.Functions.Like(book.Title, $"%{value}%") ||
                EF.Functions.Like(book.Author.Name, $"%{value}%"));
        }

        if (authorId.HasValue)
        {
            query = query.Where(book => book.AuthorId == authorId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(book => book.Categories.Any(category => category.Id == categoryId.Value));
        }

        if (available.HasValue)
        {
            query = query.Where(book => book.Available == available.Value);
        }

        if (favoritesOnly && !string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(book => book.FavoriteUsers.Any(user => user.Id == userId));
        }

        return query
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Book?> GetDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Context.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .Include(book => book.Categories)
            .Include(book => book.FavoriteUsers)
            .Include(book => book.Reviews)
            .ThenInclude(review => review.User)
            .AsSplitQuery()
            .SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public Task<Book?> GetForEditAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Context.Books
            .Include(book => book.Categories)
            .SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public Task<List<Book>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var selectedIds = ids.Distinct().ToArray();
        return Context.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .Where(book => selectedIds.Contains(book.Id))
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Context.Books.CountAsync(cancellationToken);
    }

    public Task<bool> IsFavoriteAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Books
            .AnyAsync(book => book.Id == bookId && book.FavoriteUsers.Any(user => user.Id == userId), cancellationToken);
    }

    public async Task<bool> ToggleFavoriteAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var book = await Context.Books
            .Include(item => item.FavoriteUsers)
            .SingleOrDefaultAsync(item => item.Id == bookId, cancellationToken);
        var user = await Context.Users.FindAsync(new object?[] { userId }, cancellationToken);

        if (book is null || user is null)
        {
            return false;
        }

        var alreadyFavorite = book.FavoriteUsers.Any(item => item.Id == userId);
        if (alreadyFavorite)
        {
            var favoriteUser = book.FavoriteUsers.Single(item => item.Id == userId);
            book.FavoriteUsers.Remove(favoriteUser);
        }
        else
        {
            book.FavoriteUsers.Add(user);
        }

        await Context.SaveChangesAsync(cancellationToken);
        return !alreadyFavorite;
    }
}
