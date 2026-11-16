using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Controllers;

public class SahiplerController : Controller
{
    private readonly KlinikDbContext _db;

    public SahiplerController(KlinikDbContext db)
    {
        _db = db;
    }

    private static string BosluksuzTelefon(string? telefon) =>
        string.Concat((telefon ?? string.Empty).Where(char.IsDigit));

    private async Task<bool> TelefonBaskaSahipteVarMiAsync(string telefon, int? haricId)
    {
        var hedef = BosluksuzTelefon(telefon);
        var telefonlar = await _db.Sahipler.AsNoTracking()
            .Where(s => haricId == null || s.Id != haricId)
            .Select(s => s.Telefon)
            .ToListAsync();
        return telefonlar.Any(t => BosluksuzTelefon(t) == hedef);
    }

    public async Task<IActionResult> Index(string? arama)
    {
        arama = arama?.Trim();

        var sahipler = await _db.Sahipler.AsNoTracking()
            .Select(s => new SahipListeOgesi
            {
                Id = s.Id,
                Ad = s.Ad,
                Soyad = s.Soyad,
                Telefon = s.Telefon,
                Eposta = s.Eposta,
                HayvanSayisi = s.Hayvanlar.Count,
                KayitTarihi = s.KayitTarihi
            })
            .ToListAsync();

        sahipler = sahipler.OrderBy(s => s.Soyad, TurkceMetin.Siralama).ThenBy(s => s.Ad, TurkceMetin.Siralama).ToList();

        if (!string.IsNullOrEmpty(arama))
        {
            var aramaBosluksuz = BosluksuzTelefon(arama);
            sahipler = sahipler.Where(s =>
                TurkceMetin.Icerir(s.AdSoyad, arama) ||
                TurkceMetin.Icerir(s.Telefon, arama) ||
                (aramaBosluksuz.Length > 0 && TurkceMetin.Icerir(BosluksuzTelefon(s.Telefon), aramaBosluksuz)) ||
                TurkceMetin.Icerir(s.Eposta, arama)).ToList();
        }

        ViewData["Arama"] = arama;
        return View(sahipler);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var sahip = await _db.Sahipler.AsNoTracking()
            .Include(s => s.Hayvanlar).ThenInclude(h => h.Muayeneler).ThenInclude(m => m.ReceteKalemleri)
            .Include(s => s.Hayvanlar).ThenInclude(h => h.Muayeneler).ThenInclude(m => m.Odemeler)
            .Include(s => s.Odemeler)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sahip == null) return NotFound();

        sahip.Odemeler = sahip.Odemeler.OrderByDescending(o => o.Tarih).ThenByDescending(o => o.Id).ToList();
        sahip.Hayvanlar = sahip.Hayvanlar.OrderBy(h => h.Ad, TurkceMetin.Siralama).ToList();
        return View(sahip);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Ad,Soyad,Telefon,Eposta,Adres")] Sahip sahip)
    {
        if (ModelState.IsValid && await TelefonBaskaSahipteVarMiAsync(sahip.Telefon, null))
            ModelState.AddModelError(nameof(Sahip.Telefon), "Bu telefon numarası başka bir sahibe kayıtlı.");

        if (!ModelState.IsValid) return View(sahip);

        sahip.KayitTarihi = DateTime.Now;
        _db.Sahipler.Add(sahip);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Sahip kaydedildi.";
        return RedirectToAction(nameof(Details), new { id = sahip.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var sahip = await _db.Sahipler.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (sahip == null) return NotFound();
        return View(sahip);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,Telefon,Eposta,Adres")] Sahip sahip)
    {
        if (id != sahip.Id) return NotFound();

        var mevcut = await _db.Sahipler.FirstOrDefaultAsync(s => s.Id == id);
        if (mevcut == null) return NotFound();

        if (ModelState.IsValid && await TelefonBaskaSahipteVarMiAsync(sahip.Telefon, id))
            ModelState.AddModelError(nameof(Sahip.Telefon), "Bu telefon numarası başka bir sahibe kayıtlı.");

        if (!ModelState.IsValid)
        {
            sahip.KayitTarihi = mevcut.KayitTarihi;
            return View(sahip);
        }

        mevcut.Ad = sahip.Ad;
        mevcut.Soyad = sahip.Soyad;
        mevcut.Telefon = sahip.Telefon;
        mevcut.Eposta = sahip.Eposta;
        mevcut.Adres = sahip.Adres;
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Sahip güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var sahip = await _db.Sahipler.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (sahip == null) return NotFound();

        ViewData["HayvanSayisi"] = await _db.Hayvanlar.CountAsync(h => h.SahipId == id);
        ViewData["OdemeSayisi"] = await _db.Odemeler.CountAsync(o => o.SahipId == id);
        return View(sahip);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sahip = await _db.Sahipler.FirstOrDefaultAsync(s => s.Id == id);
        if (sahip == null) return NotFound();

        var iliskiVar = await _db.Hayvanlar.AnyAsync(h => h.SahipId == id)
                        || await _db.Odemeler.AnyAsync(o => o.SahipId == id);
        if (iliskiVar)
        {
            TempData["Hata"] = "Bu sahibe kayıtlı hayvan veya ödeme olduğu için silinemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _db.Sahipler.Remove(sahip);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Sahip silindi.";
        return RedirectToAction(nameof(Index));
    }
}
