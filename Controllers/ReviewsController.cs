using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaAspNet.Controllers;

/// <summary>Listado público y escritura moderada de reseñas.</summary>
public sealed class ReviewsController : Controller
{
    private readonly IReviewService reviews;
    private readonly IBookService books;

    /// <summary>Recibe servicios de reseñas y libros.</summary>
    public ReviewsController(IReviewService reviews, IBookService books)
    {
        this.reviews = reviews;
        this.books = books;
    }

    [HttpGet]
    /// <summary>GET: lista reseñas con filtro opcional de puntuación.</summary>
    public async Task<IActionResult> Index(int? rating, CancellationToken cancellationToken)
    {
        ViewData["Rating"] = rating;
        return View(await reviews.SearchAsync(rating, cancellationToken));
    }

    [Authorize]
    [HttpGet]
    /// <summary>GET autenticado: muestra el formulario para un libro concreto.</summary>
    public async Task<IActionResult> Create(int bookId, CancellationToken cancellationToken)
    {
        var book = await books.GetDetailsAsync(bookId, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        return View(new ReviewFormViewModel
        {
            BookId = book.Id,
            BookTitle = book.Title
        });
    }

    [Authorize]
    [HttpPost]
    /// <summary>POST autenticado: crea la reseña usando el usuario de la cookie.</summary>
    public async Task<IActionResult> Create(
        ReviewFormViewModel model,
        CancellationToken cancellationToken)
    {
        var book = await books.GetDetailsAsync(model.BookId, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        model.BookTitle = book.Title;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        try
        {
            await reviews.CreateAsync(new Review
            {
                BookId = model.BookId,
                Comment = model.Comment,
                Rating = model.Rating
            }, userId, cancellationToken);
            TempData["Message"] = "Reseña publicada correctamente.";
            return RedirectToAction("Details", "Books", new { id = model.BookId });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [Authorize]
    [HttpGet]
    /// <summary>GET autenticado: carga una reseña si el usuario puede modificarla.</summary>
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var review = await reviews.GetByIdAsync(id, cancellationToken);
        if (review is null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !reviews.CanModify(review, userId, User.IsInRole(RoleNames.Admin)))
        {
            return Forbid();
        }

        return View(new ReviewFormViewModel
        {
            Id = review.Id,
            BookId = review.BookId,
            BookTitle = review.Book.Title,
            Comment = review.Comment,
            Rating = review.Rating
        });
    }

    [Authorize]
    [HttpPost]
    /// <summary>POST autenticado: actualiza comentario y puntuación.</summary>
    public async Task<IActionResult> Edit(
        int id,
        ReviewFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var book = await books.GetDetailsAsync(model.BookId, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        model.BookTitle = book.Title;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var updated = await reviews.UpdateAsync(
            id,
            new Review { Comment = model.Comment, Rating = model.Rating },
            userId,
            User.IsInRole(RoleNames.Admin),
            cancellationToken);
        if (!updated)
        {
            return Forbid();
        }

        TempData["Message"] = "Reseña actualizada correctamente.";
        return RedirectToAction("Details", "Books", new { id = model.BookId });
    }

    [Authorize]
    [HttpPost]
    /// <summary>POST autenticado: elimina una reseña propia o moderada por un admin.</summary>
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var review = await reviews.GetByIdAsync(id, cancellationToken);
        var bookId = review?.BookId;
        var deleted = await reviews.DeleteAsync(id, userId, User.IsInRole(RoleNames.Admin), cancellationToken);
        if (!deleted)
        {
            return Forbid();
        }

        TempData["Message"] = "Reseña eliminada correctamente.";
        return bookId.HasValue
            ? RedirectToAction("Details", "Books", new { id = bookId.Value })
            : RedirectToAction(nameof(Index));
    }
}
