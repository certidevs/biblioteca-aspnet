namespace BibliotecaAspNet.Services;

/// <summary>
/// Carpetas de imágenes que la aplicación admite. Mantenerlas como una lista cerrada
/// evita que una entrada del formulario pueda decidir arbitrariamente dónde escribir.
/// </summary>
public enum ImageFolder
{
    Avatars,
    BookCovers
}
