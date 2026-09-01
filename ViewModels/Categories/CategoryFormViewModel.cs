using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Categories;

public sealed class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^#[0-9a-fA-F]{6}$", ErrorMessage = "Usa un color hexadecimal como #2563eb.")]
    [Display(Name = "Color hexadecimal")]
    public string? Color { get; set; }
}
