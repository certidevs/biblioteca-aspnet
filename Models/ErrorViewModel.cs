namespace BibliotecaAspNet.Models;

/// <summary>Datos mínimos que necesita la vista de error para mostrar el identificador de la petición.</summary>
public class ErrorViewModel
{
    /// <summary>Identificador de diagnóstico de la petición actual.</summary>
    public string? RequestId { get; set; }

    /// <summary>Indica si hay un identificador que merezca mostrarse al usuario.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
