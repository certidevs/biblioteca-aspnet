using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

public sealed class Author
{
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

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
