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

    public CartService(IHttpContextAccessor httpContextAccessor, IBookRepository books)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.books = books;
    }

    public async Task<CartViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var quantities = ReadQuantities();
        if (quantities.Count == 0)
        {
            return new CartViewModel();
        }

        var catalogBooks = await books.GetByIdsAsync(quantities.Keys, cancellationToken);
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

    public IReadOnlyDictionary<int, int> GetQuantities()
    {
        return ReadQuantities();
    }

    public int GetTotalQuantity()
    {
        return ReadQuantities().Values.Sum();
    }

    public void Remove(int bookId)
    {
        var quantities = ReadQuantities();
        if (quantities.Remove(bookId))
        {
            WriteQuantities(quantities);
        }
    }

    public void Clear()
    {
        Session.Remove(SessionKey);
    }

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
            Session.Remove(SessionKey);
            return new Dictionary<int, int>();
        }
    }

    private void WriteQuantities(Dictionary<int, int> quantities)
    {
        if (quantities.Count == 0)
        {
            Clear();
            return;
        }

        Session.SetString(SessionKey, JsonSerializer.Serialize(quantities));
    }

    private ISession Session => httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("No hay una sesión HTTP disponible para el carrito.");
}
