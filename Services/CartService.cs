using System.Text.Json;
using BibliotecaAspNet.Data;
using BibliotecaAspNet.ViewModels.Cart;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Carrito de la sesión actual. Solo guarda IDs y cantidades: libros y precios se leen
/// de SQLite cada vez para no confiar en datos enviados por el navegador.
/// </summary>
public sealed class CartService
{
    private const string SessionKey = "Biblioteca.Cart";
    private const int MaxQuantityPerBook = 99;

    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ApplicationDbContext context;

    public CartService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.context = context;
    }

    /// <summary>Construye el carrito con los precios actuales y retira líneas inválidas.</summary>
    public CartViewModel Get()
    {
        var quantities = ReadQuantities();
        if (quantities.Count == 0)
        {
            return new CartViewModel();
        }

        var catalogBooks = context.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .Where(book => quantities.Keys.Contains(book.Id))
            .OrderBy(book => book.Title)
            .ToList();
        var validIds = catalogBooks.Where(book => book.Available).Select(book => book.Id).ToHashSet();

        var removedInvalidLine = false;
        foreach (var id in quantities.Keys.Where(id => !validIds.Contains(id)).ToList())
        {
            quantities.Remove(id);
            removedInvalidLine = true;
        }

        if (removedInvalidLine)
        {
            WriteQuantities(quantities);
        }

        return new CartViewModel
        {
            Items = catalogBooks
                .Where(book => validIds.Contains(book.Id))
                .Select(book => new CartItemViewModel { Book = book, Quantity = quantities[book.Id] })
                .ToList()
        };
    }

    /// <summary>Añade unidades tras comprobar cantidad, existencia y disponibilidad.</summary>
    public CartOperationResult Add(int bookId, int quantity = 1)
    {
        if (quantity < 1 || quantity > MaxQuantityPerBook)
        {
            return new CartOperationResult(false, "La cantidad debe estar entre 1 y 99.");
        }

        var book = context.Books.Find(bookId);
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

    /// <summary>Cambia una cantidad o elimina la línea cuando llega a cero.</summary>
    public CartOperationResult SetQuantity(int bookId, int quantity)
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

        var book = context.Books.Find(bookId);
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

    /// <summary>Devuelve una copia de las cantidades para que checkout vuelva a validarlas.</summary>
    public IReadOnlyDictionary<int, int> GetQuantities() => ReadQuantities();

    /// <summary>Calcula el contador visible junto al icono del carrito.</summary>
    public int GetTotalQuantity() => ReadQuantities().Values.Sum();

    /// <summary>Elimina una línea y actualiza la sesión.</summary>
    public void Remove(int bookId)
    {
        var quantities = ReadQuantities();
        if (quantities.Remove(bookId))
        {
            WriteQuantities(quantities);
        }
    }

    /// <summary>Vacía por completo el carrito de la sesión actual.</summary>
    public void Clear() => Session.Remove(SessionKey);

    /// <summary>Lee el JSON de sesión y descarta valores corruptos o imposibles.</summary>
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
                : quantities.Where(item => item.Key > 0 && item.Value > 0)
                    .ToDictionary(item => item.Key, item => Math.Min(item.Value, MaxQuantityPerBook));
        }
        catch (JsonException)
        {
            Session.Remove(SessionKey);
            return new Dictionary<int, int>();
        }
    }

    /// <summary>Guarda solo datos temporales; el precio nunca entra en la sesión.</summary>
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
