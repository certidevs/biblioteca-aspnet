using BibliotecaAspNet.Models;
using BibliotecaAspNet.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>Registro, login y logout mediante ASP.NET Core Identity.</summary>
public sealed class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    /// <summary>Recibe los gestores de usuarios y sesiones de Identity.</summary>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpGet]
    [AllowAnonymous]
    /// <summary>Muestra el formulario de inicio de sesión.</summary>
    public IActionResult Login(string? returnUrl, bool loggedOut = false) => View(new LoginViewModel
    {
        ReturnUrl = returnUrl,
        ShowLoggedOutMessage = loggedOut
    });

    [HttpPost]
    [AllowAnonymous]
    /// <summary>Identity necesita esta operación asíncrona para validar contraseña y crear la cookie.</summary>
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var identifier = model.Username.Trim();
        var user = await userManager.FindByNameAsync(identifier)
            ?? await userManager.FindByEmailAsync(identifier);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "El usuario o la contraseña no son válidos.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "La cuenta se ha bloqueado temporalmente por demasiados intentos fallidos.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "El usuario o la contraseña no son válidos.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    /// <summary>Muestra el formulario de registro.</summary>
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    /// <summary>Crea una cuenta User y la inicia automáticamente.</summary>
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Username.Trim(),
            Email = model.Email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(model.DisplayName)
                ? model.Username.Trim()
                : model.DisplayName.Trim(),
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        var roleResult = await userManager.AddToRoleAsync(user, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddIdentityErrors(roleResult);
            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        TempData["Message"] = "Tu cuenta se ha creado correctamente.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    /// <summary>Cierra la sesión y limpia el carrito asociado al navegador.</summary>
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        // Evita que el carrito de una cuenta se reutilice accidentalmente al entrar con otra.
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login), new { loggedOut = true });
    }

    [HttpGet]
    [AllowAnonymous]
    /// <summary>Muestra la página cuando Identity deniega una autorización.</summary>
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>Evita redirecciones externas al terminar el login.</summary>
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl!)
            : RedirectToAction("Index", "Home")!;
    }

    /// <summary>Convierte errores de Identity en errores que puede pintar Razor.</summary>
    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
