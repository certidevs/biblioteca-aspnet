using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.Utilities;
using BibliotecaAspNet.ViewModels.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>Catálogo público y CRUD administrativo de libros.</summary>
public sealed class BooksController : Controller
{
    private readonly BookService bookService;
    private readonly AuthorService authorService;
    private readonly CategoryService categoryService;
    private readonly CartService cartService;

    public BooksController(
        BookService bookService,
        AuthorService authorService,
        CategoryService categoryService,
        CartService cartService)
    {
        this.bookService = bookService;
        this.authorService = authorService;
        this.categoryService = categoryService;
        this.cartService = cartService;
    }

    [HttpGet]
    /// <summary>GET: lista libros con filtros y rellena sus selectores.</summary>
    public IActionResult Index(string? search, int? authorId, int? categoryId, bool? available, bool favoritesOnly)
    {
        string? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = User.GetRequiredUserId();
        }

        return View(new BookListViewModel
        {
            Search = search,
            AuthorId = authorId,
            CategoryId = categoryId,
            Available = available,
            FavoritesOnly = favoritesOnly,
            CurrentUserId = userId,
            Books = bookService.Search(
                search: search,
                authorId: authorId,
                categoryId: categoryId,
                available: available,
                favoritesOnly: favoritesOnly,
                userId: userId),
            Authors = authorService.Search(null),
            Categories = categoryService.Search(null)
        });
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var book = bookService.GetDetails(id);
        if (book is null)
        {
            return NotFound();
        }

        return View(book);
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
            bookService.Create(model.ToBook(), model.SelectedCategoryIds, model.CoverImage);
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
        var book = bookService.GetForEdit(id);
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
            if (!bookService.Update(id, model.ToBook(), model.SelectedCategoryIds, model.CoverImage, model.RemoveCoverImage))
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
        var book = bookService.GetDetails(id);
        if (book is null)
        {
            return NotFound();
        }

        return View(book);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!bookService.Delete(id))
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
        var addedToFavorites = bookService.ToggleFavorite(id, User.GetRequiredUserId());
        TempData["Message"] = addedToFavorites
            ? "Libro añadido a favoritos."
            : "Libro quitado de favoritos.";
        return RedirectToLocal(returnUrl, id);
    }

    [Authorize]
    [HttpPost]
    public IActionResult AddToCart(int id, int quantity = 1, string? returnUrl = null)
    {
        var result = cartService.Add(id, quantity);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToLocal(returnUrl, id);
        }

        TempData["Message"] = quantity == 1
            ? "Libro añadido al carrito."
            : $"Se han añadido {quantity} unidades al carrito.";
        return RedirectToLocal(returnUrl, id);
    }

    /// <summary>Evita repetir las consultas para los selectores de Create y Edit.</summary>
    private void FillCatalogOptions(BookFormViewModel model)
    {
        model.Authors = authorService.Search(null);
        model.Categories = categoryService.Search(null);
    }

    /// <summary>Evita que un parámetro returnUrl redirija a una web externa.</summary>
    private IActionResult RedirectToLocal(string? returnUrl, int bookId)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl!);
        }

        return RedirectToAction(nameof(Details), new { id = bookId })!;
    }
}
