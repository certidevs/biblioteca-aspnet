using System.Security.Claims;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>Catálogo público y CRUD administrativo de libros.</summary>
public sealed class BooksController : Controller
{
    private readonly BookService books;
    private readonly AuthorService authors;
    private readonly CategoryService categories;
    private readonly CartService cart;

    public BooksController(
        BookService books,
        AuthorService authors,
        CategoryService categories,
        CartService cart)
    {
        this.books = books;
        this.authors = authors;
        this.categories = categories;
        this.cart = cart;
    }

    [HttpGet]
    /// <summary>GET: lista libros con filtros y rellena sus selectores.</summary>
    public IActionResult Index(string? search, int? authorId, int? categoryId, bool? available, bool favoritesOnly)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return View(new BookListViewModel
        {
            Search = search,
            AuthorId = authorId,
            CategoryId = categoryId,
            Available = available,
            FavoritesOnly = favoritesOnly,
            CurrentUserId = userId,
            Books = books.Search(search, authorId, categoryId, available, favoritesOnly, userId),
            Authors = authors.Search(null),
            Categories = categories.Search(null)
        });
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var book = books.GetDetails(id);
        return book is null ? NotFound() : View(book);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Create()
    {
        var model = new BookFormViewModel();
        FillCatalogOptions(model);
        return View(model);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    /// <summary>POST: el ViewModel trae IDs; el servicio carga las entidades de verdad.</summary>
    public IActionResult Create(BookFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            FillCatalogOptions(model);
            return View(model);
        }

        try
        {
            books.Create(model.ToBook(), model.SelectedCategoryIds, model.CoverImage);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            FillCatalogOptions(model);
            return View(model);
        }

        TempData["Message"] = "Libro creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var book = books.GetForEdit(id);
        if (book is null)
        {
            return NotFound();
        }

        var model = BookFormViewModel.FromBook(book);
        FillCatalogOptions(model);
        return View(model);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public IActionResult Edit(int id, BookFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            FillCatalogOptions(model);
            return View(model);
        }

        try
        {
            if (!books.Update(id, model.ToBook(), model.SelectedCategoryIds, model.CoverImage, model.RemoveCoverImage))
            {
                return NotFound();
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            FillCatalogOptions(model);
            return View(model);
        }

        TempData["Message"] = "Libro actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var book = books.GetDetails(id);
        return book is null ? NotFound() : View(book);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!books.Delete(id))
        {
            return NotFound();
        }

        TempData["Message"] = "Libro eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    /// <summary>POST: alterna una relación N:M entre la cuenta actual y el libro.</summary>
    public IActionResult ToggleFavorite(int id, string? returnUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        TempData["Message"] = books.ToggleFavorite(id, userId)
            ? "Libro añadido a favoritos."
            : "Libro quitado de favoritos.";
        return RedirectToLocal(returnUrl, id);
    }

    [Authorize]
    [HttpPost]
    public IActionResult AddToCart(int id, int quantity = 1, string? returnUrl = null)
    {
        var result = cart.Add(id, quantity);
        TempData[result.Succeeded ? "Message" : "Error"] = result.Succeeded
            ? quantity > 1 ? $"Se han añadido {quantity} unidades al carrito." : "Libro añadido al carrito."
            : result.Error;
        return RedirectToLocal(returnUrl, id);
    }

    /// <summary>Evita repetir las consultas para los selectores de Create y Edit.</summary>
    private void FillCatalogOptions(BookFormViewModel model)
    {
        model.Authors = authors.Search(null);
        model.Categories = categories.Search(null);
    }

    /// <summary>Evita que un parámetro returnUrl redirija a una web externa.</summary>
    private IActionResult RedirectToLocal(string? returnUrl, int bookId) => Url.IsLocalUrl(returnUrl)
        ? Redirect(returnUrl!)
        : RedirectToAction(nameof(Details), new { id = bookId })!;
}
