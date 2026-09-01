using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaAspNet.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public sealed class UsersController : Controller
{
    private readonly IUserService users;

    public UsersController(IUserService users)
    {
        this.users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        ViewData["Search"] = search;
        return View(await users.SearchAsync(search, cancellationToken));
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost]
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
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var model = await users.GetProfileAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var model = await users.GetEditModelAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
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
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var model = await users.GetProfileAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete")]
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

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
