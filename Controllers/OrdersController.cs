using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaAspNet.Controllers;

[Authorize]
/// <summary>Consulta del histórico de pedidos del usuario o del administrador.</summary>
public sealed class OrdersController : Controller
{
    private readonly IOrderService orders;

    /// <summary>Recibe el servicio de pedidos.</summary>
    public OrdersController(IOrderService orders)
    {
        this.orders = orders;
    }

    [HttpGet]
    /// <summary>GET: muestra pedidos propios o todos si el usuario es administrador.</summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var model = User.IsInRole(RoleNames.Admin)
            ? await orders.GetAllAsync(cancellationToken)
            : await orders.GetForUserAsync(userId, cancellationToken);
        ViewData["IsAdminView"] = User.IsInRole(RoleNames.Admin);
        return View(model);
    }

    [HttpGet]
    /// <summary>GET: muestra un pedido si pertenece al usuario o es administrador.</summary>
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var order = await orders.GetDetailsAsync(
            id,
            userId,
            User.IsInRole(RoleNames.Admin),
            cancellationToken);
        return order is null ? NotFound() : View(order);
    }
}
