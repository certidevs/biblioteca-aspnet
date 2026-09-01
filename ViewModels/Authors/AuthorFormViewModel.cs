using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Authors;

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
}
