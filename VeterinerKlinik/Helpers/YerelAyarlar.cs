namespace VeterinerKlinik.Helpers;

internal static class YerelAyarlar
{
    public static Dictionary<string, string> Oku(string icerikDizini)
    {
        var dosya = Path.Combine(icerikDizini, ".env");
        var ustDizin = Directory.GetParent(icerikDizini);
        if (!File.Exists(dosya) && ustDizin is not null &&
            File.Exists(Path.Combine(ustDizin.FullName, "VeterinerKlinik.slnx")))
        {
            dosya = Path.Combine(ustDizin.FullName, ".env");
        }

        var ayarlar = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!File.Exists(dosya))
            return ayarlar;

        foreach (var satir in File.ReadLines(dosya))
        {
            var ayirici = satir.IndexOf('=');
            if (ayirici <= 0)
                continue;

            var anahtar = satir[..ayirici].Trim();
            if (anahtar.Length == 0 || anahtar.StartsWith('#'))
                continue;

            var deger = satir[(ayirici + 1)..].Trim();
            if (deger.Length >= 2 && (deger[0] is '\'' or '"') && deger[^1] == deger[0])
                deger = deger[1..^1];

            ayarlar[anahtar] = deger;
        }

        return ayarlar;
    }
}
