using BibliotecaAspNet.Models;

namespace BibliotecaAspNet.ViewModels.Users;

public sealed class UserListItemViewModel
{
    public ApplicationUser User { get; init; } = null!;
    public string Role { get; init; } = RoleNames.User;
}
