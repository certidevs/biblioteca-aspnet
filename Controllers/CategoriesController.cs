using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>CRUD MVC de categorías.</summary>
public sealed class CategoriesController : Controller
{
    private readonly CategoryService categories;

    public CategoriesController(CategoryService categories)
    {
        this.categories = categories;
    }

    [HttpGet]
    public IActionResult Index(string? search)
    {
        ViewData["Search"] = search;
        return View(categories.Search(search));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var category = categories.GetDetails(id);
        return category is null ? NotFound() : View(category);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Create() => View(new CategoryFormViewModel());

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public IActionResult Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            categories.Create(ToEntity(model));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }

        TempData["Message"] = "Categoría creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var category = categories.GetDetails(id);
        return category is null ? NotFound() : View(ToViewModel(category));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public IActionResult Edit(int id, CategoryFormViewModel model)
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
            if (!categories.Update(id, ToEntity(model)))
            {
                return NotFound();
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }

        TempData["Message"] = "Categoría actualizada correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var category = categories.GetDetails(id);
        return category is null ? NotFound() : View(category);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!categories.Delete(id))
        {
            return NotFound();
        }

        TempData["Message"] = "Categoría eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Convierte el modelo del formulario en la entidad persistente.</summary>
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
