namespace BibliotecaAspNet.Utilities;

/// <summary>
/// Validador de ISBN-10 e ISBN-13 con sus algoritmos de checksum.
/// </summary>
public static class IsbnValidator
{
    /// <summary>Normaliza guiones/espacios y valida ISBN-10 o ISBN-13.</summary>
    public static bool IsValid(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return false;
        }

        var normalized = isbn
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();

        return normalized.Length switch
        {
            10 => IsValidIsbn10(normalized),
            13 => IsValidIsbn13(normalized),
            _ => false
        };
    }

    /// <summary>Valida el checksum ponderado de un ISBN-10.</summary>
    private static bool IsValidIsbn10(string value)
    {
        var sum = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var digit = value[index] == 'X' && index == 9
                ? 10
                : value[index] - '0';
            if (digit is < 0 or > 9 && !(index == 9 && digit == 10))
            {
                return false;
            }

            sum += (10 - index) * digit;
        }

        return sum % 11 == 0;
    }

    /// <summary>Valida el checksum alterno de un ISBN-13.</summary>
    private static bool IsValidIsbn13(string value)
    {
        if (value.Any(character => !char.IsDigit(character)))
        {
            return false;
        }

        var sum = 0;
        for (var index = 0; index < 12; index++)
        {
            var digit = value[index] - '0';
            sum += index % 2 == 0 ? digit : digit * 3;
        }

        var checkDigit = (10 - sum % 10) % 10;
        return checkDigit == value[12] - '0';
    }
}
