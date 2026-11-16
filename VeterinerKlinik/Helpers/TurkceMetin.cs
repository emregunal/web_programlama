using System.Globalization;

namespace VeterinerKlinik.Helpers;

public static class TurkceMetin
{
    private static readonly CompareInfo TurkceKarsilastirma = new CultureInfo("tr-TR").CompareInfo;
    private static readonly CompareInfo GenelKarsilastirma = CultureInfo.InvariantCulture.CompareInfo;

    public static readonly StringComparer Siralama = StringComparer.Create(new CultureInfo("tr-TR"), true);

    public static bool Icerir(string? kaynak, string aranan) =>
        !string.IsNullOrEmpty(kaynak) &&
        (TurkceKarsilastirma.IndexOf(kaynak, aranan, CompareOptions.IgnoreCase) >= 0 ||
         GenelKarsilastirma.IndexOf(kaynak, aranan, CompareOptions.IgnoreCase) >= 0);

    public static bool Esit(string? a, string? b)
    {
        var x = (a ?? string.Empty).Trim();
        var y = (b ?? string.Empty).Trim();
        return TurkceKarsilastirma.Compare(x, y, CompareOptions.IgnoreCase) == 0 ||
               GenelKarsilastirma.Compare(x, y, CompareOptions.IgnoreCase) == 0;
    }
}
