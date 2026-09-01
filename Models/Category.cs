using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Etiqueta del catálogo. Se relaciona N:M con <see cref="Book"/> para que un
/// libro pueda tener varias categorías y una categoría agrupe muchos libros.
/// </summary>
public sealed class Category
{
    /// <summary>Identificador generado por la base de datos.</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^#[0-9a-fA-F]{6}$", ErrorMessage = "Usa un color hexadecimal como #2563eb.")]
    public string? Color { get; set; }

    /// <summary>Libros asociados a esta categoría mediante BookCategories.</summary>
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
