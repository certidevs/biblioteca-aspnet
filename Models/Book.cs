using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

public sealed class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Range(0, 999999.99, ErrorMessage = "El precio debe ser cero o mayor.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    public bool Available { get; set; } = true;

    [DataType(DataType.Date)]
    public DateTime? PublishDate { get; set; }

    [StringLength(20)]
    public string? Isbn { get; set; }

    [Range(1, 10000, ErrorMessage = "Las páginas deben estar entre 1 y 10.000.")]
    public int? Pages { get; set; }

    [StringLength(60)]
    public string? Language { get; set; }

    [StringLength(5000)]
    public string? Synopsis { get; set; }

    /// <summary>
    /// Nombre generado por la aplicación para la portada almacenada en wwwroot/uploads.
    /// Nunca se guarda la ruta ni el nombre original del archivo subido por el usuario.
    /// </summary>
    [StringLength(260)]
    public string? CoverImageFileName { get; set; }

    [Required(ErrorMessage = "Selecciona un autor.")]
    public int AuthorId { get; set; }

    public Author Author { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ApplicationUser> FavoriteUsers { get; set; } = new List<ApplicationUser>();
}
