using System.Diagnostics;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

/// <summary>Página inicial con estadísticas y una pequeña selección del catálogo.</summary>
public sealed class HomeController : Controller
{
    private readonly BookService bookService;
    private readonly AuthorService authorService;
    private readonly CategoryService categoryService;

    public HomeController(
        BookService bookService,
        AuthorService authorService,
        CategoryService categoryService)
    {
        this.bookService = bookService;
        this.authorService = authorService;
        this.categoryService = categoryService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var featuredBooks = bookService.Search(available: true).Take(3).ToList();
        return View(new DashboardViewModel
        {
            BookCount = bookService.Count(),
            AuthorCount = authorService.Count(),
            CategoryCount = categoryService.Count(),
            FeaturedBooks = featuredBooks
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
