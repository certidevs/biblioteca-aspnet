using BibliotecaAspNet.ViewModels.Cart;

namespace BibliotecaAspNet.Services;

/// <summary>
/// Contrato del carrito temporal. Guarda IDs y cantidades en la sesión HTTP,
/// no entidades completas ni precios confiados al navegador.
/// </summary>
public interface ICartService
{
    /// <summary>Lee el carrito y refresca sus libros desde la base de datos.</summary>
    Task<CartViewModel> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Añade unidades después de comprobar existencia y disponibilidad.</summary>
    Task<CartOperationResult> AddAsync(int bookId, int quantity = 1, CancellationToken cancellationToken = default);

    /// <summary>Establece la cantidad o elimina la línea si llega cero.</summary>
    Task<CartOperationResult> SetQuantityAsync(int bookId, int quantity, CancellationToken cancellationToken = default);

    /// <summary>Devuelve una copia de las cantidades guardadas en sesión.</summary>
    IReadOnlyDictionary<int, int> GetQuantities();

    /// <summary>Calcula cuántas unidades hay en total.</summary>
    int GetTotalQuantity();

    /// <summary>Quita un libro del carrito.</summary>
    void Remove(int bookId);

    /// <summary>Vacía por completo el carrito de la sesión actual.</summary>
    void Clear();
}
