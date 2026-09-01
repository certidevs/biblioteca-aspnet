using System.Text.Json;
using BibliotecaAspNet.Repositories;
using BibliotecaAspNet.ViewModels.Cart;
using Microsoft.AspNetCore.Http;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Carrito de la sesión actual. Solo guarda identificadores y cantidades; los precios
/// se vuelven a leer de la base de datos cuando se muestra o finaliza el pedido.
/// </summary>
public sealed class CartService : ICartService
{
    private const string SessionKey = "Biblioteca.Cart";
    private const int MaxQuantityPerBook = 99;

    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IBookRepository books;

    /// <summary>Recibe acceso a la petición y al catálogo para validar sus líneas.</summary>
    public CartService(IHttpContextAccessor httpContextAccessor, IBookRepository books)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.books = books;
    }

    /// <summary>Construye el modelo del carrito usando precios actuales del catálogo.</summary>
    public async Task<CartViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var quantities = ReadQuantities();
        if (quantities.Count == 0)
        {
            return new CartViewModel();
        }

        var catalogBooks = await books.GetByIdsAsync(quantities.Keys, cancellationToken);
        // Un libro borrado o agotado desaparece automáticamente del carrito.
        var validIds = catalogBooks
            .Where(book => book.Available)
            .Select(book => book.Id)
            .ToHashSet();

        var removedIds = quantities.Keys.Where(bookId => !validIds.Contains(bookId)).ToList();
        foreach (var removedId in removedIds)
        {
            quantities.Remove(removedId);
        }

        if (removedIds.Count > 0)
        {
            WriteQuantities(quantities);
        }

        return new CartViewModel
        {
            Items = catalogBooks
                .Where(book => validIds.Contains(book.Id))
                .Select(book => new CartItemViewModel
                {
                    Book = book,
                    Quantity = quantities[book.Id]
                })
                .ToList()
        };
    }

    /// <summary>Añade unidades después de validar cantidad, existencia y disponibilidad.</summary>
    public async Task<CartOperationResult> AddAsync(
        int bookId,
        int quantity = 1,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 1 || quantity > MaxQuantityPerBook)
        {
            return new CartOperationResult(false, "La cantidad debe estar entre 1 y 99.");
        }

        var book = await books.GetByIdAsync(bookId, cancellationToken);
        if (book is null)
        {
            return new CartOperationResult(false, "El libro no existe.");
        }

        if (!book.Available)
        {
            return new CartOperationResult(false, "Este libro no está disponible para compra.");
        }

        var quantities = ReadQuantities();
        quantities.TryGetValue(bookId, out var currentQuantity);
        quantities[bookId] = Math.Min(currentQuantity + quantity, MaxQuantityPerBook);
        WriteQuantities(quantities);
        return new CartOperationResult(true);
    }

    /// <summary>Establece la cantidad solicitada y elimina si llega cero.</summary>
    public async Task<CartOperationResult> SetQuantityAsync(
        int bookId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            Remove(bookId);
            return new CartOperationResult(true);
        }

        if (quantity > MaxQuantityPerBook)
        {
            return new CartOperationResult(false, "La cantidad máxima por libro es 99.");
        }

        var book = await books.GetByIdAsync(bookId, cancellationToken);
        if (book is null || !book.Available)
        {
            Remove(bookId);
            return new CartOperationResult(false, "Este libro ya no está disponible.");
        }

        var quantities = ReadQuantities();
        quantities[bookId] = quantity;
        WriteQuantities(quantities);
        return new CartOperationResult(true);
    }

    /// <summary>Devuelve las cantidades para que checkout las valide otra vez.</summary>
    public IReadOnlyDictionary<int, int> GetQuantities()
    {
        return ReadQuantities();
    }

    /// <summary>Calcula el contador que se muestra junto al icono del carrito.</summary>
    public int GetTotalQuantity()
    {
        return ReadQuantities().Values.Sum();
    }

    /// <summary>Elimina una línea concreta y persiste la sesión.</summary>
    public void Remove(int bookId)
    {
        var quantities = ReadQuantities();
        if (quantities.Remove(bookId))
        {
            WriteQuantities(quantities);
        }
    }

    /// <summary>Elimina la clave completa del carrito en la sesión.</summary>
    public void Clear()
    {
        Session.Remove(SessionKey);
    }

    /// <summary>Deserializa la sesión y descarta datos corruptos o fuera de rango.</summary>
    private Dictionary<int, int> ReadQuantities()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<int, int>();
        }

        try
        {
            var quantities = JsonSerializer.Deserialize<Dictionary<int, int>>(json);
            return quantities is null
                ? new Dictionary<int, int>()
                : quantities
                    .Where(item => item.Key > 0 && item.Value > 0)
                    .ToDictionary(item => item.Key, item => Math.Min(item.Value, MaxQuantityPerBook));
        }
        catch (JsonException)
        {
            // Una sesión corrupta no debe romper toda la página del catálogo.
            Session.Remove(SessionKey);
            return new Dictionary<int, int>();
        }
    }

    /// <summary>Serializa solo IDs y cantidades; el precio nunca procede de la sesión.</summary>
    private void WriteQuantities(Dictionary<int, int> quantities)
    {
        if (quantities.Count == 0)
        {
            Clear();
            return;
        }

        Session.SetString(SessionKey, JsonSerializer.Serialize(quantities));
    }

    /// <summary>Obtiene la sesión HTTP actual o informa de un uso fuera de una petición.</summary>
    private ISession Session => httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("No hay una sesión HTTP disponible para el carrito.");
}
