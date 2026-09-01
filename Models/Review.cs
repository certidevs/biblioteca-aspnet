using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Opinión de un usuario sobre un libro. Tanto <see cref="UserId"/> como
/// <see cref="BookId"/> son FKs obligatorias: la reseña siempre tiene autor y libro.
/// </summary>
public sealed class Review
{
    /// <summary>Identificador generado por la base de datos.</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(2000, ErrorMessage = "El comentario no puede superar los 2.000 caracteres.")]
    public string Comment { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5.")]
    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>FK del usuario que publicó la reseña; relación N:1 con ApplicationUser.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Usuario que publicó la reseña; navegación inversa de <see cref="ApplicationUser.Reviews"/>.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>FK del libro reseñado; relación N:1 con Book.</summary>
    public int BookId { get; set; }

    /// <summary>Libro al que pertenece la reseña; navegación inversa de <see cref="Book.Reviews"/>.</summary>
    public Book Book { get; set; } = null!;
}
