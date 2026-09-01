using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaAspNet.Controllers;

/// <summary>Catálogo público y CRUD administrativo de libros.</summary>
public sealed class BooksController : Controller
{
    private readonly IBookService books;
    private readonly IAuthorService authors;
    private readonly ICategoryService categories;
    private readonly ICartService cart;

    /// <summary>Recibe servicios de libros, autores, categorías y carrito.</summary>
    public BooksController(
        IBookService books,
        IAuthorService authors,
        ICategoryService categories,
        ICartService cart)
    {
        this.books = books;
        this.authors = authors;
        this.categories = categories;
        this.cart = cart;
    }

    [HttpGet]
    /// <summary>GET: lista libros con filtros y prepara los selectores del catálogo.</summary>
    public async Task<IActionResult> Index(
        string? search,
        int? authorId,
        int? categoryId,
        bool? available,
        bool favoritesOnly,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var model = new BookListViewModel
        {
            Search = search,
            AuthorId = authorId,
            CategoryId = categoryId,
            Available = available,
            FavoritesOnly = favoritesOnly,
            CurrentUserId = userId,
            Books = await books.SearchAsync(search, authorId, categoryId, available, favoritesOnly, userId, cancellationToken),
            Authors = await authors.SearchAsync(null, cancellationToken),
            Categories = await categories.SearchAsync(null, cancellationToken)
        };

        return View(model);
    }

    [HttpGet]
    /// <summary>GET: muestra la ficha, favoritos y reseñas de un libro.</summary>
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var book = await books.GetDetailsAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    /// <summary>GET protegido: muestra el alta con autores y categorías disponibles.</summary>
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new BookFormViewModel();
        await FillCatalogOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    /// <summary>POST protegido: valida y crea el libro con sus categorías y portada.</summary>
    public async Task<IActionResult> Create(
        BookFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillCatalogOptionsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            await books.CreateAsync(
                model.ToBook(),
                model.SelectedCategoryIds,
                model.CoverImage,
                cancellationToken);
            TempData["Message"] = "Libro creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await FillCatalogOptionsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    /// <summary>GET protegido: carga libro y asociaciones en el formulario de edición.</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var book = await books.GetForEditAsync(id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        var model = BookFormViewModel.FromBook(book);
        await FillCatalogOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    /// <summary>POST protegido: actualiza campos, categorías y portada del libro.</summary>
    public async Task<IActionResult> Edit(
        int id,
        BookFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await FillCatalogOptionsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            var updated = await books.UpdateAsync(
                id,
                model.ToBook(),
                model.SelectedCategoryIds,
                model.CoverImage,
                model.RemoveCoverImage,
                cancellationToken);
            if (!updated)
            {
                return NotFound();
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await FillCatalogOptionsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Message"] = "Libro actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    /// <summary>GET protegido: muestra la confirmación de borrado.</summary>
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await books.GetDetailsAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    /// <summary>POST protegido: confirma el borrado del libro.</summary>
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var deleted = await books.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        TempData["Message"] = "Libro eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    /// <summary>POST autenticado: alterna el favorito y vuelve a la página de origen.</summary>
    public async Task<IActionResult> ToggleFavorite(
        int id,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var isFavorite = await books.ToggleFavoriteAsync(id, userId, cancellationToken);
        TempData["Message"] = isFavorite
            ? "Libro añadido a favoritos."
            : "Libro quitado de favoritos.";
        return RedirectToLocal(returnUrl, id);
    }

    [Authorize]
    [HttpPost]
    /// <summary>POST autenticado: añade unidades al carrito de sesión.</summary>
    public async Task<IActionResult> AddToCart(
        int id,
        int quantity = 1,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        var result = await cart.AddAsync(id, quantity, cancellationToken);
        if (result.Succeeded)
        {
            TempData["Message"] = quantity > 1
                ? $"Se han añadido {quantity} unidades al carrito."
                : "Libro añadido al carrito.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToLocal(returnUrl, id);
    }

    /// <summary>Rellena las opciones necesarias para los selectores de Create/Edit.</summary>
    private async Task FillCatalogOptionsAsync(
        BookFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Authors = await authors.SearchAsync(null, cancellationToken);
        model.Categories = await categories.SearchAsync(null, cancellationToken);
    }

    /// <summary>Evita redirecciones externas y usa el detalle como fallback.</summary>
    private IActionResult RedirectToLocal(string? returnUrl, int bookId)
    {
        return Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl!)
            : RedirectToAction(nameof(Details), new { id = bookId })!;
    }
}
