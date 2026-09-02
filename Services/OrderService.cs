using BibliotecaAspNet.Data;
using BibliotecaAspNet.Models;
using BibliotecaAspNet.ViewModels.Orders;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAspNet.Services;

/// <summary>Checkout y consultas de pedidos; valida siempre el catálogo en el servidor.</summary>
public sealed class OrderService
{
    private readonly ApplicationDbContext context;

    public OrderService(ApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>Valida tarjeta demo, disponibilidad y crea el pedido con sus líneas.</summary>
    public CheckoutResult Checkout(string userId, IReadOnlyDictionary<int, int> quantities, CheckoutViewModel payment)
    {
        var requestedLines = quantities
            .Where(item => item.Key > 0 && item.Value is >= 1 and <= 99)
            .ToDictionary(item => item.Key, item => item.Value);
        if (requestedLines.Count == 0)
        {
            return new CheckoutResult(Error: "El carrito está vacío.");
        }

        var cardNumber = NormalizeCardNumber(payment.CardNumber);
        if (cardNumber.Length != 16 || !PassesLuhnCheck(cardNumber))
        {
            return new CheckoutResult(Error: "La tarjeta de prueba debe tener 16 dígitos válidos. Usa 4242 4242 4242 4242.");
        }

        var catalogBooks = context.Books
            .Where(book => requestedLines.Keys.Contains(book.Id))
            .OrderBy(book => book.Title)
            .ToList();
        if (catalogBooks.Count != requestedLines.Count)
        {
            return new CheckoutResult(Error: "Uno de los libros del carrito ya no existe.");
        }

        var unavailableBook = catalogBooks.FirstOrDefault(book => !book.Available);
        if (unavailableBook is not null)
        {
            return new CheckoutResult(Error: $"El libro «{unavailableBook.Title}» ya no está disponible.");
        }

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Paid,
            PaymentMethod = "Tarjeta demo",
            PaymentLastFour = cardNumber[^4..]
        };

        // Se copia precio y título: el pedido sigue siendo histórico si el libro cambia.
        foreach (var book in catalogBooks)
        {
            order.Items.Add(new OrderItem
            {
                BookId = book.Id,
                BookTitle = book.Title,
                Quantity = requestedLines[book.Id],
                UnitPrice = book.Price
            });
        }

        order.Total = order.Items.Sum(item => item.LineTotal);
        context.Orders.Add(order);
        context.SaveChanges();
        return new CheckoutResult(order);
    }

    /// <summary>Lista el histórico del usuario autenticado.</summary>
    public List<Order> GetForUser(string userId) => QueryWithDetails()
        .Where(order => order.UserId == userId)
        .OrderByDescending(order => order.CreatedAt)
        .ToList();

    /// <summary>Lista todos los pedidos para el panel de administración.</summary>
    public List<Order> GetAll() => QueryWithDetails()
        .Include(order => order.User)
        .OrderByDescending(order => order.CreatedAt)
        .ToList();

    /// <summary>Devuelve un pedido solo al propietario o a un administrador.</summary>
    public Order? GetDetails(int id, string? userId, bool includeAllUsers)
    {
        var query = QueryWithDetails();
        if (!includeAllUsers)
        {
            query = query.Where(order => order.UserId == userId);
        }

        return query.Include(order => order.User).SingleOrDefault(order => order.Id == id);
    }

    /// <summary>Cuenta pedidos para el resumen del perfil.</summary>
    public int CountForUser(string userId) => context.Orders.Count(order => order.UserId == userId);

    /// <summary>Suma solo compras pagadas para el resumen del perfil.</summary>
    public decimal TotalForUser(string userId) => context.Orders
        .Where(order => order.UserId == userId && order.Status == OrderStatus.Paid)
        .Select(order => (decimal?)order.Total)
        .Sum() ?? 0m;

    /// <summary>Consulta base que carga las líneas y, si existe, el libro actual.</summary>
    private IQueryable<Order> QueryWithDetails() => context.Orders
        .AsNoTracking()
        .Include(order => order.Items)
        .ThenInclude(item => item.Book)
        .AsSplitQuery();

    private static string NormalizeCardNumber(string? cardNumber) =>
        new string((cardNumber ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>Aplica el checksum de Luhn usado en validaciones de tarjeta.</summary>
    private static bool PassesLuhnCheck(string cardNumber)
    {
        var sum = 0;
        var shouldDouble = false;
        for (var index = cardNumber.Length - 1; index >= 0; index--)
        {
            var digit = cardNumber[index] - '0';
            if (shouldDouble)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            shouldDouble = !shouldDouble;
        }

        return sum % 10 == 0;
    }
}
