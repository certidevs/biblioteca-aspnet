using System.ComponentModel.DataAnnotations;

namespace BibliotecaAspNet.ViewModels.Orders;

/// <summary>Datos de tarjeta ficticia; nunca se convierte en una entidad persistente.</summary>
public sealed class CheckoutViewModel
{
    [Required(ErrorMessage = "El titular es obligatorio.")]
    [StringLength(100, ErrorMessage = "El titular no puede superar los 100 caracteres.")]
    [Display(Name = "Titular de la tarjeta")]
    public string CardholderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
    [RegularExpression(@"^[0-9][0-9 -]{11,22}$", ErrorMessage = "Introduce una tarjeta de prueba válida de 16 dígitos.")]
    [Display(Name = "Número de tarjeta")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "La caducidad es obligatoria.")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Usa el formato MM/AA.")]
    [Display(Name = "Caducidad")]
    public string Expiry { get; set; } = string.Empty;

    [Required(ErrorMessage = "El CVV es obligatorio.")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "El CVV debe tener 3 o 4 dígitos.")]
    [DataType(DataType.Password)]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;
}
