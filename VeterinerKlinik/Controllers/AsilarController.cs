using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;

namespace VeterinerKlinik.Controllers;

public class AsilarController : Controller
{
    private readonly KlinikDbContext _db;

    public AsilarController(KlinikDbContext db)
    {
        _db = db;
    }

    private async Task SecimListeleriniDoldurAsync(AsiKaydi kayit, int? mevcutVeterinerId)
    {
        var hayvanlar = (await _db.Hayvanlar.AsNoTracking().Include(h => h.Sahip).ToListAsync())
            .Select(h => new { h.Id, Metin = $"{h.Ad} ({h.Tur.GetDisplayName()}) - {h.Sahip!.AdSoyad}" })
            .OrderBy(h => h.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Hayvanlar"] = new SelectList(hayvanlar, "Id", "Metin", kayit.HayvanId);

        var asilar = (await _db.Asilar.AsNoTracking().ToListAsync())
            .Select(a => new { a.Id, Metin = $"{a.Ad} ({a.TekrarAraligiGun} günde bir)" })
            .OrderBy(a => a.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Asilar"] = new SelectList(asilar, "Id", "Metin", kayit.AsiId);

        var veterinerler = (await _db.Veterinerler.AsNoTracking()
                .Where(v => v.Aktif || v.Id == mevcutVeterinerId)
                .ToListAsync())
            .Select(v => new { v.Id, Metin = v.Aktif ? v.AdSoyad : $"{v.AdSoyad} (pasif)" })
            .OrderBy(v => v.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Veterinerler"] = new SelectList(veterinerler, "Id", "Metin", kayit.VeterinerId);
    }

    private async Task KayitDogrulaAsync(AsiKaydi kayit, int? mevcutVeterinerId)
    {
        Hayvan? hayvan = null;
        if (ModelState.GetFieldValidationState(nameof(AsiKaydi.HayvanId)) != ModelValidationState.Invalid)
        {
            hayvan = await _db.Hayvanlar.AsNoTracking().FirstOrDefaultAsync(h => h.Id == kayit.HayvanId);
            if (hayvan == null)
                ModelState.AddModelError(nameof(AsiKaydi.HayvanId), "Hayvan seçin.");
        }

        Asi? asi = null;
        if (ModelState.GetFieldValidationState(nameof(AsiKaydi.AsiId)) != ModelValidationState.Invalid)
        {
            asi = await _db.Asilar.AsNoTracking().FirstOrDefaultAsync(a => a.Id == kayit.AsiId);
            if (asi == null)
                ModelState.AddModelError(nameof(AsiKaydi.AsiId), "Aşı seçin.");
        }

        if (kayit.VeterinerId != null)
        {
            var veteriner = await _db.Veterinerler.AsNoTracking().FirstOrDefaultAsync(v => v.Id == kayit.VeterinerId);
            if (veteriner == null)
                ModelState.AddModelError(nameof(AsiKaydi.VeterinerId), "Seçilen veteriner bulunamadı.");
            else if (!veteriner.Aktif && kayit.VeterinerId != mevcutVeterinerId)
                ModelState.AddModelError(nameof(AsiKaydi.VeterinerId), "Seçilen veteriner aktif değil.");
        }

        if (ModelState.GetFieldValidationState(nameof(AsiKaydi.UygulamaTarihi)) == ModelValidationState.Invalid)
            return;

        if (kayit.UygulamaTarihi.Date > DateTime.Today)
            ModelState.AddModelError(nameof(AsiKaydi.UygulamaTarihi), "Uygulama tarihi ileri bir tarih olamaz.");
        else if (hayvan?.DogumTarihi != null && kayit.UygulamaTarihi.Date < hayvan.DogumTarihi.Value.Date)
            ModelState.AddModelError(nameof(AsiKaydi.UygulamaTarihi), "Uygulama tarihi hayvanın doğum tarihinden önce olamaz.");

        if (kayit.SonrakiTarih == null && asi != null)
            kayit.SonrakiTarih = kayit.UygulamaTarihi.Date.AddDays(asi.TekrarAraligiGun);
        else if (kayit.SonrakiTarih != null && kayit.SonrakiTarih.Value.Date <= kayit.UygulamaTarihi.Date)
            ModelState.AddModelError(nameof(AsiKaydi.SonrakiTarih), "Sonraki tarih uygulama tarihinden sonra olmalıdır.");
    }

    public async Task<IActionResult> Index(string? arama, AsiDurumu? durum)
    {
        arama = arama?.Trim();

        var kayitlar = await _db.AsiKayitlari.AsNoTracking()
            .Include(a => a.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(a => a.Asi)
            .Include(a => a.Veteriner)
            .ToListAsync();

        var liste = AsiTakvimi.Listele(kayitlar);

        ViewData["GecikmisSayisi"] = liste.Count(k => k.Durum == AsiDurumu.Gecikmis);
        ViewData["YaklasanSayisi"] = liste.Count(k => k.Durum == AsiDurumu.Yaklasan);
        ViewData["GuncelSayisi"] = liste.Count(k => k.Durum == AsiDurumu.Guncel);

        if (durum != null)
            liste = liste.Where(k => k.Durum == durum).ToList();

        if (!string.IsNullOrEmpty(arama))
            liste = liste.Where(k =>
                TurkceMetin.Icerir(k.HayvanAdi, arama) ||
                TurkceMetin.Icerir(k.SahipAdSoyad, arama) ||
                TurkceMetin.Icerir(k.AsiAdi, arama)).ToList();

        ViewData["Arama"] = arama;
        ViewData["Durumlar"] = EnumHelper.SecimListesi(durum);
        return View(liste);
    }

    public async Task<IActionResult> Create(int? hayvanId)
    {
        var kayit = new AsiKaydi { UygulamaTarihi = DateTime.Today };
        if (hayvanId != null && await _db.Hayvanlar.AnyAsync(h => h.Id == hayvanId))
            kayit.HayvanId = hayvanId.Value;

        await SecimListeleriniDoldurAsync(kayit, null);
        return View(kayit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("HayvanId,AsiId,VeterinerId,UygulamaTarihi,SonrakiTarih")] AsiKaydi kayit)
    {
        await KayitDogrulaAsync(kayit, null);

        if (!ModelState.IsValid)
        {
            await SecimListeleriniDoldurAsync(kayit, null);
            return View(kayit);
        }

        _db.AsiKayitlari.Add(kayit);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Aşı kaydedildi.";
        return RedirectToAction("Details", "Hayvanlar", new { id = kayit.HayvanId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var kayit = await _db.AsiKayitlari.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (kayit == null) return NotFound();

        await SecimListeleriniDoldurAsync(kayit, kayit.VeterinerId);
        return View(kayit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,HayvanId,AsiId,VeterinerId,UygulamaTarihi,SonrakiTarih")] AsiKaydi kayit)
    {
        if (id != kayit.Id) return NotFound();

        var mevcut = await _db.AsiKayitlari.FirstOrDefaultAsync(a => a.Id == id);
        if (mevcut == null) return NotFound();

        await KayitDogrulaAsync(kayit, mevcut.VeterinerId);

        if (!ModelState.IsValid)
        {
            await SecimListeleriniDoldurAsync(kayit, mevcut.VeterinerId);
            return View(kayit);
        }

        mevcut.HayvanId = kayit.HayvanId;
        mevcut.AsiId = kayit.AsiId;
        mevcut.VeterinerId = kayit.VeterinerId;
        mevcut.UygulamaTarihi = kayit.UygulamaTarihi;
        mevcut.SonrakiTarih = kayit.SonrakiTarih;
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Aşı kaydı güncellendi.";
        return RedirectToAction("Details", "Hayvanlar", new { id = kayit.HayvanId });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var kayit = await _db.AsiKayitlari.AsNoTracking()
            .Include(a => a.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(a => a.Asi)
            .Include(a => a.Veteriner)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (kayit == null) return NotFound();
        return View(kayit);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var kayit = await _db.AsiKayitlari.FirstOrDefaultAsync(a => a.Id == id);
        if (kayit == null) return NotFound();

        _db.AsiKayitlari.Remove(kayit);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Aşı kaydı silindi.";
        return RedirectToAction("Details", "Hayvanlar", new { id = kayit.HayvanId });
    }
}
