using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.ViewModels.Authors;

/// <summary>Campos editables de un autor para las vistas Create y Edit.</summary>
public sealed class AuthorFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    [DataType(DataType.MultilineText)]
    public string? Bio { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateTime? BirthDate { get; set; }

    [StringLength(80)]
    public string? Nationality { get; set; }

    /// <summary>Fotografía opcional recibida desde el formulario multipart.</summary>
    [Display(Name = "Fotografía del autor")]
    public IFormFile? Photo { get; set; }

    /// <summary>Nombre de la fotografía que ya está guardada, solo para mostrarla en Edit.</summary>
    public string? CurrentPhotoFileName { get; set; }

    [Display(Name = "Eliminar fotografía actual")]
    public bool RemovePhoto { get; set; }
}
