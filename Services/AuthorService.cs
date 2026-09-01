using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;
using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

/// <summary>Orquesta las operaciones de autores entre MVC y el repositorio.</summary>
public sealed class AuthorService : IAuthorService
{
    private readonly IAuthorRepository authors;
    private readonly IImageStorage images;

    /// <summary>Recibe el repositorio y el almacenamiento de imágenes mediante DI.</summary>
    public AuthorService(IAuthorRepository authors, IImageStorage images)
    {
        this.authors = authors;
        this.images = images;
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

    /// <summary>Inserta el autor y guarda su foto solo si la base de datos tiene éxito.</summary>
    public async Task CreateAsync(
        Author author,
        IFormFile? photo,
        CancellationToken cancellationToken = default)
    {
        string? newPhotoFileName = null;
        if (photo is not null)
        {
            var upload = await images.SaveAsync(photo, ImageFolder.AuthorPhotos, cancellationToken);
            if (!upload.Succeeded)
            {
                throw new InvalidOperationException(upload.Error);
            }

            newPhotoFileName = upload.Image!.FileName;
            author.PhotoFileName = newPhotoFileName;
        }

        try
        {
            await authors.AddAsync(author, cancellationToken);
            await authors.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Si falla la BD, eliminamos el archivo para no dejar basura en disco.
            images.Delete(ImageFolder.AuthorPhotos, newPhotoFileName);
            throw;
        }
    }

    /// <summary>Busca el autor existente y copia solo los campos editables del formulario.</summary>
    public async Task<bool> UpdateAsync(
        int id,
        Author author,
        IFormFile? photo,
        bool removePhoto,
        CancellationToken cancellationToken = default)
    {
        var existing = await authors.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        var oldPhotoFileName = existing.PhotoFileName;
        string? newPhotoFileName = null;
        if (photo is not null)
        {
            var upload = await images.SaveAsync(photo, ImageFolder.AuthorPhotos, cancellationToken);
            if (!upload.Succeeded)
            {
                throw new InvalidOperationException(upload.Error);
            }

            newPhotoFileName = upload.Image!.FileName;
        }

        existing.Name = author.Name;
        existing.Bio = author.Bio;
        existing.BirthDate = author.BirthDate;
        existing.Nationality = author.Nationality;
        existing.PhotoFileName = newPhotoFileName
            ?? (removePhoto ? null : oldPhotoFileName);

        try
        {
            await authors.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // La nueva foto solo se conserva cuando también se guardó el autor.
            images.Delete(ImageFolder.AuthorPhotos, newPhotoFileName);
            throw;
        }

        if (!string.Equals(oldPhotoFileName, existing.PhotoFileName, StringComparison.Ordinal))
        {
            images.Delete(ImageFolder.AuthorPhotos, oldPhotoFileName);
        }

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

        var photoFileName = author.PhotoFileName;
        authors.Delete(author);
        await authors.SaveChangesAsync(cancellationToken);
        images.Delete(ImageFolder.AuthorPhotos, photoFileName);
        return true;
    }

    /// <summary>Devuelve el total de autores.</summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return authors.CountAsync(cancellationToken);
    }
}
