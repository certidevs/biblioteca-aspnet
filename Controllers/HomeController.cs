using System.Diagnostics;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

public sealed class HomeController : Controller
{
    private readonly IBookService books;
    private readonly IAuthorService authors;
    private readonly ICategoryService categories;

    public HomeController(
        IBookService books,
        IAuthorService authors,
        ICategoryService categories)
    {
        this.books = books;
        this.authors = authors;
        this.categories = categories;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var featuredBooks = await books.SearchAsync(
            search: null,
            authorId: null,
            categoryId: null,
            available: true,
            favoritesOnly: false,
            userId: null,
            cancellationToken);

        var model = new DashboardViewModel
        {
            BookCount = await books.CountAsync(cancellationToken),
            AuthorCount = await authors.CountAsync(cancellationToken),
            CategoryCount = await categories.CountAsync(cancellationToken),
            FeaturedBooks = featuredBooks.Take(3).ToList()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
