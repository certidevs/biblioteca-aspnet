using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Repositories;

/// <summary>Acceso a libros y consultas del catálogo.</summary>
public sealed class BookRepository : EfRepository<Book>, IBookRepository
{
    /// <summary>Inicializa el repositorio con el contexto de la petición.</summary>
    public BookRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <summary>Compone la consulta de catálogo con filtros y relaciones de lectura.</summary>
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

        // Cada Where se traduce a SQL y solo se incorporan los filtros seleccionados.
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

    /// <summary>Carga la ficha completa de un libro, incluida la actividad de usuarios.</summary>
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

    /// <summary>Carga el libro con categorías rastreadas para editar la relación N:M.</summary>
    public Task<Book?> GetForEditAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return Context.Books
            .Include(book => book.Categories)
            .SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    /// <summary>Obtiene los libros existentes para un conjunto de IDs, como los del carrito.</summary>
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

    /// <summary>Cuenta libros directamente en la base de datos.</summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Context.Books.CountAsync(cancellationToken);
    }

    /// <summary>Comprueba el favorito sin cargar el libro completo.</summary>
    public Task<bool> IsFavoriteAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Context.Books
            .AnyAsync(book => book.Id == bookId && book.FavoriteUsers.Any(user => user.Id == userId), cancellationToken);
    }

    /// <summary>Modifica la tabla intermedia UserFavorites y devuelve el nuevo estado.</summary>
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

        // Las navegaciones permiten añadir o quitar la fila N:M de forma legible.
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
