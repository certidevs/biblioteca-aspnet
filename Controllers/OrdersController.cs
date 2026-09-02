using System.Security.Claims;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

[Authorize]
/// <summary>Histórico de pedidos propio o global cuando lo consulta un administrador.</summary>
public sealed class OrdersController : Controller
{
    private readonly OrderService orders;

    public OrdersController(OrderService orders)
    {
        this.orders = orders;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var isAdmin = User.IsInRole(RoleNames.Admin);
        ViewData["IsAdminView"] = isAdmin;
        return View(isAdmin ? orders.GetAll() : orders.GetForUser(userId));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var order = orders.GetDetails(id, userId, User.IsInRole(RoleNames.Admin));
        return order is null ? NotFound() : View(order);
    }
}
