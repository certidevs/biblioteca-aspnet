using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Account;

/// <summary>Datos que envía la página de login; no es la entidad Identity.</summary>
public sealed class LoginViewModel
{
    /// <summary>Ruta local a la que se vuelve tras iniciar sesión.</summary>
    public string? ReturnUrl { get; set; }

    /// <summary>Permite mostrar el aviso de logout sin usar ViewData.</summary>
    public bool ShowLoggedOutMessage { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [Display(Name = "Usuario o email")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }
}
