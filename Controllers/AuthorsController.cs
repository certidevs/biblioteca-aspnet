using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Authors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

public sealed class AuthorsController : Controller
{
    private readonly IAuthorService authors;

    public AuthorsController(IAuthorService authors)
    {
        this.authors = authors;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        ViewData["Search"] = search;
        return View(await authors.SearchAsync(search, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var author = await authors.GetDetailsAsync(id, cancellationToken);
        return author is null ? NotFound() : View(author);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Create() => View(new AuthorFormViewModel());

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
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
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var author = await authors.GetDetailsAsync(id, cancellationToken);
        return author is null
            ? NotFound()
            : View(ToViewModel(author));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
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
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var author = await authors.GetDetailsAsync(id, cancellationToken);
        return author is null ? NotFound() : View(author);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        if (!await authors.DeleteAsync(id, cancellationToken))
        {
            return NotFound();
        }

        TempData["Message"] = "Autor eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

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
        Nationality = author.Nationality
    };
}
