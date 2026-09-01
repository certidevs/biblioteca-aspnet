namespace BibliotecaAspNet.Utilities;

/// <summary>
/// Normaliza los colores configurables de las categorías y elige un texto con
/// contraste suficiente para que la etiqueta siga siendo legible.
/// </summary>
public static class ColorContrast
{
    private const string DefaultBackground = "#eef2ff";
    private const string DarkText = "#122033";
    private const string LightText = "#ffffff";

    /// <summary>Devuelve un hexadecimal válido o un fondo claro por defecto.</summary>
    public static string NormalizeHex(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return DefaultBackground;
        }

        var value = color.Trim();
        if (value.Length == 4 && value[0] == '#')
        {
            value = $"#{value[1]}{value[1]}{value[2]}{value[2]}{value[3]}{value[3]}";
        }

        return value.Length == 7 && value[0] == '#' &&
               value.Skip(1).All(Uri.IsHexDigit)
            ? value
            : DefaultBackground;
    }

    /// <summary>Elige texto blanco u oscuro según el contraste del fondo.</summary>
    public static string TextColor(string? backgroundColor)
    {
        var color = NormalizeHex(backgroundColor);
        var red = Convert.ToInt32(color[1..3], 16) / 255d;
        var green = Convert.ToInt32(color[3..5], 16) / 255d;
        var blue = Convert.ToInt32(color[5..7], 16) / 255d;

        var luminance = 0.2126 * Linearize(red)
            + 0.7152 * Linearize(green)
            + 0.0722 * Linearize(blue);
        var whiteContrast = 1.05 / (luminance + 0.05);
        var darkContrast = (luminance + 0.05) / 0.05;
        return whiteContrast >= darkContrast ? LightText : DarkText;
    }

    /// <summary>Convierte un canal sRGB para calcular luminancia relativa.</summary>
    private static double Linearize(double channel)
    {
        return channel <= 0.03928
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }
}
