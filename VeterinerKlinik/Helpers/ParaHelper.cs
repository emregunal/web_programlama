namespace VeterinerKlinik.Helpers;

public static class ParaHelper
{
    public static string TL(this decimal tutar) => tutar.ToString("N2") + " ₺";
}
