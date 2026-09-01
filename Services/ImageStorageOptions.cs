namespace BibliotecaAspNet.Services;

/// <summary>Opciones de tamaño y extensiones permitidas para las imágenes.</summary>
public sealed class ImageStorageOptions
{
    /// <summary>Tamaño máximo de un archivo subido, en bytes.</summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>Extensiones que se aceptan después de validar también la firma binaria.</summary>
    public string[] AllowedExtensions { get; set; } =
    [".jpg", ".jpeg", ".png", ".gif", ".webp"];
}
