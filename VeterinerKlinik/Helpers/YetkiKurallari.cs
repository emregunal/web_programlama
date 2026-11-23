using System.Security.Claims;
using VeterinerKlinik.Models;

namespace VeterinerKlinik.Helpers;

public static class YetkiKurallari
{
    private static readonly string[] OkumaIslemleri = { "Index", "Details", "Takvim", "Recete", "Fatura" };

    private static readonly KullaniciRolu[] Hepsi =
    {
        KullaniciRolu.Admin, KullaniciRolu.Veteriner, KullaniciRolu.Sekreter
    };

    private static readonly KullaniciRolu[] AdminSekreter = { KullaniciRolu.Admin, KullaniciRolu.Sekreter };
    private static readonly KullaniciRolu[] AdminVeteriner = { KullaniciRolu.Admin, KullaniciRolu.Veteriner };
    private static readonly KullaniciRolu[] YalnizcaAdmin = { KullaniciRolu.Admin };

    private static readonly Dictionary<string, (KullaniciRolu[] Okuma, KullaniciRolu[] Yazma)> Tablo =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Home"] = (Hepsi, Hepsi),
            ["Sahipler"] = (Hepsi, AdminSekreter),
            ["Hayvanlar"] = (Hepsi, Hepsi),
            ["Randevular"] = (Hepsi, AdminSekreter),
            ["Muayeneler"] = (Hepsi, AdminVeteriner),
            ["Ilaclar"] = (Hepsi, AdminVeteriner),
            ["Asilar"] = (Hepsi, AdminVeteriner),
            ["Odemeler"] = (AdminSekreter, AdminSekreter),
            ["Kullanicilar"] = (YalnizcaAdmin, YalnizcaAdmin)
        };

    private static bool OkumaMi(string action) =>
        OkumaIslemleri.Contains(action, StringComparer.OrdinalIgnoreCase);

    private static KullaniciRolu? RolBul(ClaimsPrincipal kullanici)
    {
        if (kullanici.Identity?.IsAuthenticated != true) return null;
        var rolMetni = kullanici.FindFirstValue(ClaimTypes.Role);
        if (Enum.TryParse<KullaniciRolu>(rolMetni, out var rol) && Enum.IsDefined(rol)) return rol;
        return null;
    }

    public static bool IzinVarMi(ClaimsPrincipal kullanici, string controller, string action)
    {
        var rol = RolBul(kullanici);
        if (rol == null) return false;
        if (!Tablo.TryGetValue(controller, out var kural)) return false;
        var izinliRoller = OkumaMi(action) ? kural.Okuma : kural.Yazma;
        return izinliRoller.Contains(rol.Value);
    }

    public static bool OkuyabilirMi(ClaimsPrincipal kullanici, string controller)
    {
        var rol = RolBul(kullanici);
        if (rol == null) return false;
        return Tablo.TryGetValue(controller, out var kural) && kural.Okuma.Contains(rol.Value);
    }
}
