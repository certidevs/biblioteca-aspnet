using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;
using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Casos de uso de libros. Aquí se coordinan repositorios, categorías y almacenamiento
/// de imágenes antes de que el controlador devuelva una respuesta.
/// </summary>
public sealed class BookService : IBookService
{
    private readonly IBookRepository books;
    private readonly IAuthorRepository authors;
    private readonly ICategoryRepository categories;
    private readonly IImageStorage images;

    /// <summary>Recibe las dependencias necesarias mediante inyección de dependencias.</summary>
    public BookService(
        IBookRepository books,
        IAuthorRepository authors,
        ICategoryRepository categories,
        IImageStorage images)
    {
        this.books = books;
        this.authors = authors;
        this.categories = categories;
        this.images = images;
    }

    /// <summary>Busca libros con los filtros del catálogo.</summary>
    public Task<List<Book>> SearchAsync(
        string? search,
        int? authorId,
        int? categoryId,
        bool? available,
        bool favoritesOnly,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        return books.SearchAsync(
            search,
            authorId,
            categoryId,
            available,
            favoritesOnly,
            userId,
            cancellationToken);
    }

    /// <summary>Obtiene la ficha pública de un libro.</summary>
    public Task<Book?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return books.GetDetailsAsync(id, cancellationToken);
    }

    /// <summary>Obtiene un libro con categorías rastreadas para el formulario de edición.</summary>
    public Task<Book?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        return books.GetForEditAsync(id, cancellationToken);
    }

    /// <summary>Valida autor, resuelve categorías y guarda libro y portada.</summary>
    public async Task CreateAsync(
        Book book,
        IEnumerable<int> categoryIds,
        IFormFile? coverImage,
        CancellationToken cancellationToken = default)
    {
        var author = await authors.GetByIdAsync(book.AuthorId, cancellationToken)
            ?? throw new InvalidOperationException("El autor seleccionado no existe.");
        // Se cargan entidades existentes: el formulario solo envía sus IDs.
        var selectedCategories = await categories.GetByIdsAsync(categoryIds, cancellationToken);

        book.Author = author;
        book.Categories = selectedCategories;

        string? newCoverFileName = null;
        if (coverImage is not null)
        {
            var upload = await images.SaveAsync(
                coverImage,
                ImageFolder.BookCovers,
                cancellationToken);
            if (!upload.Succeeded)
            {
                throw new InvalidOperationException(upload.Error);
            }

            newCoverFileName = upload.Image!.FileName;
            book.CoverImageFileName = newCoverFileName;
        }

        try
        {
            await books.AddAsync(book, cancellationToken);
            await books.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Si falla la BD, no dejamos en disco una imagen huérfana.
            images.Delete(ImageFolder.BookCovers, newCoverFileName);
            throw;
        }
    }

    /// <summary>Actualiza campos, asociaciones y portada de un libro existente.</summary>
    public async Task<bool> UpdateAsync(
        int id,
        Book book,
        IEnumerable<int> categoryIds,
        IFormFile? coverImage,
        bool removeCoverImage,
        CancellationToken cancellationToken = default)
    {
        var existing = await books.GetForEditAsync(id, cancellationToken);
        var author = await authors.GetByIdAsync(book.AuthorId, cancellationToken);
        if (existing is null || author is null)
        {
            return false;
        }

        var oldCoverFileName = existing.CoverImageFileName;
        string? newCoverFileName = null;
        if (coverImage is not null)
        {
            var upload = await images.SaveAsync(
                coverImage,
                ImageFolder.BookCovers,
                cancellationToken);
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
        existing.CoverImageFileName = newCoverFileName
            ?? (removeCoverImage ? null : oldCoverFileName);
        existing.AuthorId = author.Id;
        existing.Author = author;

        // Se reconstruye la colección N:M a partir de los IDs seleccionados.
        existing.Categories.Clear();
        foreach (var category in await categories.GetByIdsAsync(categoryIds, cancellationToken))
        {
            existing.Categories.Add(category);
        }

        try
        {
            await books.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // La nueva portada solo se conserva si también se guardó el libro.
            images.Delete(ImageFolder.BookCovers, newCoverFileName);
            throw;
        }

        if (!string.Equals(oldCoverFileName, existing.CoverImageFileName, StringComparison.Ordinal))
        {
            images.Delete(ImageFolder.BookCovers, oldCoverFileName);
        }

        return true;
    }

    /// <summary>Elimina un libro y su portada si la operación tiene éxito.</summary>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await books.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return false;
        }

        var coverFileName = book.CoverImageFileName;
        books.Delete(book);
        await books.SaveChangesAsync(cancellationToken);
        images.Delete(ImageFolder.BookCovers, coverFileName);
        return true;
    }

    /// <summary>Cambia el favorito del usuario autenticado.</summary>
    public Task<bool> ToggleFavoriteAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return books.ToggleFavoriteAsync(bookId, userId, cancellationToken);
    }

    /// <summary>Devuelve el total de libros.</summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return books.CountAsync(cancellationToken);
    }
}
