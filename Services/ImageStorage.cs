using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Almacenamiento local sencillo para el aula.
///
/// La base de datos conserva únicamente un nombre generado por la aplicación.
/// Se valida extensión, tamaño y firma binaria para no confiar solamente en el
/// Content-Type que envía el navegador.
/// </summary>
public sealed class ImageStorage : IImageStorage
{
    private static readonly byte[] PngSignature =
        [137, 80, 78, 71, 13, 10, 26, 10];

    private readonly string webRootPath;
    private readonly ImageStorageOptions options;
    private readonly ILogger<ImageStorage> logger;

    public ImageStorage(
        IWebHostEnvironment environment,
        IOptions<ImageStorageOptions> options,
        ILogger<ImageStorage> logger)
    {
        webRootPath = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<ImageUploadResult> SaveAsync(
        IFormFile file,
        ImageFolder folder,
        CancellationToken cancellationToken = default)
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

        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var bytesRead = await ReadHeaderAsync(input, header, cancellationToken);
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
            await using var output = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                useAsync: true);

            await output.WriteAsync(header.AsMemory(0, bytesRead), cancellationToken);
            await input.CopyToAsync(output, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TryDelete(path);
            throw;
        }
        catch (IOException exception)
        {
            TryDelete(path);
            logger.LogError(exception, "No se pudo guardar la imagen {Path}", path);
            return ImageUploadResult.Failure("No se pudo guardar la imagen. Inténtalo de nuevo.");
        }

        return ImageUploadResult.Success(new StoredImage(fileName, GetContentType(extension)));
    }

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

        var folderName = folder switch
        {
            ImageFolder.Avatars => "avatars",
            ImageFolder.BookCovers => "book-covers",
            _ => throw new ArgumentOutOfRangeException(nameof(folder))
        };

        return $"/uploads/{folderName}/{Uri.EscapeDataString(safeFileName)}";
    }

    private string GetDirectory(ImageFolder folder)
    {
        var folderName = folder switch
        {
            ImageFolder.Avatars => "avatars",
            ImageFolder.BookCovers => "book-covers",
            _ => throw new ArgumentOutOfRangeException(nameof(folder))
        };

        return Path.Combine(webRootPath, "uploads", folderName);
    }

    private static async Task<int> ReadHeaderAsync(
        Stream input,
        byte[] header,
        CancellationToken cancellationToken)
    {
        var total = 0;
        while (total < header.Length)
        {
            var read = await input.ReadAsync(header.AsMemory(total), cancellationToken);
            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total;
    }

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
            return length >= 6 &&
                (Encoding.ASCII.GetString(header, 0, 6) is "GIF87a" or "GIF89a");
        }

        return extension == ".webp" &&
            length >= 12 &&
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
