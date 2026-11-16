using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Controllers;

public class IlaclarController : Controller
{
    private readonly KlinikDbContext _db;

    public IlaclarController(KlinikDbContext db)
    {
        _db = db;
    }

    private async Task<bool> AyniIlacVarMiAsync(string ad, string? form, int? haricId)
    {
        var ilaclar = await _db.Ilaclar.AsNoTracking()
            .Where(i => haricId == null || i.Id != haricId)
            .Select(i => new { i.Ad, i.Form })
            .ToListAsync();
        return ilaclar.Any(i => TurkceMetin.Esit(i.Ad, ad) && TurkceMetin.Esit(i.Form, form));
    }

    public async Task<IActionResult> Index(string? arama)
    {
        arama = arama?.Trim();

        var ilaclar = await _db.Ilaclar.AsNoTracking()
            .Select(i => new IlacListeOgesi
            {
                Id = i.Id,
                Ad = i.Ad,
                Form = i.Form,
                Birim = i.Birim,
                BirimFiyat = i.BirimFiyat,
                KullanimSayisi = _db.ReceteKalemleri.Count(k => k.IlacId == i.Id)
            })
            .ToListAsync();

        if (!string.IsNullOrEmpty(arama))
            ilaclar = ilaclar.Where(i => TurkceMetin.Icerir(i.Ad, arama) || TurkceMetin.Icerir(i.Form, arama)).ToList();

        ilaclar = ilaclar.OrderBy(i => i.Ad, TurkceMetin.Siralama).ToList();

        ViewData["Arama"] = arama;
        return View(ilaclar);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Ad,Form,Birim,BirimFiyat")] Ilac ilac)
    {
        ilac.Ad = (ilac.Ad ?? string.Empty).Trim();

        if (ModelState.IsValid && await AyniIlacVarMiAsync(ilac.Ad, ilac.Form, null))
            ModelState.AddModelError(nameof(Ilac.Ad), "Bu ad ve formda kayıtlı bir ilaç zaten var.");

        if (!ModelState.IsValid) return View(ilac);

        _db.Ilaclar.Add(ilac);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "İlaç kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var ilac = await _db.Ilaclar.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (ilac == null) return NotFound();
        return View(ilac);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Form,Birim,BirimFiyat")] Ilac ilac)
    {
        if (id != ilac.Id) return NotFound();

        var mevcut = await _db.Ilaclar.FirstOrDefaultAsync(i => i.Id == id);
        if (mevcut == null) return NotFound();

        ilac.Ad = (ilac.Ad ?? string.Empty).Trim();

        if (ModelState.IsValid && await AyniIlacVarMiAsync(ilac.Ad, ilac.Form, id))
            ModelState.AddModelError(nameof(Ilac.Ad), "Bu ad ve formda kayıtlı bir ilaç zaten var.");

        if (!ModelState.IsValid) return View(ilac);

        mevcut.Ad = ilac.Ad;
        mevcut.Form = ilac.Form;
        mevcut.Birim = ilac.Birim;
        mevcut.BirimFiyat = ilac.BirimFiyat;
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "İlaç güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var ilac = await _db.Ilaclar.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (ilac == null) return NotFound();

        ViewData["KullanimSayisi"] = await _db.ReceteKalemleri.CountAsync(k => k.IlacId == id);
        return View(ilac);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ilac = await _db.Ilaclar.FirstOrDefaultAsync(i => i.Id == id);
        if (ilac == null) return NotFound();

        if (await _db.ReceteKalemleri.AnyAsync(k => k.IlacId == id))
        {
            TempData["Hata"] = "Bu ilaç reçetelerde kullanıldığı için silinemez.";
            return RedirectToAction(nameof(Index));
        }

        _db.Ilaclar.Remove(ilac);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "İlaç silindi.";
        return RedirectToAction(nameof(Index));
    }
}
