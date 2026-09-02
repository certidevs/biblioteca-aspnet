using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Authors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>CRUD MVC de autores. El controlador traduce HTTP a una vista o redirección.</summary>
public sealed class AuthorsController : Controller
{
    private readonly AuthorService authorService;

    public AuthorsController(AuthorService authorService)
    {
        this.authorService = authorService;
    }

    [HttpGet]
    /// <summary>GET: muestra autores y aplica el texto de búsqueda.</summary>
    public IActionResult Index(string? search)
    {
        return View(new AuthorIndexViewModel
        {
            Search = search,
            Authors = authorService.Search(search)
        });
    }

    [HttpGet]
    /// <summary>GET: muestra un autor con sus libros.</summary>
    public IActionResult Details(int id)
    {
        var author = authorService.GetDetails(id);
        if (author is null)
        {
            return NotFound();
        }

        return View(author);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Create() => View(new AuthorFormViewModel());

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    /// <summary>POST: valida el formulario antes de insertar el autor.</summary>
    public IActionResult Create(AuthorFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            authorService.Create(ToEntity(model), model.Photo);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }

        TempData["Message"] = "Autor creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var author = authorService.GetDetails(id);
        if (author is null)
        {
            return NotFound();
        }

        return View(ToViewModel(author));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    /// <summary>POST: actualiza solo campos editables y conserva la foto al fallar la validación.</summary>
    public IActionResult Edit(int id, AuthorFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            RestoreCurrentPhoto(model, id);
            return View(model);
        }

        try
        {
            if (!authorService.Update(id, ToEntity(model), model.Photo, model.RemovePhoto))
            {
                return NotFound();
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            RestoreCurrentPhoto(model, id);
            return View(model);
        }

        TempData["Message"] = "Autor actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var author = authorService.GetDetails(id);
        if (author is null)
        {
            return NotFound();
        }

        return View(author);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!authorService.Delete(id))
        {
            return NotFound();
        }

        TempData["Message"] = "Autor eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Convierte el ViewModel del formulario en la entidad que guarda EF Core.</summary>
    private static Author ToEntity(AuthorFormViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Bio = model.Bio,
        BirthDate = model.BirthDate,
        Nationality = model.Nationality
    };

    private static AuthorFormViewModel ToViewModel(Author author) => new()
    {
        Id = author.Id,
        Name = author.Name,
        Bio = author.Bio,
        BirthDate = author.BirthDate,
        Nationality = author.Nationality,
        CurrentPhotoFileName = author.PhotoFileName
    };

    private void RestoreCurrentPhoto(AuthorFormViewModel model, int id)
    {
        model.CurrentPhotoFileName = authorService.GetDetails(id)?.PhotoFileName;
    }
}
