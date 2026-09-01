using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;
using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository books;
    private readonly IAuthorRepository authors;
    private readonly ICategoryRepository categories;
    private readonly IImageStorage images;

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

    public Task<Book?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return books.GetDetailsAsync(id, cancellationToken);
    }

    public Task<Book?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        return books.GetForEditAsync(id, cancellationToken);
    }

    public async Task CreateAsync(
        Book book,
        IEnumerable<int> categoryIds,
        IFormFile? coverImage,
        CancellationToken cancellationToken = default)
    {
        var author = await authors.GetByIdAsync(book.AuthorId, cancellationToken)
            ?? throw new InvalidOperationException("El autor seleccionado no existe.");
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
            images.Delete(ImageFolder.BookCovers, newCoverFileName);
            throw;
        }
    }

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
            images.Delete(ImageFolder.BookCovers, newCoverFileName);
            throw;
        }

        if (!string.Equals(oldCoverFileName, existing.CoverImageFileName, StringComparison.Ordinal))
        {
            images.Delete(ImageFolder.BookCovers, oldCoverFileName);
        }

        return true;
    }

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

    public Task<bool> ToggleFavoriteAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return books.ToggleFavoriteAsync(bookId, userId, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return books.CountAsync(cancellationToken);
    }
}
