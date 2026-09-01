using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Account;

/// <summary>Datos que envía la página de login; no es la entidad Identity.</summary>
public sealed class LoginViewModel
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [Display(Name = "Usuario o email")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }
}
