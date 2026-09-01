using BibliotecaAspNet.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Users;

public sealed class CreateUserViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 30 caracteres.")]
    [Display(Name = "Nombre de usuario")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Introduce un email válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre visible es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre visible")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string PasswordConfirmation { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Rol")]
    public string Role { get; set; } = RoleNames.User;

    [Display(Name = "Cuenta activa")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Avatar")]
    public IFormFile? Avatar { get; set; }
}
