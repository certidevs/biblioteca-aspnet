using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

/// <summary>Contrato común para guardar y servir avatares y portadas locales.</summary>
public interface IImageStorage
{
    /// <summary>Valida y guarda el archivo con un nombre generado por la aplicación.</summary>
    Task<ImageUploadResult> SaveAsync(
        IFormFile file,
        ImageFolder folder,
        CancellationToken cancellationToken = default);

    /// <summary>Elimina una imagen si el nombre es seguro.</summary>
    void Delete(ImageFolder folder, string? fileName);

    /// <summary>Construye la URL pública de una imagen validando su nombre.</summary>
    string? GetUrl(ImageFolder folder, string? fileName);
}
