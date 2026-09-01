namespace BibliotecaAspNet.Services;

/// <summary>Resultado de una operación del carrito con mensaje para la UI.</summary>
public sealed record CartOperationResult(bool Succeeded, string? Error = null);
