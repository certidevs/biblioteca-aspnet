using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Reviews;

/// <summary>DTO del formulario de reseña; el UserId se obtiene de Claims en el servidor.</summary>
public sealed class ReviewFormViewModel
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(2000, ErrorMessage = "El comentario no puede superar los 2.000 caracteres.")]
    [DataType(DataType.MultilineText)]
    public string Comment { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5.")]
    public int Rating { get; set; } = 5;
}
