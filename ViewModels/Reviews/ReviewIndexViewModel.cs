using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Reviews;

/// <summary>Filtro seleccionado y reseñas que muestra la página.</summary>
public sealed class ReviewIndexViewModel
{
    public int? Rating { get; init; }
    public List<Review> Reviews { get; init; } = [];
}
