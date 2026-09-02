namespace BibliotecaAspNet.ViewModels.Users;

/// <summary>Búsqueda y filas del listado administrativo de usuarios.</summary>
public sealed class UserIndexViewModel
{
    public string? Search { get; init; }
    public List<UserListItemViewModel> Users { get; init; } = [];
}
