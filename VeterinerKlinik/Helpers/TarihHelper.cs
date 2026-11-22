namespace VeterinerKlinik.Helpers;

public static class TarihHelper
{
    public static string YasMetni(DateTime dogumTarihi, DateTime tarih)
    {
        var dogum = dogumTarihi.Date;
        var aylar = (tarih.Year - dogum.Year) * 12 + tarih.Month - dogum.Month;
        if (tarih.Day < dogum.Day) aylar--;
        if (aylar < 0) aylar = 0;

        if (aylar == 0) return "1 aydan küçük";
        if (aylar >= 12) return $"{aylar / 12} yaş";
        return $"{aylar} aylık";
    }
}
