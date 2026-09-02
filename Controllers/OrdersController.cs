using BibliotecaAspNet.Models;
using BibliotecaAspNet.Services;
using BibliotecaAspNet.Utilities;
using BibliotecaAspNet.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

[Authorize]
/// <summary>Histórico de pedidos propio o global cuando lo consulta un administrador.</summary>
public sealed class OrdersController : Controller
{
    private readonly OrderService orderService;

    public OrdersController(OrderService orderService)
    {
        this.orderService = orderService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var isAdmin = User.IsInRole(RoleNames.Admin);
        var orderList = isAdmin
            ? orderService.GetAll()
            : orderService.GetForUser(User.GetRequiredUserId());

        return View(new OrderIndexViewModel
        {
            IsAdminView = isAdmin,
            Orders = orderList
        });
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var order = orderService.GetDetails(
            id,
            User.GetRequiredUserId(),
            User.IsInRole(RoleNames.Admin));
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }
}
