using System.Security.Claims;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>Listado público y escritura moderada de reseñas.</summary>
public sealed class ReviewsController : Controller
{
    private readonly ReviewService reviews;
    private readonly BookService books;

    public ReviewsController(ReviewService reviews, BookService books)
    {
        this.reviews = reviews;
        this.books = books;
    }

    [HttpGet]
    public IActionResult Index(int? rating)
    {
        ViewData["Rating"] = rating;
        return View(reviews.Search(rating));
    }

    [Authorize]
    [HttpGet]
    public IActionResult Create(int bookId)
    {
        var book = books.GetDetails(bookId);
        return book is null
            ? NotFound()
            : View(new ReviewFormViewModel { BookId = book.Id, BookTitle = book.Title });
    }

    [Authorize]
    [HttpPost]
    /// <summary>POST: toma UserId de la cookie, no del formulario.</summary>
    public IActionResult Create(ReviewFormViewModel model)
    {
        var book = books.GetDetails(model.BookId);
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
            reviews.Create(new Review { BookId = model.BookId, Comment = model.Comment, Rating = model.Rating }, userId);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }

        TempData["Message"] = "Reseña publicada correctamente.";
        return RedirectToAction("Details", "Books", new { id = model.BookId });
    }

    [Authorize]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var review = reviews.GetById(id);
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
    public IActionResult Edit(int id, ReviewFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var book = books.GetDetails(model.BookId);
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

        var updated = reviews.Update(
            id,
            new Review { Comment = model.Comment, Rating = model.Rating },
            userId,
            User.IsInRole(RoleNames.Admin));
        if (!updated)
        {
            return Forbid();
        }

        TempData["Message"] = "Reseña actualizada correctamente.";
        return RedirectToAction("Details", "Books", new { id = model.BookId });
    }

    [Authorize]
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var bookId = reviews.GetById(id)?.BookId;
        if (!reviews.Delete(id, userId, User.IsInRole(RoleNames.Admin)))
        {
            return Forbid();
        }

        TempData["Message"] = "Reseña eliminada correctamente.";
        return bookId.HasValue
            ? RedirectToAction("Details", "Books", new { id = bookId.Value })
            : RedirectToAction(nameof(Index));
    }
}
