using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

public sealed class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(20)]
    public string? Color { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
