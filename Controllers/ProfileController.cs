using BibliotecaAspNet.Services;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaAspNet.Controllers;

[Authorize]
public sealed class ProfileController : Controller
{
    private readonly IUserService users;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public ProfileController(
        IUserService users,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        this.users = users;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var model = await users.GetProfileAsync(userId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var model = await users.GetProfileEditModelAsync(userId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        ProfileEditViewModel model,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await RestoreCurrentAvatarAsync(model, userId, cancellationToken);
            return View(model);
        }

        var result = await users.UpdateProfileAsync(userId, model, cancellationToken);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            await RestoreCurrentAvatarAsync(model, userId, cancellationToken);
            return View(model);
        }

        await RefreshSignInAsync(userId);
        TempData["Message"] = "Perfil actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var result = await users.ChangePasswordAsync(userId, model, cancellationToken);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        await RefreshSignInAsync(userId);
        TempData["Message"] = "Contraseña cambiada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task RestoreCurrentAvatarAsync(
        ProfileEditViewModel model,
        string userId,
        CancellationToken cancellationToken)
    {
        var current = await users.GetProfileEditModelAsync(userId, cancellationToken);
        model.CurrentAvatarFileName = current?.CurrentAvatarFileName;
    }

    private async Task RefreshSignInAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await signInManager.RefreshSignInAsync(user);
        }
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
