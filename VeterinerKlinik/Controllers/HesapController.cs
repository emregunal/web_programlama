using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Controllers;

public class HesapController : Controller
{
    private readonly KlinikDbContext _db;
    private readonly IPasswordHasher<Kullanici> _hasher;

    public HesapController(KlinikDbContext db, IPasswordHasher<Kullanici> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    private static ClaimsPrincipal KimlikOlustur(Kullanici kullanici)
    {
        var claimler = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new(ClaimTypes.Name, kullanici.KullaniciAdi),
            new("AdSoyad", kullanici.AdSoyad),
            new(ClaimTypes.Role, kullanici.Rol.ToString())
        };
        if (kullanici.VeterinerId != null)
            claimler.Add(new Claim("VeterinerId", kullanici.VeterinerId.Value.ToString()));

        var kimlik = new ClaimsIdentity(claimler, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(kimlik);
    }

    [AllowAnonymous]
    public IActionResult Giris(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new GirisModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var ad = (model.KullaniciAdi ?? string.Empty).Trim().ToLowerInvariant();
        var kullanici = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciAdi.ToLower() == ad);

        var sonuc = kullanici == null
            ? PasswordVerificationResult.Failed
            : _hasher.VerifyHashedPassword(kullanici, kullanici.ParolaOzeti, model.Parola);

        if (kullanici == null || !kullanici.Aktif || sonuc == PasswordVerificationResult.Failed)
        {
            await Task.Delay(500);
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya parola hatalı.");
            model.Parola = string.Empty;
            return View(model);
        }

        if (sonuc == PasswordVerificationResult.SuccessRehashNeeded)
        {
            kullanici.ParolaOzeti = _hasher.HashPassword(kullanici, model.Parola);
            await _db.SaveChangesAsync();
        }

        var ozellikler = new AuthenticationProperties { IsPersistent = model.BeniHatirla };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, KimlikOlustur(kullanici), ozellikler);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Giris));
    }

    public IActionResult ErisimEngellendi()
    {
        return View();
    }
}
