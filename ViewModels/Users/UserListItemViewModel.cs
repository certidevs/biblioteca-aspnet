using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Users;

/// <summary>Fila de usuario con su rol principal para el listado administrativo.</summary>
public sealed class UserListItemViewModel
{
    /// <summary>Usuario de Identity que se muestra.</summary>
    public ApplicationUser User { get; init; } = null!;

    /// <summary>Rol que la vista imprime como etiqueta.</summary>
    public string Role { get; init; } = RoleNames.User;
}
