using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Categories;

/// <summary>Datos que necesita la página de listado de categorías.</summary>
public sealed class CategoryIndexViewModel
{
    public string? Search { get; init; }
    public List<Category> Categories { get; init; } = [];
}
