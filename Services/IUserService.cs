using BibliotecaAspNet.ViewModels.Profile;
using BibliotecaAspNet.ViewModels.Users;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Casos de uso transversales de usuarios. Esta interfaz y su implementación
/// forman la base que se puede copiar a ecommerce, cine o restaurantes.
/// </summary>
public interface IUserService
{
    /// <summary>Busca usuarios y proyecta sus datos a filas de administración.</summary>
    Task<List<UserListItemViewModel>> SearchAsync(string? search, CancellationToken cancellationToken = default);

    /// <summary>Construye el perfil con favoritos, reseñas y pedidos.</summary>
    Task<ProfileViewModel?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Prepara el formulario de edición del perfil y su avatar actual.</summary>
    Task<ProfileEditViewModel?> GetProfileEditModelAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Actualiza datos personales y avatar mediante UserManager.</summary>
    Task<IdentityResult> UpdateProfileAsync(
        string userId,
        ProfileEditViewModel model,
        CancellationToken cancellationToken = default);

    /// <summary>Cambia la contraseña comprobando primero la contraseña actual.</summary>
    Task<IdentityResult> ChangePasswordAsync(
        string userId,
        ChangePasswordViewModel model,
        CancellationToken cancellationToken = default);

    /// <summary>Prepara el formulario administrativo de un usuario.</summary>
    Task<EditUserViewModel?> GetEditModelAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Actualiza perfil, rol, estado y contraseña opcional de un usuario.</summary>
    Task<IdentityResult> UpdateAsync(
        EditUserViewModel model,
        string currentAdminId,
        CancellationToken cancellationToken = default);

    /// <summary>Crea un usuario administrativo con su rol y avatar opcional.</summary>
    Task<IdentityResult> CreateAsync(
        CreateUserViewModel model,
        CancellationToken cancellationToken = default);

    /// <summary>Elimina un usuario salvo que la regla de seguridad lo impida.</summary>
    Task<IdentityResult> DeleteAsync(
        string id,
        string currentAdminId,
        CancellationToken cancellationToken = default);
}
