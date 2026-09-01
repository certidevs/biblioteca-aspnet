using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Books;

/// <summary>
/// DTO de formulario para libros. Incluye IDs de asociaciones y listas para los
/// desplegables, pero no expone la entidad completa al model binder.
/// </summary>
public sealed class BookFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Range(0, 999999.99, ErrorMessage = "El precio debe ser cero o mayor.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Display(Name = "Disponible para compra")]
    public bool Available { get; set; } = true;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de publicación")]
    public DateTime? PublishDate { get; set; }

    [StringLength(20)]
    public string? Isbn { get; set; }

    [Range(1, 10000, ErrorMessage = "Las páginas deben estar entre 1 y 10.000.")]
    public int? Pages { get; set; }

    [StringLength(60)]
    public string? Language { get; set; }

    [StringLength(5000)]
    [DataType(DataType.MultilineText)]
    public string? Synopsis { get; set; }

    [Display(Name = "Portada del libro")]
    public IFormFile? CoverImage { get; set; }

    public string? CurrentCoverImageFileName { get; set; }

    [Display(Name = "Eliminar portada actual")]
    public bool RemoveCoverImage { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un autor.")]
    [Display(Name = "Autor")]
    public int AuthorId { get; set; }

    [Display(Name = "Categorías")]
    /// <summary>IDs enviados por el multiselect de la relación N:M con categorías.</summary>
    public int[] SelectedCategoryIds { get; set; } = Array.Empty<int>();

    /// <summary>Opciones que el controlador carga para el selector de autor.</summary>
    public List<Author> Authors { get; set; } = new();

    /// <summary>Opciones que el controlador carga para el multiselect de categorías.</summary>
    public List<Category> Categories { get; set; } = new();

    /// <summary>Convierte una entidad en el modelo que necesita el formulario de edición.</summary>
    public static BookFormViewModel FromBook(Book book)
    {
        return new BookFormViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Price = book.Price,
            Available = book.Available,
            PublishDate = book.PublishDate,
            Isbn = book.Isbn,
            Pages = book.Pages,
            Language = book.Language,
            Synopsis = book.Synopsis,
            CurrentCoverImageFileName = book.CoverImageFileName,
            AuthorId = book.AuthorId,
            SelectedCategoryIds = book.Categories.Select(category => category.Id).ToArray()
        };
    }

    /// <summary>Convierte los campos editables del formulario en una entidad Book.</summary>
    public Book ToBook()
    {
        return new Book
        {
            Id = Id,
            Title = Title,
            Price = Price,
            Available = Available,
            PublishDate = PublishDate,
            Isbn = Isbn,
            Pages = Pages,
            Language = Language,
            Synopsis = Synopsis,
            AuthorId = AuthorId
        };
    }
}
