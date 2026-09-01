using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Profile;

public sealed class ProfileEditViewModel
{
    [Required(ErrorMessage = "El nombre visible es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre visible no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre visible")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Introduce un email válido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Avatar")]
    public IFormFile? Avatar { get; set; }

    [Display(Name = "Eliminar avatar actual")]
    public bool RemoveAvatar { get; set; }

    public string? CurrentAvatarFileName { get; set; }
}
