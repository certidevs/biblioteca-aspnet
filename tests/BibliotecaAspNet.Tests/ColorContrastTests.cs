using BibliotecaAspNet.Utilities;

namespace BibliotecaAspNet.Tests;

/// <summary>
/// Primer test de referencia: protege el contraste de las etiquetas configurables
/// sin necesitar servidor web ni una base de datos.
/// </summary>
public sealed class ColorContrastTests
{
    [Theory]
    [InlineData("#008000", "#ffffff")]
    [InlineData("#eef2ff", "#122033")]
    [InlineData("#12G456", "#122033")]
    public void TextColor_ChoosesReadableText_ForValidAndInvalidBackgrounds(
        string backgroundColor,
        string expectedTextColor)
    {
        var textColor = ColorContrast.TextColor(backgroundColor);

        Assert.Equal(expectedTextColor, textColor);
    }
}
