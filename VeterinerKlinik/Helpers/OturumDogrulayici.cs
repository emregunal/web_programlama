using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;

namespace VeterinerKlinik.Helpers;

public static class OturumDogrulayici
{
    public static async Task DogrulaAsync(CookieValidatePrincipalContext context)
    {
        var kimlikMetni = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var rolMetni = context.Principal?.FindFirstValue(ClaimTypes.Role);
        var db = context.HttpContext.RequestServices.GetRequiredService<KlinikDbContext>();

        var kullanici = int.TryParse(kimlikMetni, out var kimlik)
            ? await db.Kullanicilar.AsNoTracking().FirstOrDefaultAsync(k => k.Id == kimlik)
            : null;

        if (kullanici == null || !kullanici.Aktif || kullanici.Rol.ToString() != rolMetni)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
