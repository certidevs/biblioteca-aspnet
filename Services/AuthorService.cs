using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Operaciones de autores que no encajan en una acción MVC: guardar o retirar su foto.
/// Usa <see cref="ApplicationDbContext"/> directamente: EF Core ya implementa el
/// patrón repositorio y añadir otra capa no aporta nada a este proyecto docente.
/// </summary>
public sealed class AuthorService
{
    private readonly ApplicationDbContext context;
    private readonly ImageStorage images;

    public AuthorService(ApplicationDbContext context, ImageStorage images)
    {
        this.context = context;
        this.images = images;
    }

    /// <summary>Busca autores por nombre o nacionalidad para el listado.</summary>
    public List<Author> Search(string? search)
    {
        var query = context.Authors.AsNoTracking().Include(author => author.Books).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(author =>
                EF.Functions.Like(author.Name, $"%{value}%") ||
                (author.Nationality != null && EF.Functions.Like(author.Nationality, $"%{value}%")));
        }

        return query.OrderBy(author => author.Name).ToList();
    }

    /// <summary>Carga el autor y los libros que necesita su página de detalle.</summary>
    public Author? GetDetails(int id) => context.Authors
        .AsNoTracking()
        .Include(author => author.Books)
        .ThenInclude(book => book.Categories)
        .SingleOrDefault(author => author.Id == id);

    /// <summary>Guarda un autor y su foto opcional.</summary>
    public void Create(Author author, IFormFile? photo)
    {
        string? newPhotoFileName = null;
        if (photo is not null)
        {
            var upload = images.Save(photo, ImageFolder.AuthorPhotos);
            if (!upload.Succeeded)
            {
                throw new InvalidOperationException(upload.Error);
            }

            newPhotoFileName = upload.Image!.FileName;
            author.PhotoFileName = newPhotoFileName;
        }

        try
        {
            context.Authors.Add(author);
            context.SaveChanges();
        }
        catch
        {
            // Si falla SQLite, no se deja una foto sin autor en el disco.
            images.Delete(ImageFolder.AuthorPhotos, newPhotoFileName);
            throw;
        }
    }

    /// <summary>Actualiza solo los datos editables y gestiona el cambio de foto.</summary>
    public bool Update(int id, Author author, IFormFile? photo, bool removePhoto)
    {
        var existing = context.Authors.Find(id);
        if (existing is null)
        {
            return false;
        }

        var oldPhotoFileName = existing.PhotoFileName;
        string? newPhotoFileName = null;
        if (photo is not null)
        {
            var upload = images.Save(photo, ImageFolder.AuthorPhotos);
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
        existing.PhotoFileName = newPhotoFileName ?? (removePhoto ? null : oldPhotoFileName);

        try
        {
            context.SaveChanges();
        }
        catch
        {
            images.Delete(ImageFolder.AuthorPhotos, newPhotoFileName);
            throw;
        }

        if (!string.Equals(oldPhotoFileName, existing.PhotoFileName, StringComparison.Ordinal))
        {
            images.Delete(ImageFolder.AuthorPhotos, oldPhotoFileName);
        }

        return true;
    }

    /// <summary>Elimina el autor y, después, su foto local.</summary>
    public bool Delete(int id)
    {
        var author = context.Authors.Find(id);
        if (author is null)
        {
            return false;
        }

        var photoFileName = author.PhotoFileName;
        context.Authors.Remove(author);
        context.SaveChanges();
        images.Delete(ImageFolder.AuthorPhotos, photoFileName);
        return true;
    }

    /// <summary>Cuenta autores para el panel inicial.</summary>
    public int Count() => context.Authors.Count();
}
