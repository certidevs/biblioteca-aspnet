using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

public sealed class Review
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(2000, ErrorMessage = "El comentario no puede superar los 2.000 caracteres.")]
    public string Comment { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5.")]
    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
}
