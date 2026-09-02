using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.Utilities;
using BibliotecaAspNet.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

[Authorize]
/// <summary>Consulta y edición del perfil del usuario autenticado.</summary>
public sealed class ProfileController : Controller
{
    private readonly UserService userService;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public ProfileController(
        UserService userService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        this.userService = userService;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = userService.GetProfile(User.GetRequiredUserId());
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Edit()
    {
        var model = userService.GetProfileEditModel(User.GetRequiredUserId());
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    /// <summary>Identity requiere operaciones asíncronas para cambiar la cuenta y refrescar la cookie.</summary>
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        var userId = User.GetRequiredUserId();

        if (!ModelState.IsValid)
        {
            RestoreCurrentAvatar(model, userId);
            return View(model);
        }

        var result = await userService.UpdateProfileAsync(userId, model);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            RestoreCurrentAvatar(model, userId);
            return View(model);
        }

        await RefreshSignInAsync(userId);
        TempData["Message"] = "Perfil actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.GetRequiredUserId();
        var result = await userService.ChangePasswordAsync(userId, model);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        await RefreshSignInAsync(userId);
        TempData["Message"] = "Contraseña cambiada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private void RestoreCurrentAvatar(ProfileEditViewModel model, string userId)
    {
        model.CurrentAvatarFileName = userService.GetProfileEditModel(userId)?.CurrentAvatarFileName;
    }

    /// <summary>Actualiza la cookie cuando cambia información que muestra el layout.</summary>
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
