using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Controllers;

public class RandevularController : Controller
{
    private static readonly TimeSpan MesaiBaslangic = new(8, 0, 0);
    private static readonly TimeSpan MesaiBitis = new(20, 0, 0);

    private readonly KlinikDbContext _db;

    public RandevularController(KlinikDbContext db)
    {
        _db = db;
    }

    private static DateTime HaftaBasi(DateTime tarih) => tarih.Date.AddDays(-(((int)tarih.DayOfWeek + 6) % 7));

    private static RandevuListeOgesi ListeOgesineDonustur(Randevu r, int? muayeneId) => new()
    {
        Id = r.Id,
        Baslangic = r.Baslangic,
        Bitis = r.Baslangic.AddMinutes(r.SureDakika),
        HayvanId = r.HayvanId,
        HayvanAdi = r.Hayvan!.Ad,
        HayvanTuru = r.Hayvan.Tur,
        SahipId = r.Hayvan.SahipId,
        SahipAdSoyad = r.Hayvan.Sahip!.AdSoyad,
        VeterinerAdSoyad = r.Veteriner!.AdSoyad,
        Neden = r.Neden,
        Durum = r.Durum,
        MuayeneId = muayeneId
    };

    private async Task<SelectList> VeterinerFiltreListesiAsync(int? seciliId)
    {
        var veterinerler = (await _db.Veterinerler.AsNoTracking().ToListAsync())
            .Select(v => new { v.Id, Metin = v.Aktif ? v.AdSoyad : $"{v.AdSoyad} (pasif)" })
            .OrderBy(v => v.Metin, TurkceMetin.Siralama)
            .ToList();
        return new SelectList(veterinerler, "Id", "Metin", seciliId);
    }

