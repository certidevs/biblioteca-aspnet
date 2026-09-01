using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.Models;

/// <summary>
/// Entidad principal del catálogo. Sirve para practicar CRUD, filtros, imágenes,
/// favoritos, reseñas y líneas de pedido.
/// </summary>
public sealed class Book
{
    /// <summary>Identificador generado por la base de datos.</summary>
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

    /// <summary>FK obligatoria hacia el autor: muchos libros pueden pertenecer al mismo autor.</summary>
    [Required(ErrorMessage = "Selecciona un autor.")]
    public int AuthorId { get; set; }

    /// <summary>Navegación N:1 hacia <see cref="Author"/>.</summary>
    public Author Author { get; set; } = null!;

    /// <summary>Relación N:M con categorías, almacenada en la tabla intermedia BookCategories.</summary>
    public ICollection<Category> Categories { get; set; } = new List<Category>();

    /// <summary>Reseñas publicadas para el libro: relación 1:N.</summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>Líneas de pedidos que originalmente compraron este libro.</summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>Usuarios que han marcado el libro como favorito: relación N:M.</summary>
    public ICollection<ApplicationUser> FavoriteUsers { get; set; } = new List<ApplicationUser>();
}
