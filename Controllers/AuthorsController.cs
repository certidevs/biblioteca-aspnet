using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Authors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>Acciones MVC del CRUD de autores.</summary>
public sealed class AuthorsController : Controller
{
    private readonly IAuthorService authors;

    /// <summary>Recibe el servicio de autores mediante inyección de dependencias.</summary>
    public AuthorsController(IAuthorService authors)
    {
        this.authors = authors;
    }

    [HttpGet]
    /// <summary>GET: muestra autores y aplica el texto de búsqueda.</summary>
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        ViewData["Search"] = search;
        return View(await authors.SearchAsync(search, cancellationToken));
    }

    [HttpGet]
    /// <summary>GET: muestra un autor con sus libros.</summary>
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var author = await authors.GetDetailsAsync(id, cancellationToken);
        return author is null ? NotFound() : View(author);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    /// <summary>GET protegido: muestra el formulario de alta.</summary>
    public IActionResult Create() => View(new AuthorFormViewModel());

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    /// <summary>POST protegido: valida y persiste el nuevo autor.</summary>
    public async Task<IActionResult> Create(
        AuthorFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await authors.CreateAsync(ToEntity(model), cancellationToken);
        TempData["Message"] = "Autor creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    /// <summary>GET protegido: carga un autor en el formulario de edición.</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var author = await authors.GetDetailsAsync(id, cancellationToken);
        return author is null
            ? NotFound()
            : View(ToViewModel(author));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    /// <summary>POST protegido: actualiza el autor después de validar el formulario.</summary>
    public async Task<IActionResult> Edit(
        int id,
        AuthorFormViewModel model,
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

        if (!await authors.UpdateAsync(id, ToEntity(model), cancellationToken))
        {
            return NotFound();
        }

        TempData["Message"] = "Autor actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    /// <summary>GET protegido: muestra la confirmación de borrado.</summary>
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var author = await authors.GetDetailsAsync(id, cancellationToken);
        return author is null ? NotFound() : View(author);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    /// <summary>POST protegido: confirma el borrado del autor.</summary>
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        if (!await authors.DeleteAsync(id, cancellationToken))
        {
            return NotFound();
        }

        TempData["Message"] = "Autor eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Mapea el DTO de formulario a la entidad persistente.</summary>
    private static Author ToEntity(AuthorFormViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Bio = model.Bio,
        BirthDate = model.BirthDate,
        Nationality = model.Nationality
    };

    /// <summary>Mapea la entidad a un DTO seguro para la vista de edición.</summary>
    private static AuthorFormViewModel ToViewModel(Author author) => new()
    {
        Id = author.Id,
        Name = author.Name,
        Bio = author.Bio,
        BirthDate = author.BirthDate,
        Nationality = author.Nationality
    };
}
