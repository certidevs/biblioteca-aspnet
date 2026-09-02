using System.Security.Claims;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

[Authorize(Roles = RoleNames.Admin)]
/// <summary>Panel administrativo de cuentas, roles, estado y avatares.</summary>
public sealed class UsersController : Controller
{
    private readonly UserService users;

    public UsersController(UserService users)
    {
        this.users = users;
    }

    [HttpGet]
    /// <summary>Identity ofrece roles de forma asíncrona; el resto del CRUD es síncrono.</summary>
    public async Task<IActionResult> Index(string? search)
    {
        ViewData["Search"] = search;
        return View(await users.SearchAsync(search));
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await users.CreateAsync(model);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        TempData["Message"] = "Usuario creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(string id)
    {
        var model = users.GetProfile(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var model = await users.GetEditModelAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
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

        var result = await users.UpdateAsync(model, currentAdminId);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        TempData["Message"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult Delete(string id)
    {
        var model = users.GetProfile(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentAdminId is null)
        {
            return Challenge();
        }

        var result = await users.DeleteAsync(id, currentAdminId);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Message"] = "Usuario eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
