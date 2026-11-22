using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Controllers;

public class KullanicilarController : Controller
{
    private readonly KlinikDbContext _db;
    private readonly IPasswordHasher<Kullanici> _hasher;

    public KullanicilarController(KlinikDbContext db, IPasswordHasher<Kullanici> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    private async Task VeterinerListesiniDoldurAsync(int? seciliId, int? mevcutVeterinerId)
    {
        var veterinerler = (await _db.Veterinerler.AsNoTracking()
                .Where(v => v.Aktif || v.Id == mevcutVeterinerId)
                .ToListAsync())
            .Select(v => new { v.Id, Metin = v.AdSoyad })
            .OrderBy(v => v.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Veterinerler"] = new SelectList(veterinerler, "Id", "Metin", seciliId);
    }

    private static bool ParolaYeterli(string parola) =>
        parola.Length >= 8 && parola.Any(char.IsLetter) && parola.Any(char.IsDigit);

    private async Task KullaniciDogrulaAsync(KullaniciFormModel model, Kullanici? mevcut)
    {
        if (!Enum.IsDefined(model.Rol))
            ModelState.AddModelError(nameof(KullaniciFormModel.Rol), "Geçerli bir rol seçin.");

        if (ModelState.GetFieldValidationState(nameof(KullaniciFormModel.KullaniciAdi)) != ModelValidationState.Invalid)
        {
            var kucukAd = model.KullaniciAdi.ToLowerInvariant();
            var alinmis = await _db.Kullanicilar.AsNoTracking()
                .AnyAsync(k => k.KullaniciAdi.ToLower() == kucukAd && k.Id != model.Id);
            if (alinmis)
                ModelState.AddModelError(nameof(KullaniciFormModel.KullaniciAdi), "Bu kullanıcı adı alınmış.");
        }

        if (model.Rol == KullaniciRolu.Veteriner)
        {
            var mevcutVeterinerId = mevcut?.VeterinerId;
            var gecerli = model.VeterinerId != null
                          && await _db.Veterinerler.AnyAsync(v => v.Id == model.VeterinerId && (v.Aktif || v.Id == mevcutVeterinerId));
            if (!gecerli)
                ModelState.AddModelError(nameof(KullaniciFormModel.VeterinerId), "Veteriner rolü için veteriner kaydı seçin.");
        }
        else
        {
            model.VeterinerId = null;
        }

        var parolaGirildi = !string.IsNullOrEmpty(model.YeniParola) || !string.IsNullOrEmpty(model.YeniParolaTekrar);
        if (mevcut == null && string.IsNullOrEmpty(model.YeniParola))
        {
            ModelState.AddModelError(nameof(KullaniciFormModel.YeniParola), "Parola zorunludur.");
        }
        else if (parolaGirildi)
        {
            if (ModelState.GetFieldValidationState(nameof(KullaniciFormModel.YeniParola)) != ModelValidationState.Invalid
                && !ParolaYeterli(model.YeniParola ?? string.Empty))
                ModelState.AddModelError(nameof(KullaniciFormModel.YeniParola),
                    "Parola en az 8 karakter olmalı, harf ve rakam içermelidir.");
            if (model.YeniParola != model.YeniParolaTekrar)
                ModelState.AddModelError(nameof(KullaniciFormModel.YeniParolaTekrar), "Parolalar eşleşmiyor.");
        }

        if (mevcut != null)
            await YonetimKurallariniDogrulaAsync(model, mevcut);
    }

    private async Task YonetimKurallariniDogrulaAsync(KullaniciFormModel model, Kullanici mevcut)
    {
        var kendisi = mevcut.Id == User.KullaniciId();
        var yoneticiligiBitiyor = mevcut.Rol == KullaniciRolu.Admin && mevcut.Aktif
                                  && (model.Rol != KullaniciRolu.Admin || !model.Aktif);

        if (kendisi && (model.Rol != KullaniciRolu.Admin || !model.Aktif))
        {
            ModelState.AddModelError(string.Empty, "Kendi hesabınızın rolünü veya durumunu değiştiremezsiniz.");
            return;
        }

        if (yoneticiligiBitiyor)
        {
            var baskaYoneticiVar = await _db.Kullanicilar
                .AnyAsync(k => k.Id != mevcut.Id && k.Rol == KullaniciRolu.Admin && k.Aktif);
            if (!baskaYoneticiVar)
                ModelState.AddModelError(string.Empty, "Sistemde en az bir aktif yönetici kalmalıdır.");
        }
    }

    public async Task<IActionResult> Index()
    {
        var kullanicilar = await _db.Kullanicilar.AsNoTracking()
            .Select(k => new KullaniciListeOgesi
            {
                Id = k.Id,
                KullaniciAdi = k.KullaniciAdi,
                AdSoyad = k.AdSoyad,
                Rol = k.Rol,
                VeterinerAdSoyad = k.Veteriner == null ? null : k.Veteriner.Ad + " " + k.Veteriner.Soyad,
                Aktif = k.Aktif
            })
            .ToListAsync();

        kullanicilar = kullanicilar.OrderBy(k => k.AdSoyad, TurkceMetin.Siralama).ToList();
        return View(kullanicilar);
    }

    public async Task<IActionResult> Create()
    {
        await VeterinerListesiniDoldurAsync(null, null);
        return View(new KullaniciFormModel { Rol = KullaniciRolu.Sekreter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KullaniciFormModel model)
    {
        model.Id = 0;
        model.KullaniciAdi = (model.KullaniciAdi ?? string.Empty).Trim();
        model.AdSoyad = (model.AdSoyad ?? string.Empty).Trim();
        await KullaniciDogrulaAsync(model, null);

        if (!ModelState.IsValid)
        {
            await VeterinerListesiniDoldurAsync(model.VeterinerId, null);
            return View(model);
        }

        var kullanici = new Kullanici
        {
            KullaniciAdi = model.KullaniciAdi,
            AdSoyad = model.AdSoyad,
            Rol = model.Rol,
            VeterinerId = model.VeterinerId,
            Aktif = model.Aktif,
            KayitTarihi = DateTime.Now
        };
        kullanici.ParolaOzeti = _hasher.HashPassword(kullanici, model.YeniParola!);
        _db.Kullanicilar.Add(kullanici);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Kullanıcı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var kullanici = await _db.Kullanicilar.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id);
        if (kullanici == null) return NotFound();

        await VeterinerListesiniDoldurAsync(kullanici.VeterinerId, kullanici.VeterinerId);
        return View(new KullaniciFormModel
        {
            Id = kullanici.Id,
            KullaniciAdi = kullanici.KullaniciAdi,
            AdSoyad = kullanici.AdSoyad,
            Rol = kullanici.Rol,
            VeterinerId = kullanici.VeterinerId,
            Aktif = kullanici.Aktif
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KullaniciFormModel model)
    {
        if (id != model.Id) return NotFound();

        var mevcut = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.Id == id);
        if (mevcut == null) return NotFound();

        model.KullaniciAdi = (model.KullaniciAdi ?? string.Empty).Trim();
        model.AdSoyad = (model.AdSoyad ?? string.Empty).Trim();
        await KullaniciDogrulaAsync(model, mevcut);

        if (!ModelState.IsValid)
        {
            await VeterinerListesiniDoldurAsync(model.VeterinerId, mevcut.VeterinerId);
            return View(model);
        }

        mevcut.KullaniciAdi = model.KullaniciAdi;
        mevcut.AdSoyad = model.AdSoyad;
        mevcut.Rol = model.Rol;
        mevcut.VeterinerId = model.VeterinerId;
        mevcut.Aktif = model.Aktif;
        if (!string.IsNullOrEmpty(model.YeniParola))
            mevcut.ParolaOzeti = _hasher.HashPassword(mevcut, model.YeniParola);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Kullanıcı güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}
