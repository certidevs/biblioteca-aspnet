using System.Diagnostics;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>Página inicial con estadísticas y una pequeña selección del catálogo.</summary>
public sealed class HomeController : Controller
{
    private readonly BookService books;
    private readonly AuthorService authors;
    private readonly CategoryService categories;

    public HomeController(BookService books, AuthorService authors, CategoryService categories)
    {
        this.books = books;
        this.authors = authors;
        this.categories = categories;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var featuredBooks = books.Search(null, null, null, true, false, null).Take(3).ToList();
        return View(new DashboardViewModel
        {
            BookCount = books.Count(),
            AuthorCount = authors.Count(),
            CategoryCount = categories.Count(),
            FeaturedBooks = featuredBooks
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
