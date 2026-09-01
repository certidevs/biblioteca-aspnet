using BibliotecaAspNet.Services;
using BibliotecaAspNet.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BibliotecaAspNet.Controllers;

[Authorize]
/// <summary>Carrito temporal y checkout de la sesión autenticada.</summary>
public sealed class CartController : Controller
{
    private readonly ICartService cartService;
    private readonly IOrderService orders;

    /// <summary>Recibe los servicios de carrito y pedidos.</summary>
    public CartController(ICartService cartService, IOrderService orders)
    {
        this.cartService = cartService;
        this.orders = orders;
    }

    [HttpGet]
    /// <summary>GET: muestra las líneas válidas del carrito.</summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await cartService.GetAsync(cancellationToken));
    }

    [HttpPost]
    /// <summary>POST: cambia unidades o quita la línea si la cantidad es cero.</summary>
    public async Task<IActionResult> Update(
        int id,
        int quantity,
        CancellationToken cancellationToken)
    {
        var result = await cartService.SetQuantityAsync(id, quantity, cancellationToken);
        if (result.Succeeded)
        {
            TempData["Message"] = quantity <= 0
                ? "Elemento quitado del carrito."
                : "Cantidad actualizada.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    /// <summary>POST: elimina un libro concreto del carrito.</summary>
    public IActionResult Remove(int id)
    {
        cartService.Remove(id);
        TempData["Message"] = "Elemento quitado del carrito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    /// <summary>POST: vacía el carrito completo.</summary>
    public IActionResult Clear()
    {
        cartService.Clear();
        TempData["Message"] = "Carrito vaciado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    /// <summary>GET: muestra el resumen y el formulario de pago ficticio.</summary>
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var cart = await cartService.GetAsync(cancellationToken);
        if (cart.IsEmpty)
        {
            TempData["Error"] = "Añade algún libro antes de continuar al pago.";
            return RedirectToAction(nameof(Index));
        }

        return View(new CheckoutPageViewModel { Cart = cart });
    }

    [HttpPost]
    /// <summary>POST: valida el pago y crea el pedido persistente.</summary>
    public async Task<IActionResult> Checkout(
        CheckoutPageViewModel model,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.GetAsync(cancellationToken);
        model.Cart = cart;
        if (cart.IsEmpty)
        {
            TempData["Error"] = "El carrito está vacío.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Challenge();
        }

        var result = await orders.CheckoutAsync(
            userId,
            cartService.GetQuantities(),
            model.Payment,
            cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("Payment.CardNumber", result.Error!);
            return View(model);
        }

        cartService.Clear();
        TempData["Message"] = "Pago ficticio realizado y pedido creado correctamente.";
        return RedirectToAction("Details", "Orders", new { id = result.Order!.Id });
    }
}
