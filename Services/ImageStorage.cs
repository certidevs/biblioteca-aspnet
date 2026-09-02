using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Almacenamiento local sencillo para el aula. La base de datos guarda solo un nombre
/// generado por la aplicación; extensión, tamaño y firma se validan antes de escribir.
/// </summary>
public sealed class ImageStorage
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    private readonly string webRootPath;
    private readonly ImageStorageOptions options;
    private readonly ILogger<ImageStorage> logger;

    public ImageStorage(
        IWebHostEnvironment environment,
        IOptions<ImageStorageOptions> options,
        ILogger<ImageStorage> logger)
    {
        webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        this.options = options.Value;
        this.logger = logger;
    }

    /// <summary>Valida y guarda el archivo con nombre aleatorio para no usar el del navegador.</summary>
    public ImageUploadResult Save(IFormFile file, ImageFolder folder)
    {
        if (file.Length <= 0)
        {
            return ImageUploadResult.Failure("Selecciona una imagen.");
        }

        if (file.Length > options.MaxFileSizeBytes)
        {
            var megabytes = options.MaxFileSizeBytes / 1024d / 1024d;
            return ImageUploadResult.Failure($"La imagen no puede superar {megabytes:0.#} MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = options.AllowedExtensions
            .Select(value => value.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!allowedExtensions.Contains(extension))
        {
            return ImageUploadResult.Failure("Solo se permiten imágenes JPG, PNG, GIF o WEBP.");
        }

        // Se revisan los primeros bytes: el Content-Type del navegador no es fiable.
        using var input = file.OpenReadStream();
        var header = new byte[12];
        var bytesRead = input.Read(header, 0, header.Length);
        if (!LooksLikeSupportedImage(header, bytesRead, extension))
        {
            return ImageUploadResult.Failure("El contenido del archivo no parece una imagen válida.");
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var directory = GetDirectory(folder);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);

        try
        {
            using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            output.Write(header, 0, bytesRead);
            input.CopyTo(output);
        }
        catch (IOException exception)
        {
            TryDelete(path);
            logger.LogError(exception, "No se pudo guardar la imagen {Path}", path);
            return ImageUploadResult.Failure("No se pudo guardar la imagen. Inténtalo de nuevo.");
        }

        return ImageUploadResult.Success(new StoredImage(fileName, GetContentType(extension)));
    }

    /// <summary>Elimina una imagen si el nombre no permite salir de su carpeta.</summary>
    public void Delete(ImageFolder folder, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var safeFileName = Path.GetFileName(fileName);
        if (!string.Equals(safeFileName, fileName, StringComparison.Ordinal))
        {
            logger.LogWarning("Se rechazó un nombre de imagen no seguro: {FileName}", fileName);
            return;
        }

        TryDelete(Path.Combine(GetDirectory(folder), safeFileName));
    }

    /// <summary>Construye la URL pública de una imagen guardada de forma segura.</summary>
    public string? GetUrl(ImageFolder folder, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var safeFileName = Path.GetFileName(fileName);
        if (!string.Equals(safeFileName, fileName, StringComparison.Ordinal))
        {
            return null;
        }

        return $"/uploads/{GetFolderName(folder)}/{Uri.EscapeDataString(safeFileName)}";
    }

    private string GetDirectory(ImageFolder folder) => Path.Combine(webRootPath, "uploads", GetFolderName(folder));

    private static string GetFolderName(ImageFolder folder) => folder switch
    {
        ImageFolder.Avatars => "avatars",
        ImageFolder.BookCovers => "book-covers",
        ImageFolder.AuthorPhotos => "author-photos",
        _ => throw new ArgumentOutOfRangeException(nameof(folder))
    };

    /// <summary>Comprueba firmas binarias básicas y no solo el Content-Type del navegador.</summary>
    private static bool LooksLikeSupportedImage(byte[] header, int length, string extension)
    {
        if (extension is ".jpg" or ".jpeg")
        {
            return length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        }

        if (extension == ".png")
        {
            return length >= PngSignature.Length && header[..PngSignature.Length].SequenceEqual(PngSignature);
        }

        if (extension == ".gif")
        {
            return length >= 6 && (Encoding.ASCII.GetString(header, 0, 6) is "GIF87a" or "GIF89a");
        }

        return extension == ".webp" && length >= 12 &&
            Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
            Encoding.ASCII.GetString(header, 8, 4) == "WEBP";
    }

    private static string GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };

    private void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "No se pudo eliminar la imagen {Path}", path);
        }
    }
}
