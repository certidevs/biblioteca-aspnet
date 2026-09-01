using BibliotecaAspNet.Models;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Users;

public sealed class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    public string? CurrentAvatarFileName { get; set; }

    [Required(ErrorMessage = "El nombre visible es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre visible")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Introduce un email válido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Cuenta activa")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Display(Name = "Rol")]
    public string Role { get; set; } = RoleNames.User;

    [Display(Name = "Nueva contraseña (opcional)")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Display(Name = "Repetir nueva contraseña")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    public string? NewPasswordConfirmation { get; set; }
}
