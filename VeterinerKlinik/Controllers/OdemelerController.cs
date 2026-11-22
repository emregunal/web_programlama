using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Controllers;

public class OdemelerController : Controller
{
    private readonly KlinikDbContext _db;

    public OdemelerController(KlinikDbContext db)
    {
        _db = db;
    }

    private async Task<Muayene?> MuayeneyiYukleAsync(int muayeneId) =>
        await _db.Muayeneler.AsNoTracking()
            .Include(m => m.Hayvan).ThenInclude(h => h!.Sahip)
            .HesapIle()
            .FirstOrDefaultAsync(m => m.Id == muayeneId);

    private async Task MuayeneListesiniDoldurAsync(int? seciliMuayeneId)
    {
        var muayeneler = await _db.Muayeneler.AsNoTracking()
            .Include(m => m.Hayvan).ThenInclude(h => h!.Sahip)
            .HesapIle()
            .ToListAsync();

        var liste = muayeneler
            .Where(m => m.KalanBorc > 0 || m.Id == seciliMuayeneId)
            .OrderByDescending(m => m.Tarih)
            .Select(m => new
            {
                m.Id,
                Metin = $"{m.KisaTanim} ({m.Hayvan!.Sahip!.AdSoyad}) - Kalan: {m.KalanBorc.TL()}"
            })
            .ToList();
        ViewData["Muayeneler"] = new SelectList(liste, "Id", "Metin", seciliMuayeneId);
    }

    private async Task<Muayene?> DogrulaVeMuayeneyiGetirAsync(Odeme odeme, decimal geriEklenecekTutar)
    {
        if (odeme.MuayeneId == null)
        {
            ModelState.AddModelError(nameof(Odeme.MuayeneId), "Muayene seçin.");
            return null;
        }

        var muayene = await MuayeneyiYukleAsync(odeme.MuayeneId.Value);
        if (muayene == null)
        {
            ModelState.AddModelError(nameof(Odeme.MuayeneId), "Seçilen muayene bulunamadı.");
            return null;
        }

        if (ModelState.GetFieldValidationState(nameof(Odeme.Tutar)) != ModelValidationState.Invalid)
        {
            var kalan = muayene.KalanBorc + geriEklenecekTutar;
            if (kalan <= 0)
            {
                ModelState.AddModelError(nameof(Odeme.Tutar), "Bu muayenenin ödenecek borcu yok.");
            }
            else if (odeme.Tutar > kalan)
            {
                if (geriEklenecekTutar > 0)
                    ModelState.AddModelError(nameof(Odeme.Tutar), $"Tutar en fazla {kalan.TL()} olabilir.");
                else
                    ModelState.AddModelError(nameof(Odeme.Tutar), $"Tutar kalan borcu ({kalan.TL()}) aşamaz.");
            }
        }

        if (ModelState.GetFieldValidationState(nameof(Odeme.Tarih)) != ModelValidationState.Invalid)
        {
            if (odeme.Tarih > DateTime.Now.AddMinutes(5))
                ModelState.AddModelError(nameof(Odeme.Tarih), "Ödeme tarihi ileri bir tarih olamaz.");
            else if (odeme.Tarih.Date < muayene.Tarih.Date)
                ModelState.AddModelError(nameof(Odeme.Tarih), "Ödeme tarihi muayene tarihinden önce olamaz.");
        }

        return muayene;
    }

    private async Task MuayeneBilgisiniDoldurAsync(int? muayeneId)
    {
        if (muayeneId == null)
        {
            ViewData["MuayeneBilgi"] = "—";
            return;
        }

        var muayene = await _db.Muayeneler.AsNoTracking()
            .Include(m => m.Hayvan).ThenInclude(h => h!.Sahip)
            .FirstOrDefaultAsync(m => m.Id == muayeneId);
        ViewData["MuayeneBilgi"] = muayene == null
            ? "—"
            : $"{muayene.KisaTanim} ({muayene.Hayvan!.Sahip!.AdSoyad})";
    }

    public async Task<IActionResult> Index(string? arama, OdemeYontemi? yontem, DateTime? baslangic, DateTime? bitis)
    {
        arama = arama?.Trim();

        var sorgu = _db.Odemeler.AsNoTracking()
            .Include(o => o.Sahip)
            .Include(o => o.Muayene).ThenInclude(m => m!.Hayvan)
            .AsQueryable();

        if (yontem != null)
            sorgu = sorgu.Where(o => o.Yontem == yontem);

        if (baslangic != null && bitis != null && baslangic.Value.Date > bitis.Value.Date)
        {
            ViewData["FiltreHata"] = "Başlangıç tarihi bitiş tarihinden sonra olamaz.";
        }
        else
        {
            if (baslangic != null)
            {
                var bas = baslangic.Value.Date;
                sorgu = sorgu.Where(o => o.Tarih >= bas);
            }
            if (bitis != null && bitis.Value.Date < DateTime.MaxValue.Date)
            {
                var son = bitis.Value.Date.AddDays(1);
                sorgu = sorgu.Where(o => o.Tarih < son);
            }
        }

        var odemeler = await sorgu.OrderByDescending(o => o.Tarih).ThenByDescending(o => o.Id).ToListAsync();

        var liste = odemeler.Select(o => new OdemeListeOgesi
        {
            Id = o.Id,
            Tarih = o.Tarih,
            SahipId = o.SahipId,
            SahipAdSoyad = o.Sahip!.AdSoyad,
            MuayeneId = o.MuayeneId,
            HayvanAdi = o.Muayene?.Hayvan?.Ad,
            Yontem = o.Yontem,
            Tutar = o.Tutar,
            Aciklama = o.Aciklama
        }).ToList();

        if (!string.IsNullOrEmpty(arama))
            liste = liste.Where(o =>
                TurkceMetin.Icerir(o.SahipAdSoyad, arama) ||
                TurkceMetin.Icerir(o.HayvanAdi, arama) ||
                TurkceMetin.Icerir(o.Aciklama, arama)).ToList();

        ViewData["Yontemler"] = EnumHelper.SecimListesi(yontem);
        ViewData["Arama"] = arama;
        ViewData["Baslangic"] = baslangic?.ToString("yyyy-MM-dd");
        ViewData["Bitis"] = bitis?.ToString("yyyy-MM-dd");
        return View(liste);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var odeme = await _db.Odemeler.AsNoTrackingWithIdentityResolution()
            .Include(o => o.Sahip)
            .Include(o => o.Muayene).ThenInclude(m => m!.Hayvan)
            .Include(o => o.Muayene).ThenInclude(m => m!.ReceteKalemleri)
            .Include(o => o.Muayene).ThenInclude(m => m!.Odemeler)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (odeme == null) return NotFound();

        return View(odeme);
    }

