using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaAspNet.Controllers;

[Authorize(Roles = RoleNames.Admin)]
/// <summary>Panel administrativo de cuentas, roles, estado y avatares.</summary>
public sealed class UsersController : Controller
{
    private readonly IUserService users;

    /// <summary>Recibe los casos de uso comunes de usuarios.</summary>
    public UsersController(IUserService users)
    {
        this.users = users;
    }

    [HttpGet]
    /// <summary>GET: lista y busca cuentas de usuario.</summary>
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        ViewData["Search"] = search;
        return View(await users.SearchAsync(search, cancellationToken));
    }

    [HttpGet]
    /// <summary>GET: muestra el formulario de alta administrativa.</summary>
    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost]
    /// <summary>POST: crea una cuenta con rol, estado y avatar opcional.</summary>
    public async Task<IActionResult> Create(
        CreateUserViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await users.CreateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        TempData["Message"] = "Usuario creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    /// <summary>GET: muestra el perfil y actividad de una cuenta.</summary>
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var model = await users.GetProfileAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    /// <summary>GET: carga una cuenta en el formulario de administración.</summary>
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var model = await users.GetEditModelAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    /// <summary>POST: actualiza datos, rol, estado y contraseña opcional.</summary>
    public async Task<IActionResult> Edit(
        string id,
        EditUserViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentAdminId is null)
        {
            return Challenge();
        }

        var result = await users.UpdateAsync(model, currentAdminId, cancellationToken);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        TempData["Message"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    /// <summary>GET: muestra la confirmación de borrado de una cuenta.</summary>
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var model = await users.GetProfileAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete")]
    /// <summary>POST: elimina la cuenta respetando las reglas del último administrador.</summary>
    public async Task<IActionResult> DeleteConfirmed(
        string id,
        CancellationToken cancellationToken)
    {
        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentAdminId is null)
        {
            return Challenge();
        }

        var result = await users.DeleteAsync(id, currentAdminId, cancellationToken);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Message"] = "Usuario eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Convierte errores de Identity en mensajes del formulario.</summary>
    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
