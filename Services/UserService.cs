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
public sealed class UserService : IUserService
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly IImageStorage images;

    public UserService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IImageStorage images)
    {
        this.context = context;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.images = images;
    }

    public async Task<List<UserListItemViewModel>> SearchAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = context.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(user =>
                (user.UserName != null && EF.Functions.Like(user.UserName, $"%{value}%")) ||
                (user.Email != null && EF.Functions.Like(user.Email, $"%{value}%")) ||
                (user.DisplayName != null && EF.Functions.Like(user.DisplayName, $"%{value}%")));
        }

        var users = await query.OrderBy(user => user.UserName).ToListAsync(cancellationToken);
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

    public async Task<ProfileViewModel?> GetProfileAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(item => item.FavoriteBooks)
            .Include(item => item.Reviews)
            .ThenInclude(review => review.Book)
            .Include(item => item.Orders)
            .ThenInclude(order => order.Items)
            .ThenInclude(orderItem => orderItem.Book)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);

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

    public async Task<ProfileEditViewModel?> GetProfileEditModelAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
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

    public async Task<IdentityResult> UpdateProfileAsync(
        string userId,
        ProfileEditViewModel model,
        CancellationToken cancellationToken = default)
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
        string? newAvatarFileName = null;
        if (model.Avatar is not null)
        {
            var upload = await images.SaveAsync(
                model.Avatar,
                ImageFolder.Avatars,
                cancellationToken);
            if (!upload.Succeeded)
            {
                return Failure(upload.Error!);
            }

            newAvatarFileName = upload.Image!.FileName;
        }

        user.DisplayName = displayName;
        user.Email = email;
        user.AvatarFileName = newAvatarFileName
            ?? (model.RemoveAvatar ? null : oldAvatarFileName);

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

    public async Task<IdentityResult> ChangePasswordAsync(
        string userId,
        ChangePasswordViewModel model,
        CancellationToken cancellationToken = default)
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

    public async Task<EditUserViewModel?> GetEditModelAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
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

    public async Task<IdentityResult> UpdateAsync(
        EditUserViewModel model,
        string currentAdminId,
        CancellationToken cancellationToken = default)
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

    public async Task<IdentityResult> CreateAsync(
        CreateUserViewModel model,
        CancellationToken cancellationToken = default)
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

        var roleResult = await userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return roleResult;
        }

        if (model.Avatar is not null)
        {
            var upload = await images.SaveAsync(
                model.Avatar,
                ImageFolder.Avatars,
                cancellationToken);
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

    public async Task<IdentityResult> DeleteAsync(
        string id,
        string currentAdminId,
        CancellationToken cancellationToken = default)
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

    private static IdentityResult Failure(string message) =>
        IdentityResult.Failed(new IdentityError { Description = message });
}
