using System.Security.Claims;
using VeterinerKlinik.Models;

namespace VeterinerKlinik.Helpers;

public static class KullaniciBilgisi
{
    public static string AdSoyad(this ClaimsPrincipal kullanici) =>
        kullanici.FindFirstValue("AdSoyad") ?? kullanici.Identity?.Name ?? string.Empty;

    public static string RolAdi(this ClaimsPrincipal kullanici) =>
        Enum.TryParse<KullaniciRolu>(kullanici.FindFirstValue(ClaimTypes.Role), out var rol) && Enum.IsDefined(rol)
            ? rol.GetDisplayName()
            : string.Empty;

    public static int? KullaniciId(this ClaimsPrincipal kullanici) =>
        int.TryParse(kullanici.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
