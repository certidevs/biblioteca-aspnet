using BibliotecaAspNet.ViewModels.Profile;
using BibliotecaAspNet.ViewModels.Users;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaAspNet.Services;

public interface IUserService
{
    Task<List<UserListItemViewModel>> SearchAsync(string? search, CancellationToken cancellationToken = default);
    Task<ProfileViewModel?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<ProfileEditViewModel?> GetProfileEditModelAsync(string userId, CancellationToken cancellationToken = default);
    Task<IdentityResult> UpdateProfileAsync(
        string userId,
        ProfileEditViewModel model,
        CancellationToken cancellationToken = default);
    Task<IdentityResult> ChangePasswordAsync(
        string userId,
        ChangePasswordViewModel model,
        CancellationToken cancellationToken = default);
    Task<EditUserViewModel?> GetEditModelAsync(string id, CancellationToken cancellationToken = default);
    Task<IdentityResult> UpdateAsync(
        EditUserViewModel model,
        string currentAdminId,
        CancellationToken cancellationToken = default);
    Task<IdentityResult> CreateAsync(
        CreateUserViewModel model,
        CancellationToken cancellationToken = default);
    Task<IdentityResult> DeleteAsync(
        string id,
        string currentAdminId,
        CancellationToken cancellationToken = default);
}
