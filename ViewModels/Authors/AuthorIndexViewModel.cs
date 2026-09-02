using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Authors;

/// <summary>Datos que necesita la página de listado de autores.</summary>
public sealed class AuthorIndexViewModel
{
    public string? Search { get; init; }
    public List<Author> Authors { get; init; } = [];
}
