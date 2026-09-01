namespace BibliotecaAspNet.Services;

/// <summary>Metadatos seguros de una imagen guardada.</summary>
public sealed record StoredImage(string FileName, string ContentType);

/// <summary>Resultado de guardar una imagen, con error legible para el formulario.</summary>
public sealed record ImageUploadResult(StoredImage? Image, string? Error)
{
    /// <summary>Indica que se ha guardado una imagen válida.</summary>
    public bool Succeeded => Image is not null;

    /// <summary>Crea un resultado correcto.</summary>
    public static ImageUploadResult Success(StoredImage image) => new(image, null);

    /// <summary>Crea un resultado fallido sin lanzar una excepción de validación.</summary>
    public static ImageUploadResult Failure(string error) => new(null, error);
}
