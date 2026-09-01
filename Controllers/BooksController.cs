using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaAspNet.Controllers;

public sealed class BooksController : Controller
{
    private readonly IBookService books;
    private readonly IAuthorService authors;
    private readonly ICategoryService categories;
    private readonly ICartService cart;

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
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var book = await books.GetDetailsAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new BookFormViewModel();
        await FillCatalogOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
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
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await books.GetDetailsAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
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

    private async Task FillCatalogOptionsAsync(
        BookFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Authors = await authors.SearchAsync(null, cancellationToken);
        model.Categories = await categories.SearchAsync(null, cancellationToken);
    }

    private IActionResult RedirectToLocal(string? returnUrl, int bookId)
    {
        return Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl!)
            : RedirectToAction(nameof(Details), new { id = bookId })!;
    }
}
