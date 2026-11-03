namespace Rotiva.Models;

/// <summary>
/// Urutan nama file seperti file manager, tanpa API khusus OS: angka dibandingkan sebagai angka
/// ("foto 2" sebelum "foto 10"), huruf tanpa membedakan besar-kecil.
/// </summary>
public sealed class NaturalOrder : IComparer<string?>
{
    public static NaturalOrder Instance { get; } = new();

    public int Compare(string? x, string? y)
    {
        if (x is null || y is null)
            return x is null ? (y is null ? 0 : -1) : 1;

        int i = 0, j = 0;
        while (i < x.Length && j < y.Length)
        {
            if (char.IsAsciiDigit(x[i]) && char.IsAsciiDigit(y[j]))
            {
                int startX = i, startY = j;
                while (i < x.Length && char.IsAsciiDigit(x[i])) i++;
                while (j < y.Length && char.IsAsciiDigit(y[j])) j++;
                // Nol di depan diabaikan, lalu angka yang lebih panjang lebih besar.
                var a = x.AsSpan(startX, i - startX).TrimStart('0');
                var b = y.AsSpan(startY, j - startY).TrimStart('0');
                int byValue = a.Length != b.Length ? a.Length.CompareTo(b.Length) : a.CompareTo(b, StringComparison.Ordinal);
                if (byValue != 0)
                    return byValue;
                continue;
            }

            int byChar = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
            if (byChar != 0)
                return byChar;
            i++;
            j++;
        }
        return (x.Length - i).CompareTo(y.Length - j);
    }
}
