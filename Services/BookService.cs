using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Operaciones específicas de libros: relación N:M, favoritos y portadas.
/// Las consultas EF Core están aquí de forma explícita y sin un repositorio artificial.
/// </summary>
public sealed class BookService
{
    private readonly ApplicationDbContext context;
    private readonly ImageStorage images;

    public BookService(ApplicationDbContext context, ImageStorage images)
    {
        this.context = context;
        this.images = images;
    }

    /// <summary>Busca libros y aplica únicamente los filtros elegidos en la página.</summary>
    public List<Book> Search(
        string? search,
        int? authorId,
        int? categoryId,
        bool? available,
        bool favoritesOnly,
        string? userId)
    {
        var query = context.Books
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

        return query.OrderBy(book => book.Title).ToList();
    }

    /// <summary>Carga toda la información que muestra la ficha pública.</summary>
    public Book? GetDetails(int id) => context.Books
        .AsNoTracking()
        .Include(book => book.Author)
        .Include(book => book.Categories)
        .Include(book => book.FavoriteUsers)
        .Include(book => book.Reviews)
        .ThenInclude(review => review.User)
        .AsSplitQuery()
        .SingleOrDefault(book => book.Id == id);

    /// <summary>Carga el libro rastreado para poder reconstruir las categorías al editar.</summary>
    public Book? GetForEdit(int id) => context.Books
        .Include(book => book.Categories)
        .SingleOrDefault(book => book.Id == id);

    /// <summary>Obtiene libros existentes para el carrito o el checkout.</summary>
    public List<Book> GetByIds(IEnumerable<int> ids)
    {
        var selectedIds = ids.Distinct().ToArray();
        return context.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .Where(book => selectedIds.Contains(book.Id))
            .OrderBy(book => book.Title)
            .ToList();
    }

    /// <summary>Crea el libro, resuelve las IDs del formulario y guarda la portada opcional.</summary>
    public void Create(Book book, IEnumerable<int> categoryIds, IFormFile? coverImage)
    {
        var author = context.Authors.Find(book.AuthorId)
            ?? throw new InvalidOperationException("El autor seleccionado no existe.");
        book.Author = author;
        book.Categories = GetCategories(categoryIds);

        string? newCoverFileName = null;
        if (coverImage is not null)
        {
            var upload = images.Save(coverImage, ImageFolder.BookCovers);
            if (!upload.Succeeded)
            {
                throw new InvalidOperationException(upload.Error);
            }

            newCoverFileName = upload.Image!.FileName;
            book.CoverImageFileName = newCoverFileName;
        }

        try
        {
            context.Books.Add(book);
            context.SaveChanges();
        }
        catch
        {
            images.Delete(ImageFolder.BookCovers, newCoverFileName);
            throw;
        }
    }

    /// <summary>Actualiza campos, asociaciones y portada de un libro existente.</summary>
    public bool Update(int id, Book book, IEnumerable<int> categoryIds, IFormFile? coverImage, bool removeCoverImage)
    {
        var existing = GetForEdit(id);
        var author = context.Authors.Find(book.AuthorId);
        if (existing is null || author is null)
        {
            return false;
        }

        var oldCoverFileName = existing.CoverImageFileName;
        string? newCoverFileName = null;
        if (coverImage is not null)
        {
            var upload = images.Save(coverImage, ImageFolder.BookCovers);
            if (!upload.Succeeded)
            {
                throw new InvalidOperationException(upload.Error);
            }

            newCoverFileName = upload.Image!.FileName;
        }

        existing.Title = book.Title;
        existing.Price = book.Price;
        existing.Available = book.Available;
        existing.PublishDate = book.PublishDate;
        existing.Isbn = book.Isbn;
        existing.Pages = book.Pages;
        existing.Language = book.Language;
        existing.Synopsis = book.Synopsis;
        existing.AuthorId = author.Id;
        existing.Author = author;
        existing.CoverImageFileName = newCoverFileName ?? (removeCoverImage ? null : oldCoverFileName);

        // Para actualizar N:M se reemplaza la colección por las categorías seleccionadas.
        existing.Categories.Clear();
        foreach (var category in GetCategories(categoryIds))
        {
            existing.Categories.Add(category);
        }

        try
        {
            context.SaveChanges();
        }
        catch
        {
            images.Delete(ImageFolder.BookCovers, newCoverFileName);
            throw;
        }

        if (!string.Equals(oldCoverFileName, existing.CoverImageFileName, StringComparison.Ordinal))
        {
            images.Delete(ImageFolder.BookCovers, oldCoverFileName);
        }

        return true;
    }

    /// <summary>Elimina el libro y su portada local.</summary>
    public bool Delete(int id)
    {
        var book = context.Books.Find(id);
        if (book is null)
        {
            return false;
        }

        var coverFileName = book.CoverImageFileName;
        context.Books.Remove(book);
        context.SaveChanges();
        images.Delete(ImageFolder.BookCovers, coverFileName);
        return true;
    }

    /// <summary>Añade o quita la fila N:M de favoritos y devuelve el estado final.</summary>
    public bool ToggleFavorite(int bookId, string userId)
    {
        var book = context.Books.Include(item => item.FavoriteUsers).SingleOrDefault(item => item.Id == bookId);
        var user = context.Users.Find(userId);
        if (book is null || user is null)
        {
            return false;
        }

        var favoriteUser = book.FavoriteUsers.SingleOrDefault(item => item.Id == userId);
        if (favoriteUser is null)
        {
            book.FavoriteUsers.Add(user);
            context.SaveChanges();
            return true;
        }

        book.FavoriteUsers.Remove(favoriteUser);
        context.SaveChanges();
        return false;
    }

    /// <summary>Cuenta libros para el dashboard.</summary>
    public int Count() => context.Books.Count();

    /// <summary>Convierte IDs de formulario en entidades reales de la relación N:M.</summary>
    private List<Category> GetCategories(IEnumerable<int> categoryIds)
    {
        var selectedIds = categoryIds.Distinct().ToArray();
        return context.Categories.Where(category => selectedIds.Contains(category.Id)).ToList();
    }
}
