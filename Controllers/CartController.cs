using BibliotecaAspNet.Services;
using BibliotecaAspNet.Utilities;
using BibliotecaAspNet.ViewModels.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAspNet.Controllers;

[Authorize]
/// <summary>Carrito temporal y checkout de la sesión autenticada.</summary>
public sealed class CartController : Controller
{
    private readonly CartService cartService;
    private readonly OrderService orderService;

    public CartController(CartService cartService, OrderService orderService)
    {
        this.cartService = cartService;
        this.orderService = orderService;
    }

    [HttpGet]
    public IActionResult Index() => View(cartService.Get());

    [HttpPost]
    /// <summary>POST: cambia unidades o retira la línea si la cantidad llega a cero.</summary>
    public IActionResult Update(int id, int quantity)
    {
        var result = cartService.SetQuantity(id, quantity);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        TempData["Message"] = quantity <= 0
            ? "Elemento quitado del carrito."
            : "Cantidad actualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(int id)
    {
        cartService.Remove(id);
        TempData["Message"] = "Elemento quitado del carrito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Clear()
    {
        cartService.Clear();
        TempData["Message"] = "Carrito vaciado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = cartService.Get();
        if (cart.IsEmpty)
        {
            TempData["Error"] = "Añade algún libro antes de continuar al pago.";
            return RedirectToAction(nameof(Index));
        }

        return View(new CheckoutPageViewModel { Cart = cart });
    }

    [HttpPost]
    /// <summary>POST: el servidor vuelve a validar el carrito antes de crear el pedido.</summary>
    public IActionResult Checkout(CheckoutPageViewModel model)
    {
        var cart = cartService.Get();
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

        var result = orderService.Checkout(
            User.GetRequiredUserId(),
            cartService.GetQuantities(),
            model.Payment);
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
