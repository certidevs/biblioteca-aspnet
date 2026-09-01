using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Autor del catálogo. La relación es 1:N: un autor tiene muchos libros y
/// cada libro apunta a un único autor mediante <see cref="Book.AuthorId"/>.
/// </summary>
public sealed class Author
{
    /// <summary>Identificador generado por la base de datos.</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Bio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [StringLength(80)]
    public string? Nationality { get; set; }

    /// <summary>Nombre generado de la fotografía del autor guardada en wwwroot/uploads/author-photos.</summary>
    [StringLength(260)]
    public string? PhotoFileName { get; set; }

    /// <summary>Libros escritos por este autor; es la navegación inversa de <see cref="Book.Author"/>.</summary>
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
