using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.Utilities;
using BibliotecaAspNet.ViewModels.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>Listado público y escritura moderada de reseñas.</summary>
public sealed class ReviewsController : Controller
{
    private readonly ReviewService reviewService;
    private readonly BookService bookService;

    public ReviewsController(ReviewService reviewService, BookService bookService)
    {
        this.reviewService = reviewService;
        this.bookService = bookService;
    }

    [HttpGet]
    public IActionResult Index(int? rating)
    {
        return View(new ReviewIndexViewModel
        {
            Rating = rating,
            Reviews = reviewService.Search(rating)
        });
    }

    [Authorize]
    [HttpGet]
    public IActionResult Create(int bookId)
    {
        var book = bookService.GetDetails(bookId);
        if (book is null)
        {
            return NotFound();
        }

        return View(new ReviewFormViewModel { BookId = book.Id, BookTitle = book.Title });
    }

    [Authorize]
    [HttpPost]
    /// <summary>POST: toma UserId de la cookie, no del formulario.</summary>
    public IActionResult Create(ReviewFormViewModel model)
    {
        var book = bookService.GetDetails(model.BookId);
        if (book is null)
        {
            return NotFound();
        }

        model.BookTitle = book.Title;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            reviewService.Create(
                new Review { BookId = model.BookId, Comment = model.Comment, Rating = model.Rating },
                User.GetRequiredUserId());
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
        var review = reviewService.GetById(id);
        if (review is null)
        {
            return NotFound();
        }

        if (!reviewService.CanModify(review, User.GetRequiredUserId(), User.IsInRole(RoleNames.Admin)))
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

        var book = bookService.GetDetails(model.BookId);
        if (book is null)
        {
            return NotFound();
        }

        model.BookTitle = book.Title;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = reviewService.Update(
            id,
            new Review { Comment = model.Comment, Rating = model.Rating },
            User.GetRequiredUserId(),
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
        var bookId = reviewService.GetById(id)?.BookId;
        if (!reviewService.Delete(id, User.GetRequiredUserId(), User.IsInRole(RoleNames.Admin)))
        {
            return Forbid();
        }

        TempData["Message"] = "Reseña eliminada correctamente.";
        if (bookId.HasValue)
        {
            return RedirectToAction("Details", "Books", new { id = bookId.Value });
        }

        return RedirectToAction(nameof(Index));
    }
}