    private async Task SecimListeleriniDoldurAsync(Randevu randevu, int? mevcutVeterinerId)
    {
        var hayvanlar = (await _db.Hayvanlar.AsNoTracking().Include(h => h.Sahip).ToListAsync())
            .Select(h => new { h.Id, Metin = $"{h.Ad} ({h.Tur.GetDisplayName()}) - {h.Sahip!.AdSoyad}" })
            .OrderBy(h => h.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Hayvanlar"] = new SelectList(hayvanlar, "Id", "Metin", randevu.HayvanId);

        var veterinerler = (await _db.Veterinerler.AsNoTracking()
                .Where(v => v.Aktif || v.Id == mevcutVeterinerId)
                .ToListAsync())
            .Select(v => new { v.Id, Metin = v.Aktif ? v.AdSoyad : $"{v.AdSoyad} (pasif)" })
            .OrderBy(v => v.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Veterinerler"] = new SelectList(veterinerler, "Id", "Metin", randevu.VeterinerId);

        ViewData["Durumlar"] = Enum.GetValues<RandevuDurumu>()
            .Where(d => d != RandevuDurumu.Tamamlandi)
            .Select(d => new SelectListItem(d.GetDisplayName(), ((int)d).ToString(), d == randevu.Durum))
            .ToList();
    }

    private async Task<List<string>> CakismaMesajlariniGetirAsync(Randevu randevu, int? haricId, Hayvan? hayvan, Veteriner? veteriner)
    {
        var mesajlar = new List<string>();
        var gunBasi = randevu.Baslangic.Date;
        var gunSonu = gunBasi.AddDays(1);
        var yeniBaslangic = randevu.Baslangic;
        var yeniBitis = randevu.Baslangic.AddMinutes(randevu.SureDakika);

        var adaylar = (await _db.Randevular.AsNoTracking()
                .Include(r => r.Hayvan)
                .Where(r => r.Durum == RandevuDurumu.Planlandi
                    && (haricId == null || r.Id != haricId)
                    && r.Baslangic >= gunBasi && r.Baslangic < gunSonu
                    && (r.VeterinerId == randevu.VeterinerId || r.HayvanId == randevu.HayvanId))
                .ToListAsync())
            .Where(r => r.Baslangic < yeniBitis && yeniBaslangic < r.Baslangic.AddMinutes(r.SureDakika))
            .OrderBy(r => r.Baslangic)
            .ToList();

        if (veteriner != null)
        {
            var cakisan = adaylar.FirstOrDefault(r => r.VeterinerId == veteriner.Id);
            if (cakisan != null)
            {
                var bitis = cakisan.Baslangic.AddMinutes(cakisan.SureDakika);
                mesajlar.Add($"{veteriner.AdSoyad} bu saatte dolu: {cakisan.Baslangic:HH:mm}–{bitis:HH:mm} ({cakisan.Hayvan!.Ad}).");
            }
        }

        if (hayvan != null)
        {
            var cakisan = adaylar.FirstOrDefault(r => r.HayvanId == hayvan.Id);
            if (cakisan != null)
            {
                var bitis = cakisan.Baslangic.AddMinutes(cakisan.SureDakika);
                mesajlar.Add($"{hayvan.Ad} için bu saatte başka randevu var: {cakisan.Baslangic:HH:mm}–{bitis:HH:mm}.");
            }
        }

        return mesajlar;
    }

    private async Task RandevuDogrulaAsync(Randevu randevu, int? haricId, bool yeniKayit, int? mevcutVeterinerId)
    {
        Hayvan? hayvan = null;
        if (ModelState.GetFieldValidationState(nameof(Randevu.HayvanId)) != ModelValidationState.Invalid)
        {
            hayvan = await _db.Hayvanlar.AsNoTracking().FirstOrDefaultAsync(h => h.Id == randevu.HayvanId);
            if (hayvan == null)
                ModelState.AddModelError(nameof(Randevu.HayvanId), "Hayvan seçin.");
        }

        Veteriner? veteriner = null;
        if (ModelState.GetFieldValidationState(nameof(Randevu.VeterinerId)) != ModelValidationState.Invalid)
        {
            veteriner = await _db.Veterinerler.AsNoTracking().FirstOrDefaultAsync(v => v.Id == randevu.VeterinerId);
            if (veteriner == null)
                ModelState.AddModelError(nameof(Randevu.VeterinerId), "Veteriner seçin.");
            else if (!veteriner.Aktif && (yeniKayit || randevu.VeterinerId != mevcutVeterinerId))
                ModelState.AddModelError(nameof(Randevu.VeterinerId), "Seçilen veteriner aktif değil.");
        }

        if (!Enum.IsDefined(randevu.Durum))
            ModelState.AddModelError(nameof(Randevu.Durum), "Geçerli bir durum seçin.");
        else if (randevu.Durum == RandevuDurumu.Tamamlandi)
            ModelState.AddModelError(nameof(Randevu.Durum), "Tamamlandı durumu muayene kaydıyla oluşur.");

        if (ModelState.GetFieldValidationState(nameof(Randevu.Baslangic)) == ModelValidationState.Invalid ||
            ModelState.GetFieldValidationState(nameof(Randevu.SureDakika)) == ModelValidationState.Invalid)
            return;

        if (yeniKayit && randevu.Baslangic < DateTime.Now.AddMinutes(-5))
        {
            ModelState.AddModelError(nameof(Randevu.Baslangic), "Geçmiş bir zamana randevu verilemez.");
            return;
        }

        if (randevu.Durum != RandevuDurumu.Planlandi)
            return;

        var bitis = randevu.Baslangic.AddMinutes(randevu.SureDakika);
        if (randevu.Baslangic.DayOfWeek == DayOfWeek.Sunday)
        {
            ModelState.AddModelError(nameof(Randevu.Baslangic), "Pazar günü randevu verilemez.");
            return;
        }

        if (randevu.Baslangic.TimeOfDay < MesaiBaslangic || bitis.Date != randevu.Baslangic.Date || bitis.TimeOfDay > MesaiBitis)
        {
            ModelState.AddModelError(nameof(Randevu.Baslangic), "Randevu 08:00–20:00 mesai saatleri içinde olmalıdır.");
            return;
        }

        foreach (var mesaj in await CakismaMesajlariniGetirAsync(randevu, haricId, hayvan, veteriner))
            ModelState.AddModelError(nameof(Randevu.Baslangic), mesaj);
    }

    public async Task<IActionResult> Index(string? arama, int? veterinerId, RandevuDurumu? durum, DateTime? tarih)
    {
        arama = arama?.Trim();
        var bugun = DateTime.Today;

        var sorgu = _db.Randevular.AsNoTracking()
            .Include(r => r.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(r => r.Veteriner)
            .AsQueryable();

        if (tarih != null)
        {
            var gun = tarih.Value.Date;
            var sonrakiGun = gun.AddDays(1);
            sorgu = sorgu.Where(r => r.Baslangic >= gun && r.Baslangic < sonrakiGun);
        }
        else
        {
            sorgu = sorgu.Where(r => r.Baslangic >= bugun);
        }

        if (veterinerId != null)
            sorgu = sorgu.Where(r => r.VeterinerId == veterinerId);

        if (durum != null)
            sorgu = sorgu.Where(r => r.Durum == durum);

        var randevular = await sorgu.ToListAsync();

        if (!string.IsNullOrEmpty(arama))
            randevular = randevular.Where(r =>
                TurkceMetin.Icerir(r.Hayvan!.Ad, arama) ||
                TurkceMetin.Icerir(r.Hayvan.Sahip!.AdSoyad, arama) ||
                TurkceMetin.Icerir(r.Neden, arama)).ToList();

        var idler = randevular.Select(r => r.Id).ToList();
        var muayeneler = (await _db.Muayeneler.AsNoTracking()
                .Where(m => m.RandevuId != null && idler.Contains(m.RandevuId.Value))
                .Select(m => new { RandevuId = m.RandevuId!.Value, m.Id })
                .ToListAsync())
            .GroupBy(m => m.RandevuId)
            .ToDictionary(g => g.Key, g => g.Min(m => m.Id));

        var liste = randevular
            .OrderBy(r => r.Baslangic).ThenBy(r => r.Id)
            .Select(r => ListeOgesineDonustur(r, muayeneler.TryGetValue(r.Id, out var muayeneId) ? muayeneId : null))
            .ToList();

        var yarin = bugun.AddDays(1);
        var haftaBasi = HaftaBasi(bugun);
        var haftaSonu = haftaBasi.AddDays(7);
        var otuzGunOnce = bugun.AddDays(-30);

        ViewData["BugunSayisi"] = await _db.Randevular
            .CountAsync(r => r.Durum == RandevuDurumu.Planlandi && r.Baslangic >= bugun && r.Baslangic < yarin);
        ViewData["HaftaSayisi"] = await _db.Randevular
            .CountAsync(r => r.Durum == RandevuDurumu.Planlandi && r.Baslangic >= haftaBasi && r.Baslangic < haftaSonu);
        ViewData["GelmediSayisi"] = await _db.Randevular
            .CountAsync(r => r.Durum == RandevuDurumu.Gelmedi && r.Baslangic >= otuzGunOnce && r.Baslangic < yarin);

        ViewData["Arama"] = arama;
        ViewData["Tarih"] = tarih?.Date;
        ViewData["Veterinerler"] = await VeterinerFiltreListesiAsync(veterinerId);
        ViewData["Durumlar"] = EnumHelper.SecimListesi(durum);
        return View(liste);
    }

    public async Task<IActionResult> Takvim(DateTime? hafta, int? veterinerId)
    {
        var haftaBasi = HaftaBasi(hafta ?? DateTime.Today);
        var haftaSonu = haftaBasi.AddDays(7);

        var sorgu = _db.Randevular.AsNoTracking()
            .Include(r => r.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(r => r.Veteriner)
            .Where(r => r.Baslangic >= haftaBasi && r.Baslangic < haftaSonu);

        if (veterinerId != null)
            sorgu = sorgu.Where(r => r.VeterinerId == veterinerId);

        var liste = (await sorgu.ToListAsync())
            .OrderBy(r => r.Baslangic).ThenBy(r => r.Id)
            .Select(r => ListeOgesineDonustur(r, null))
            .ToList();

        ViewData["HaftaBasi"] = haftaBasi;
        ViewData["VeterinerId"] = veterinerId;
        ViewData["Veterinerler"] = await VeterinerFiltreListesiAsync(veterinerId);
        return View(liste);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var randevu = await _db.Randevular.AsNoTracking()
            .Include(r => r.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(r => r.Veteriner)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (randevu == null) return NotFound();

        ViewData["MuayeneId"] = await _db.Muayeneler.AsNoTracking()
            .Where(m => m.RandevuId == id)
            .OrderBy(m => m.Id)
            .Select(m => (int?)m.Id)
            .FirstOrDefaultAsync();
        return View(randevu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumDegistir(int id, RandevuDurumu durum)
    {
        await using var islem = await _db.Database.BeginTransactionAsync();
        var randevu = await _db.Randevular
            .Include(r => r.Hayvan)
            .Include(r => r.Veteriner)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (randevu == null) return NotFound();

        if (durum == RandevuDurumu.Tamamlandi || !Enum.IsDefined(durum))
        {
            TempData["Hata"] = "Tamamlandı durumu muayene kaydıyla oluşur.";
            return RedirectToAction(nameof(Details), new { id = randevu.Id });
        }

        if (await _db.Muayeneler.AnyAsync(m => m.RandevuId == id))
        {
            TempData["Hata"] = "Muayenesi kaydedilmiş randevu değiştirilemez.";
            return RedirectToAction(nameof(Details), new { id = randevu.Id });
        }

        if (durum == RandevuDurumu.Planlandi && randevu.Durum != RandevuDurumu.Planlandi)
        {
            var mesajlar = await CakismaMesajlariniGetirAsync(randevu, randevu.Id, randevu.Hayvan, randevu.Veteriner);
            if (mesajlar.Count > 0)
            {
                TempData["Hata"] = string.Join(" ", mesajlar);
                return RedirectToAction(nameof(Details), new { id = randevu.Id });
            }
        }

        randevu.Durum = durum;
        await _db.SaveChangesAsync();
        await islem.CommitAsync();

        TempData["Mesaj"] = "Randevu durumu güncellendi.";
        return RedirectToAction(nameof(Details), new { id = randevu.Id });
    }

    public async Task<IActionResult> Create(int? hayvanId, int? veterinerId, DateTime? baslangic)
    {
        var simdi = DateTime.Now;
        var saatBasi = new DateTime(simdi.Year, simdi.Month, simdi.Day, simdi.Hour, 0, 0);
        var yarimSaat = saatBasi.AddMinutes(simdi.Minute < 30 ? 30 : 60);
        var randevu = new Randevu { Baslangic = baslangic ?? yarimSaat, SureDakika = 30, Durum = RandevuDurumu.Planlandi };

        if (hayvanId != null && await _db.Hayvanlar.AnyAsync(h => h.Id == hayvanId))
            randevu.HayvanId = hayvanId.Value;
        if (veterinerId != null && await _db.Veterinerler.AnyAsync(v => v.Id == veterinerId && v.Aktif))
            randevu.VeterinerId = veterinerId.Value;

        await SecimListeleriniDoldurAsync(randevu, null);
        return View(randevu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("HayvanId,VeterinerId,Baslangic,SureDakika,Neden")] Randevu randevu)
    {
        randevu.Durum = RandevuDurumu.Planlandi;

        await using var islem = await _db.Database.BeginTransactionAsync();
        await RandevuDogrulaAsync(randevu, null, true, null);

        if (!ModelState.IsValid)
        {
            await SecimListeleriniDoldurAsync(randevu, null);
            return View(randevu);
        }

        _db.Randevular.Add(randevu);
        await _db.SaveChangesAsync();
        await islem.CommitAsync();

        TempData["Mesaj"] = "Randevu oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = randevu.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var randevu = await _db.Randevular.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (randevu == null) return NotFound();

        if (await _db.Muayeneler.AnyAsync(m => m.RandevuId == id))
        {
            TempData["Hata"] = "Muayenesi kaydedilmiş randevu değiştirilemez.";
            return RedirectToAction(nameof(Details), new { id = randevu.Id });
        }

        await SecimListeleriniDoldurAsync(randevu, randevu.VeterinerId);
        return View(randevu);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,HayvanId,VeterinerId,Baslangic,SureDakika,Neden,Durum")] Randevu randevu)
    {
        if (id != randevu.Id) return NotFound();

        await using var islem = await _db.Database.BeginTransactionAsync();
        var mevcut = await _db.Randevular.FirstOrDefaultAsync(r => r.Id == id);
        if (mevcut == null) return NotFound();

        if (await _db.Muayeneler.AnyAsync(m => m.RandevuId == id))
        {
            TempData["Hata"] = "Muayenesi kaydedilmiş randevu değiştirilemez.";
            return RedirectToAction(nameof(Details), new { id = mevcut.Id });
        }

        await RandevuDogrulaAsync(randevu, id, false, mevcut.VeterinerId);

        if (!ModelState.IsValid)
        {
            await SecimListeleriniDoldurAsync(randevu, mevcut.VeterinerId);
            return View(randevu);
        }

        mevcut.HayvanId = randevu.HayvanId;
        mevcut.VeterinerId = randevu.VeterinerId;
        mevcut.Baslangic = randevu.Baslangic;
        mevcut.SureDakika = randevu.SureDakika;
        mevcut.Neden = randevu.Neden;
        mevcut.Durum = randevu.Durum;
        await _db.SaveChangesAsync();
        await islem.CommitAsync();

        TempData["Mesaj"] = "Randevu güncellendi.";
        return RedirectToAction(nameof(Details), new { id = mevcut.Id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var randevu = await _db.Randevular.AsNoTracking()
            .Include(r => r.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(r => r.Veteriner)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (randevu == null) return NotFound();

        ViewData["MuayeneVar"] = await _db.Muayeneler.AnyAsync(m => m.RandevuId == id);
        return View(randevu);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var randevu = await _db.Randevular.FirstOrDefaultAsync(r => r.Id == id);
        if (randevu == null) return NotFound();

        if (await _db.Muayeneler.AnyAsync(m => m.RandevuId == id))
        {
            TempData["Hata"] = "Muayenesi kaydedilmiş randevu değiştirilemez.";
            return RedirectToAction(nameof(Details), new { id = randevu.Id });
        }

        _db.Randevular.Remove(randevu);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Randevu silindi.";
        return RedirectToAction(nameof(Index));
    }
}
