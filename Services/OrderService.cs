using BibliotecaAspNet.Models;
using BibliotecaAspNet.Repositories;
using BibliotecaAspNet.ViewModels.Orders;

namespace BibliotecaAspNet.Services;

public sealed class OrderService : IOrderService
{
    private readonly IBookRepository books;
    private readonly IOrderRepository orders;

    public OrderService(IBookRepository books, IOrderRepository orders)
    {
        this.books = books;
        this.orders = orders;
    }

    public async Task<CheckoutResult> CheckoutAsync(
        string userId,
        IReadOnlyDictionary<int, int> quantities,
        CheckoutViewModel payment,
        CancellationToken cancellationToken = default)
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

        var catalogBooks = await books.GetByIdsAsync(requestedLines.Keys, cancellationToken);
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

        foreach (var book in catalogBooks)
        {
            var quantity = requestedLines[book.Id];
            order.Items.Add(new OrderItem
            {
                BookId = book.Id,
                BookTitle = book.Title,
                Quantity = quantity,
                UnitPrice = book.Price
            });
        }

        order.Total = order.Items.Sum(item => item.LineTotal);
        await orders.AddAsync(order, cancellationToken);
        await orders.SaveChangesAsync(cancellationToken);
        return new CheckoutResult(order);
    }

    public Task<List<Order>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return orders.GetForUserAsync(userId, cancellationToken);
    }

    public Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return orders.GetAllWithDetailsAsync(cancellationToken);
    }

    public Task<Order?> GetDetailsAsync(
        int id,
        string? userId,
        bool includeAllUsers,
        CancellationToken cancellationToken = default)
    {
        return orders.GetDetailsAsync(id, userId, includeAllUsers, cancellationToken);
    }

    public Task<int> CountForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return orders.CountForUserAsync(userId, cancellationToken);
    }

    public Task<decimal> TotalForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return orders.TotalForUserAsync(userId, cancellationToken);
    }

    private static string NormalizeCardNumber(string? cardNumber)
    {
        return new string((cardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
    }

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
