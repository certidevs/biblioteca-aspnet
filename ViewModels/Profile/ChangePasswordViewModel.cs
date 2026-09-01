using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Profile;

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la nueva contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Repetir nueva contraseña")]
    public string NewPasswordConfirmation { get; set; } = string.Empty;
}
