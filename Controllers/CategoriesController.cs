using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

public sealed class CategoriesController : Controller
{
    private readonly ICategoryService categories;

    public CategoriesController(ICategoryService categories)
    {
        this.categories = categories;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        ViewData["Search"] = search;
        return View(await categories.SearchAsync(search, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var category = await categories.GetDetailsAsync(id, cancellationToken);
        return category is null ? NotFound() : View(category);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Create() => View(new CategoryFormViewModel());

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(
        CategoryFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await categories.CreateAsync(ToEntity(model), cancellationToken);
            TempData["Message"] = "Categoría creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var category = await categories.GetDetailsAsync(id, cancellationToken);
        return category is null ? NotFound() : View(ToViewModel(category));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<IActionResult> Edit(
        int id,
        CategoryFormViewModel model,
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

        try
        {
            if (!await categories.UpdateAsync(id, ToEntity(model), cancellationToken))
            {
                return NotFound();
            }

            TempData["Message"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var category = await categories.GetDetailsAsync(id, cancellationToken);
        return category is null ? NotFound() : View(category);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        if (!await categories.DeleteAsync(id, cancellationToken))
        {
            return NotFound();
        }

        TempData["Message"] = "Categoría eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private static Category ToEntity(CategoryFormViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        Color = model.Color
    };

    private static CategoryFormViewModel ToViewModel(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        Color = category.Color
    };
}