    public async Task<IActionResult> Create(int? muayeneId)
    {
        var odeme = new Odeme { Tarih = DateTime.Now, Yontem = OdemeYontemi.Nakit };

        if (muayeneId != null)
        {
            var muayene = await MuayeneyiYukleAsync(muayeneId.Value);
            if (muayene == null) return NotFound();

            if (muayene.KalanBorc <= 0)
            {
                TempData["Hata"] = "Bu muayenenin ödenecek borcu yok.";
                return RedirectToAction("Details", "Muayeneler", new { id = muayeneId });
            }

            odeme.MuayeneId = muayeneId;
            odeme.Tutar = muayene.KalanBorc;
        }

        await MuayeneListesiniDoldurAsync(odeme.MuayeneId);
        return View(odeme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MuayeneId,Tutar,Tarih,Yontem,Aciklama")] Odeme odeme)
    {
        await using var islem = await _db.Database.BeginTransactionAsync();
        var muayene = await DogrulaVeMuayeneyiGetirAsync(odeme, geriEklenecekTutar: 0);

        if (!ModelState.IsValid)
        {
            await MuayeneListesiniDoldurAsync(odeme.MuayeneId);
            return View(odeme);
        }

        odeme.SahipId = muayene!.Hayvan!.SahipId;
        _db.Odemeler.Add(odeme);
        await _db.SaveChangesAsync();
        await islem.CommitAsync();

        TempData["Mesaj"] = "Ödeme kaydedildi.";
        return RedirectToAction(nameof(Details), new { id = odeme.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var odeme = await _db.Odemeler.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (odeme == null) return NotFound();

        if (odeme.MuayeneId == null)
        {
            TempData["Hata"] = "Muayeneye bağlı olmayan ödeme düzenlenemez.";
            return RedirectToAction(nameof(Details), new { id = odeme.Id });
        }

        await MuayeneBilgisiniDoldurAsync(odeme.MuayeneId);
        return View(odeme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Tutar,Tarih,Yontem,Aciklama")] Odeme odeme)
    {
        if (id != odeme.Id) return NotFound();

        await using var islem = await _db.Database.BeginTransactionAsync();
        var mevcut = await _db.Odemeler.FirstOrDefaultAsync(o => o.Id == id);
        if (mevcut == null) return NotFound();

        if (mevcut.MuayeneId == null)
        {
            TempData["Hata"] = "Muayeneye bağlı olmayan ödeme düzenlenemez.";
            return RedirectToAction(nameof(Details), new { id = mevcut.Id });
        }

        odeme.MuayeneId = mevcut.MuayeneId;
        await DogrulaVeMuayeneyiGetirAsync(odeme, geriEklenecekTutar: mevcut.Tutar);

        if (!ModelState.IsValid)
        {
            await MuayeneBilgisiniDoldurAsync(mevcut.MuayeneId);
            return View(odeme);
        }

        mevcut.Tutar = odeme.Tutar;
        mevcut.Tarih = odeme.Tarih;
        mevcut.Yontem = odeme.Yontem;
        mevcut.Aciklama = odeme.Aciklama;
        await _db.SaveChangesAsync();
        await islem.CommitAsync();

        TempData["Mesaj"] = "Ödeme güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var odeme = await _db.Odemeler.AsNoTracking()
            .Include(o => o.Sahip)
            .Include(o => o.Muayene).ThenInclude(m => m!.Hayvan)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (odeme == null) return NotFound();

        return View(odeme);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var odeme = await _db.Odemeler.FirstOrDefaultAsync(o => o.Id == id);
        if (odeme == null) return NotFound();

        var muayeneId = odeme.MuayeneId;
        _db.Odemeler.Remove(odeme);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Ödeme silindi.";
        if (muayeneId != null)
            return RedirectToAction("Details", "Muayeneler", new { id = muayeneId }, "odemeler");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Fatura(int? id)
    {
        if (id == null) return NotFound();
        var muayene = await _db.Muayeneler.AsNoTracking()
            .Include(m => m.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(m => m.Veteriner)
            .Include(m => m.ReceteKalemleri.OrderBy(k => k.Id)).ThenInclude(k => k.Ilac)
            .Include(m => m.Odemeler.OrderBy(o => o.Tarih).ThenBy(o => o.Id))
            .FirstOrDefaultAsync(m => m.Id == id);
        if (muayene == null) return NotFound();

        return View(muayene);
    }
}
