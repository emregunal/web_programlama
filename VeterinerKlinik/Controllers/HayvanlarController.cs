using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Controllers;

public class HayvanlarController : Controller
{
    private readonly KlinikDbContext _db;

    public HayvanlarController(KlinikDbContext db)
    {
        _db = db;
    }

    private async Task SahipListesiniDoldurAsync(int seciliSahipId)
    {
        var sahipler = (await _db.Sahipler.AsNoTracking().ToListAsync())
            .Select(s => new { s.Id, Metin = $"{s.AdSoyad} - {s.Telefon}" })
            .OrderBy(s => s.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Sahipler"] = new SelectList(sahipler, "Id", "Metin", seciliSahipId);
    }

    private async Task HayvanDogrulaAsync(Hayvan hayvan, int? haricId)
    {
        if (ModelState.GetFieldValidationState(nameof(Hayvan.SahipId)) != ModelValidationState.Invalid
            && !await _db.Sahipler.AnyAsync(s => s.Id == hayvan.SahipId))
            ModelState.AddModelError(nameof(Hayvan.SahipId), "Sahip seçin.");

        if (!Enum.IsDefined(hayvan.Tur))
            ModelState.AddModelError(nameof(Hayvan.Tur), "Geçerli bir tür seçin.");
        if (!Enum.IsDefined(hayvan.Cinsiyet))
            ModelState.AddModelError(nameof(Hayvan.Cinsiyet), "Geçerli bir cinsiyet seçin.");

        if (hayvan.Kilo != null && hayvan.Kilo <= 0)
            ModelState.AddModelError(nameof(Hayvan.Kilo), "Kilo sıfırdan büyük olmalıdır.");

        if (hayvan.DogumTarihi != null)
        {
            if (hayvan.DogumTarihi.Value.Date > DateTime.Today)
                ModelState.AddModelError(nameof(Hayvan.DogumTarihi), "Doğum tarihi ileri bir tarih olamaz.");
            else if (haricId != null)
            {
                var dogum = hayvan.DogumTarihi.Value.Date;
                var oncekiKayitVar = await _db.Muayeneler.AnyAsync(m => m.HayvanId == haricId && m.Tarih < dogum)
                                     || await _db.AsiKayitlari.AnyAsync(a => a.HayvanId == haricId && a.UygulamaTarihi < dogum);
                if (oncekiKayitVar)
                    ModelState.AddModelError(nameof(Hayvan.DogumTarihi), "Doğum tarihi, hayvanın muayene veya aşı kayıtlarından sonra olamaz.");
            }
        }

        if (ModelState.IsValid)
        {
            var adlar = await _db.Hayvanlar.AsNoTracking()
                .Where(h => h.SahipId == hayvan.SahipId && (haricId == null || h.Id != haricId))
                .Select(h => h.Ad)
                .ToListAsync();
            if (adlar.Any(a => TurkceMetin.Esit(a, hayvan.Ad)))
                ModelState.AddModelError(nameof(Hayvan.Ad), "Bu sahibin aynı adda bir hayvanı zaten kayıtlı.");
        }
    }

    public async Task<IActionResult> Index(string? arama, HayvanTuru? tur)
    {
        arama = arama?.Trim();

        var sorgu = _db.Hayvanlar.AsNoTracking();
        if (tur != null)
            sorgu = sorgu.Where(h => h.Tur == tur);

        var hayvanlar = await sorgu
            .Select(h => new HayvanListeOgesi
            {
                Id = h.Id,
                Ad = h.Ad,
                Tur = h.Tur,
                Irk = h.Irk,
                Cinsiyet = h.Cinsiyet,
                DogumTarihi = h.DogumTarihi,
                SahipId = h.SahipId,
                SahipAdSoyad = h.Sahip!.Ad + " " + h.Sahip.Soyad,
                MuayeneSayisi = h.Muayeneler.Count
            })
            .ToListAsync();

        if (!string.IsNullOrEmpty(arama))
            hayvanlar = hayvanlar.Where(h =>
                TurkceMetin.Icerir(h.Ad, arama) ||
                TurkceMetin.Icerir(h.Irk, arama) ||
                TurkceMetin.Icerir(h.SahipAdSoyad, arama)).ToList();

        hayvanlar = hayvanlar.OrderBy(h => h.Ad, TurkceMetin.Siralama).ToList();

        ViewData["Arama"] = arama;
        ViewData["Turler"] = EnumHelper.SecimListesi(tur);
        return View(hayvanlar);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var hayvan = await _db.Hayvanlar.AsNoTracking()
            .Include(h => h.Sahip)
            .Include(h => h.Muayeneler).ThenInclude(m => m.Veteriner)
            .Include(h => h.Muayeneler).ThenInclude(m => m.ReceteKalemleri)
            .Include(h => h.Muayeneler).ThenInclude(m => m.Odemeler)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (hayvan == null) return NotFound();

        hayvan.Muayeneler = hayvan.Muayeneler.OrderByDescending(m => m.Tarih).ThenByDescending(m => m.Id).ToList();

        var asiKayitlari = await _db.AsiKayitlari.AsNoTracking()
            .Include(a => a.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(a => a.Asi)
            .Include(a => a.Veteriner)
            .Where(a => a.HayvanId == id)
            .ToListAsync();

        return View(new HayvanDetayModel
        {
            Hayvan = hayvan,
            AsiKayitlari = AsiTakvimi.Listele(asiKayitlari)
        });
    }

    public async Task<IActionResult> Create(int? sahipId)
    {
        var hayvan = new Hayvan();
        if (sahipId != null && await _db.Sahipler.AnyAsync(s => s.Id == sahipId))
            hayvan.SahipId = sahipId.Value;

        await SahipListesiniDoldurAsync(hayvan.SahipId);
        return View(hayvan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SahipId,Ad,Tur,Irk,Cinsiyet,DogumTarihi,Kilo,Notlar")] Hayvan hayvan)
    {
        hayvan.Ad = (hayvan.Ad ?? string.Empty).Trim();
        await HayvanDogrulaAsync(hayvan, null);

        if (!ModelState.IsValid)
        {
            await SahipListesiniDoldurAsync(hayvan.SahipId);
            return View(hayvan);
        }

        _db.Hayvanlar.Add(hayvan);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Hayvan kaydedildi.";
        return RedirectToAction(nameof(Details), new { id = hayvan.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var hayvan = await _db.Hayvanlar.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
        if (hayvan == null) return NotFound();

        await SahipListesiniDoldurAsync(hayvan.SahipId);
        return View(hayvan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,SahipId,Ad,Tur,Irk,Cinsiyet,DogumTarihi,Kilo,Notlar")] Hayvan hayvan)
    {
        if (id != hayvan.Id) return NotFound();

        var mevcut = await _db.Hayvanlar.FirstOrDefaultAsync(h => h.Id == id);
        if (mevcut == null) return NotFound();

        hayvan.Ad = (hayvan.Ad ?? string.Empty).Trim();
        await HayvanDogrulaAsync(hayvan, id);

        if (!ModelState.IsValid)
        {
            await SahipListesiniDoldurAsync(hayvan.SahipId);
            return View(hayvan);
        }

        mevcut.SahipId = hayvan.SahipId;
        mevcut.Ad = hayvan.Ad;
        mevcut.Tur = hayvan.Tur;
        mevcut.Irk = hayvan.Irk;
        mevcut.Cinsiyet = hayvan.Cinsiyet;
        mevcut.DogumTarihi = hayvan.DogumTarihi;
        mevcut.Kilo = hayvan.Kilo;
        mevcut.Notlar = hayvan.Notlar;
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Hayvan güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var hayvan = await _db.Hayvanlar.AsNoTracking()
            .Include(h => h.Sahip)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (hayvan == null) return NotFound();

        ViewData["MuayeneSayisi"] = await _db.Muayeneler.CountAsync(m => m.HayvanId == id);
        ViewData["RandevuSayisi"] = await _db.Randevular.CountAsync(r => r.HayvanId == id);
        ViewData["AsiSayisi"] = await _db.AsiKayitlari.CountAsync(a => a.HayvanId == id);
        return View(hayvan);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var hayvan = await _db.Hayvanlar.FirstOrDefaultAsync(h => h.Id == id);
        if (hayvan == null) return NotFound();

        var iliskiVar = await _db.Muayeneler.AnyAsync(m => m.HayvanId == id)
                        || await _db.Randevular.AnyAsync(r => r.HayvanId == id)
                        || await _db.AsiKayitlari.AnyAsync(a => a.HayvanId == id);
        if (iliskiVar)
        {
            TempData["Hata"] = "Bu hayvana ait muayene, randevu veya aşı kaydı olduğu için silinemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _db.Hayvanlar.Remove(hayvan);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Hayvan silindi.";
        return RedirectToAction(nameof(Index));
    }
}
