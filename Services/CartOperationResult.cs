namespace BibliotecaAspNet.Services;

public sealed record CartOperationResult(bool Succeeded, string? Error = null);
