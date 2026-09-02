using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.ViewModels.Profile;
using BibliotecaAspNet.ViewModels.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Casos de uso comunes de usuarios. Este servicio es una de las piezas que se
/// puede reutilizar al arrancar cualquiera de los proyectos de grupos.
/// </summary>
public sealed class UserService
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly ImageStorage images;

    /// <summary>Recibe Identity, EF Core y almacenamiento de imágenes por DI.</summary>
    public UserService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ImageStorage images)
    {
        this.context = context;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.images = images;
    }

    /// <summary>Busca usuarios y añade a cada fila su rol principal.</summary>
    public async Task<List<UserListItemViewModel>> SearchAsync(string? search)
    {
        var query = context.Users.AsNoTracking().AsQueryable();
        // La búsqueda se traduce a SQL y no carga todos los usuarios antes de filtrar.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(user =>
                (user.UserName != null && EF.Functions.Like(user.UserName, $"%{value}%")) ||
                (user.Email != null && EF.Functions.Like(user.Email, $"%{value}%")) ||
                (user.DisplayName != null && EF.Functions.Like(user.DisplayName, $"%{value}%")));
        }

        var users = query.OrderBy(user => user.UserName).ToList();
        var result = new List<UserListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserListItemViewModel
            {
                User = user,
                Role = roles.FirstOrDefault() ?? RoleNames.User
            });
        }

        return result;
    }

    /// <summary>Carga el perfil y todas las relaciones que se muestran en su resumen.</summary>
    public ProfileViewModel? GetProfile(string userId)
    {
        var user = context.Users
            .AsNoTracking()
            .Include(item => item.FavoriteBooks)
            .Include(item => item.Reviews)
            .ThenInclude(review => review.Book)
            .Include(item => item.Orders)
            .ThenInclude(order => order.Items)
            .ThenInclude(orderItem => orderItem.Book)
            .AsSplitQuery()
            .SingleOrDefault(item => item.Id == userId);

        if (user is null)
        {
            return null;
        }

        return new ProfileViewModel
        {
            User = user,
            FavoriteBooks = user.FavoriteBooks.OrderBy(book => book.Title).ToList(),
            Reviews = user.Reviews.OrderByDescending(review => review.CreatedAt).ToList(),
            Orders = user.Orders.OrderByDescending(order => order.CreatedAt).ToList(),
            TotalSpent = user.Orders
                .Where(order => order.Status == OrderStatus.Paid)
                .Sum(order => order.Total)
        };
    }

    /// <summary>Proyecta un usuario a un modelo seguro para editar datos y avatar.</summary>
    public ProfileEditViewModel? GetProfileEditModel(string userId)
    {
        var user = context.Users
            .AsNoTracking()
            .SingleOrDefault(item => item.Id == userId);
        if (user is null)
        {
            return null;
        }

        return new ProfileEditViewModel
        {
            DisplayName = user.DisplayName ?? user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            CurrentAvatarFileName = user.AvatarFileName
        };
    }

    /// <summary>Actualiza datos personales y sustituye o elimina el avatar si procede.</summary>
    /// <remarks>Identity expone estas operaciones de cuenta de forma asíncrona.</remarks>
    public async Task<IdentityResult> UpdateProfileAsync(string userId, ProfileEditViewModel model)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Failure("El usuario no existe.");
        }

        var displayName = model.DisplayName.Trim();
        var email = model.Email.Trim();
        if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(email))
        {
            return Failure("El nombre visible y el email son obligatorios.");
        }

        var oldAvatarFileName = user.AvatarFileName;
        var avatarFileName = model.RemoveAvatar ? null : oldAvatarFileName;
        string? newAvatarFileName = null;
        if (model.Avatar is not null)
        {
            var upload = images.Save(model.Avatar, ImageFolder.Avatars);
            if (!upload.Succeeded)
            {
                return Failure(upload.Error!);
            }

            newAvatarFileName = upload.Image!.FileName;
            avatarFileName = newAvatarFileName;
        }

        user.DisplayName = displayName;
        user.Email = email;
        user.AvatarFileName = avatarFileName;

        // Primero se actualiza Identity; solo después se elimina el avatar antiguo.
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            images.Delete(ImageFolder.Avatars, newAvatarFileName);
            return updateResult;
        }

        if (!string.Equals(oldAvatarFileName, user.AvatarFileName, StringComparison.Ordinal))
        {
            images.Delete(ImageFolder.Avatars, oldAvatarFileName);
        }

        return updateResult;
    }

    /// <summary>Cambia la contraseña usando la verificación interna de Identity.</summary>
    public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Failure("El usuario no existe.");
        }

        return await userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword);
    }

    /// <summary>Proyecta un usuario al formulario usado por un administrador.</summary>
    public async Task<EditUserViewModel?> GetEditModelAsync(string id)
    {
        var user = context.Users
            .AsNoTracking()
            .SingleOrDefault(item => item.Id == id);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new EditUserViewModel
        {
            Id = user.Id,
            CurrentAvatarFileName = user.AvatarFileName,
            DisplayName = user.DisplayName ?? user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            Role = roles.FirstOrDefault() ?? RoleNames.User
        };
    }

    /// <summary>Actualiza una cuenta y protege las reglas del último administrador.</summary>
    public async Task<IdentityResult> UpdateAsync(EditUserViewModel model, string currentAdminId)
    {
        if (!await roleManager.RoleExistsAsync(model.Role))
        {
            return Failure("El rol seleccionado no existe.");
        }

        if (model.Id == currentAdminId &&
            (model.Role != RoleNames.Admin || !model.IsActive))
        {
            return Failure("No puedes quitarte el rol de administrador ni desactivar tu propia cuenta.");
        }

        var user = await userManager.FindByIdAsync(model.Id);
        if (user is null)
        {
            return Failure("El usuario no existe.");
        }

        user.DisplayName = model.DisplayName.Trim();
        user.Email = model.Email.Trim();
        user.IsActive = model.IsActive;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return updateResult;
        }

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await userManager.ResetPasswordAsync(
                user,
                resetToken,
                model.NewPassword);
            if (!passwordResult.Succeeded)
            {
                return passwordResult;
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        // Se mantiene un único rol para que el ejemplo sea fácil de razonar.
        if (currentRoles.Count != 1 || currentRoles[0] != model.Role)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return removeResult;
            }

            return await userManager.AddToRoleAsync(user, model.Role);
        }

        return IdentityResult.Success;
    }

    /// <summary>Crea una cuenta, asigna rol y guarda su avatar opcional.</summary>
    public async Task<IdentityResult> CreateAsync(CreateUserViewModel model)
    {
        if (!await roleManager.RoleExistsAsync(model.Role))
        {
            return Failure("El rol seleccionado no existe.");
        }

        var user = new ApplicationUser
        {
            UserName = model.Username.Trim(),
            Email = model.Email.Trim(),
            DisplayName = model.DisplayName.Trim(),
            IsActive = model.IsActive,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            return createResult;
        }

        // Si falla la asignación del rol, se deshace el usuario recién creado.
        var roleResult = await userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return roleResult;
        }

        if (model.Avatar is not null)
        {
            var upload = images.Save(model.Avatar, ImageFolder.Avatars);
            if (!upload.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Failure(upload.Error!);
            }

            user.AvatarFileName = upload.Image!.FileName;
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                images.Delete(ImageFolder.Avatars, user.AvatarFileName);
                await userManager.DeleteAsync(user);
                return updateResult;
            }
        }

        return IdentityResult.Success;
    }

    /// <summary>Elimina una cuenta sin permitir borrar la propia ni el último admin.</summary>
    public async Task<IdentityResult> DeleteAsync(string id, string currentAdminId)
    {
        if (id == currentAdminId)
        {
            return Failure("No puedes eliminar tu propia cuenta desde la administración.");
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return Failure("El usuario no existe.");
        }

        // Evita dejar el panel de administración sin ninguna cuenta administradora.
        if (await userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            var administrators = await userManager.GetUsersInRoleAsync(RoleNames.Admin);
            if (administrators.Count <= 1)
            {
                return Failure("No se puede eliminar al último administrador.");
            }
        }

        var avatarFileName = user.AvatarFileName;
        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            images.Delete(ImageFolder.Avatars, avatarFileName);
        }

        return result;
    }

    /// <summary>Crea un resultado de Identity con un mensaje de validación de negocio.</summary>
    private static IdentityResult Failure(string message) =>
        IdentityResult.Failed(new IdentityError { Description = message });
}
