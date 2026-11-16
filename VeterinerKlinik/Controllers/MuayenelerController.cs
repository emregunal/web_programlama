using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Controllers;

public class MuayenelerController : Controller
{
    private readonly KlinikDbContext _db;

    public MuayenelerController(KlinikDbContext db)
    {
        _db = db;
    }

    private static string HayvanSecimMetni(Hayvan h) =>
        $"{h.Ad} ({h.Tur.GetDisplayName()}) - {h.Sahip!.AdSoyad}";

    private static string IlacSecimMetni(Ilac i)
    {
        var birim = string.IsNullOrWhiteSpace(i.Birim) ? "adet" : i.Birim;
        return $"{i.AdVeForm} - {i.BirimFiyat.TL()}/{birim}";
    }

    private async Task SecimListeleriniDoldurAsync(Muayene muayene, int? mevcutVeterinerId)
    {
        var hayvanlar = (await _db.Hayvanlar.AsNoTracking()
                .Include(h => h.Sahip)
                .ToListAsync())
            .Select(h => new { h.Id, Metin = HayvanSecimMetni(h) })
            .OrderBy(h => h.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Hayvanlar"] = new SelectList(hayvanlar, "Id", "Metin", muayene.HayvanId);

        var veterinerler = (await _db.Veterinerler.AsNoTracking()
                .Where(v => v.Aktif || v.Id == mevcutVeterinerId)
                .ToListAsync())
            .Select(v => new { v.Id, Metin = v.Aktif ? v.AdSoyad : $"{v.AdSoyad} (pasif)" })
            .OrderBy(v => v.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Veterinerler"] = new SelectList(veterinerler, "Id", "Metin", muayene.VeterinerId);

        if (muayene.RandevuId != null)
        {
            var randevu = await _db.Randevular.AsNoTracking()
                .Include(r => r.Veteriner)
                .FirstOrDefaultAsync(r => r.Id == muayene.RandevuId);
            if (randevu != null)
                ViewData["RandevuBilgi"] = $"{randevu.Baslangic:dd.MM.yyyy HH:mm} - {randevu.Veteriner!.AdSoyad}";
        }
    }

    private async Task MuayeneDogrulaAsync(Muayene muayene, int? mevcutVeterinerId, bool yeniKayit)
    {
        Hayvan? hayvan = null;
        if (ModelState.GetFieldValidationState(nameof(Muayene.HayvanId)) != ModelValidationState.Invalid)
        {
            hayvan = await _db.Hayvanlar.AsNoTracking().FirstOrDefaultAsync(h => h.Id == muayene.HayvanId);
            if (hayvan == null)
                ModelState.AddModelError(nameof(Muayene.HayvanId), "Seçilen hayvan bulunamadı.");
        }

        if (ModelState.GetFieldValidationState(nameof(Muayene.VeterinerId)) != ModelValidationState.Invalid)
        {
            var veteriner = await _db.Veterinerler.AsNoTracking().FirstOrDefaultAsync(v => v.Id == muayene.VeterinerId);
            if (veteriner == null)
                ModelState.AddModelError(nameof(Muayene.VeterinerId), "Seçilen veteriner bulunamadı.");
            else if (!veteriner.Aktif && (yeniKayit || muayene.VeterinerId != mevcutVeterinerId))
                ModelState.AddModelError(nameof(Muayene.VeterinerId), "Seçilen veteriner aktif değil.");
        }

        if (ModelState.GetFieldValidationState(nameof(Muayene.Tarih)) != ModelValidationState.Invalid)
        {
            if (muayene.Tarih > DateTime.Now.AddMinutes(5))
                ModelState.AddModelError(nameof(Muayene.Tarih), "Muayene tarihi ileri bir tarih olamaz.");
            else if (hayvan?.DogumTarihi != null && muayene.Tarih.Date < hayvan.DogumTarihi.Value.Date)
                ModelState.AddModelError(nameof(Muayene.Tarih), "Muayene tarihi hayvanın doğum tarihinden önce olamaz.");
        }

        if (yeniKayit && muayene.RandevuId != null)
        {
            var randevu = await _db.Randevular.AsNoTracking().FirstOrDefaultAsync(r => r.Id == muayene.RandevuId);
            if (randevu == null)
                ModelState.AddModelError(string.Empty, "Randevu bulunamadı.");
            else if (randevu.HayvanId != muayene.HayvanId)
                ModelState.AddModelError(string.Empty, "Randevu seçilen hayvana ait değil.");
            else if (randevu.Durum != RandevuDurumu.Planlandi)
                ModelState.AddModelError(string.Empty, "Bu randevu için muayene başlatılamaz.");
            else if (await _db.Muayeneler.AnyAsync(m => m.RandevuId == muayene.RandevuId))
                ModelState.AddModelError(string.Empty, "Bu randevu için muayene zaten kaydedilmiş.");
        }
    }

    private async Task IlacListesiniDoldurAsync(int? seciliIlacId)
    {
        var ilaclar = (await _db.Ilaclar.AsNoTracking().ToListAsync())
            .Select(i => new { i.Id, Metin = IlacSecimMetni(i) })
            .OrderBy(i => i.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Ilaclar"] = new SelectList(ilaclar, "Id", "Metin", seciliIlacId);
    }

    private async Task<MuayeneDetayModel?> DetayModeliOlusturAsync(int id, ReceteKalemi? yeniKalem)
    {
        var muayene = await _db.Muayeneler.AsNoTracking()
            .Include(m => m.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(m => m.Veteriner)
            .Include(m => m.Randevu)
            .Include(m => m.ReceteKalemleri.OrderBy(k => k.Id)).ThenInclude(k => k.Ilac)
            .Include(m => m.Odemeler.OrderBy(o => o.Tarih).ThenBy(o => o.Id))
            .FirstOrDefaultAsync(m => m.Id == id);
        if (muayene == null) return null;

        await IlacListesiniDoldurAsync(yeniKalem?.IlacId);

        var model = new MuayeneDetayModel { Muayene = muayene };
        if (yeniKalem != null) model.YeniKalem = yeniKalem;
        return model;
    }

    public async Task<IActionResult> Index(string? arama, int? veterinerId, OdemeDurumu? odemeDurumu, DateTime? baslangic, DateTime? bitis)
    {
        arama = arama?.Trim();

        var sorgu = _db.Muayeneler.AsNoTracking()
            .Include(m => m.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(m => m.Veteriner)
            .HesapIle();

        if (veterinerId != null)
            sorgu = sorgu.Where(m => m.VeterinerId == veterinerId);

        if (baslangic != null && bitis != null && baslangic.Value.Date > bitis.Value.Date)
        {
            ViewData["FiltreHata"] = "Başlangıç tarihi bitiş tarihinden sonra olamaz.";
        }
        else
        {
            if (baslangic != null)
            {
                var bas = baslangic.Value.Date;
                sorgu = sorgu.Where(m => m.Tarih >= bas);
            }
            if (bitis != null && bitis.Value.Date < DateTime.MaxValue.Date)
            {
                var son = bitis.Value.Date.AddDays(1);
                sorgu = sorgu.Where(m => m.Tarih < son);
            }
        }

        var muayeneler = await sorgu.OrderByDescending(m => m.Tarih).ThenByDescending(m => m.Id).ToListAsync();

        if (odemeDurumu != null)
            muayeneler = muayeneler.Where(m => m.OdemeDurumu == odemeDurumu).ToList();

        var liste = muayeneler.Select(m => new MuayeneListeOgesi
        {
            Id = m.Id,
            Tarih = m.Tarih,
            HayvanAdi = m.Hayvan!.Ad,
            HayvanTuru = m.Hayvan.Tur,
            SahipId = m.Hayvan.SahipId,
            SahipAdSoyad = m.Hayvan.Sahip!.AdSoyad,
            VeterinerAdSoyad = m.Veteriner!.AdSoyad,
            Teshis = m.Teshis,
            GenelToplam = m.GenelToplam,
            KalanBorc = m.KalanBorc,
            OdemeDurumu = m.OdemeDurumu
        }).ToList();

        if (!string.IsNullOrEmpty(arama))
            liste = liste.Where(m =>
                TurkceMetin.Icerir(m.HayvanAdi, arama) ||
                TurkceMetin.Icerir(m.SahipAdSoyad, arama) ||
                TurkceMetin.Icerir(m.Teshis, arama)).ToList();

        var veterinerler = (await _db.Veterinerler.AsNoTracking().ToListAsync())
            .Select(v => new { v.Id, Metin = v.AdSoyad })
            .OrderBy(v => v.Metin, TurkceMetin.Siralama)
            .ToList();
        ViewData["Veterinerler"] = new SelectList(veterinerler, "Id", "Metin", veterinerId);
        ViewData["OdemeDurumlari"] = EnumHelper.SecimListesi(odemeDurumu);
        ViewData["Arama"] = arama;
        ViewData["Baslangic"] = baslangic?.ToString("yyyy-MM-dd");
        ViewData["Bitis"] = bitis?.ToString("yyyy-MM-dd");
        return View(liste);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var model = await DetayModeliOlusturAsync(id.Value, null);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReceteKalemiEkle(int muayeneId,
        [Bind("IlacId,Doz,KullanimSekli,SureGun,Adet", Prefix = "YeniKalem")] ReceteKalemi yeniKalem)
    {
        if (!await _db.Muayeneler.AnyAsync(m => m.Id == muayeneId)) return NotFound();

        var ilac = await _db.Ilaclar.AsNoTracking().FirstOrDefaultAsync(i => i.Id == yeniKalem.IlacId);
        if (ModelState.GetFieldValidationState("YeniKalem.IlacId") != ModelValidationState.Invalid)
        {
            if (ilac == null)
                ModelState.AddModelError("YeniKalem.IlacId", "Seçilen ilaç bulunamadı.");
            else if (await _db.ReceteKalemleri.AnyAsync(k => k.MuayeneId == muayeneId && k.IlacId == yeniKalem.IlacId))
                ModelState.AddModelError("YeniKalem.IlacId", "Bu ilaç reçetede zaten var.");
        }

        if (!ModelState.IsValid || ilac == null)
            return View("Details", await DetayModeliOlusturAsync(muayeneId, yeniKalem));

        yeniKalem.MuayeneId = muayeneId;
        yeniKalem.BirimFiyat = ilac.BirimFiyat;
        _db.ReceteKalemleri.Add(yeniKalem);
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "İlaç reçeteye eklendi.";
        return RedirectToAction(nameof(Details), null, new { id = muayeneId }, fragment: "recete");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReceteKalemiSil(int id)
    {
        await using var islem = await _db.Database.BeginTransactionAsync();
        var kalem = await _db.ReceteKalemleri
            .Include(k => k.Muayene).ThenInclude(m => m!.ReceteKalemleri)
            .Include(k => k.Muayene).ThenInclude(m => m!.Odemeler)
            .FirstOrDefaultAsync(k => k.Id == id);
        if (kalem == null) return NotFound();

        var muayeneId = kalem.MuayeneId;
        var muayene = kalem.Muayene!;
        if (muayene.GenelToplam - kalem.Tutar < muayene.OdenenToplam)
        {
            TempData["Hata"] = "Bu ilaç çıkarılırsa muayene toplamı ödenen tutarın altına düşer.";
            return RedirectToAction(nameof(Details), new { id = muayeneId });
        }

        _db.ReceteKalemleri.Remove(kalem);
        await _db.SaveChangesAsync();
        await islem.CommitAsync();

        TempData["Mesaj"] = "İlaç reçeteden çıkarıldı.";
        return RedirectToAction(nameof(Details), null, new { id = muayeneId }, fragment: "recete");
    }

    public async Task<IActionResult> Create(int? hayvanId, int? randevuId)
    {
        var muayene = new Muayene { Tarih = DateTime.Now };

        if (randevuId != null)
        {
            var randevu = await _db.Randevular.AsNoTracking().FirstOrDefaultAsync(r => r.Id == randevuId);
            if (randevu == null) return NotFound();

            var mevcutMuayeneId = await _db.Muayeneler.AsNoTracking()
                .Where(m => m.RandevuId == randevuId)
                .Select(m => (int?)m.Id)
                .FirstOrDefaultAsync();
            if (mevcutMuayeneId != null)
            {
                TempData["Hata"] = "Bu randevu için muayene zaten kaydedilmiş.";
                return RedirectToAction(nameof(Details), new { id = mevcutMuayeneId });
            }

            if (randevu.Durum != RandevuDurumu.Planlandi)
            {
                TempData["Hata"] = $"Bu randevu için muayene başlatılamaz (durum: {randevu.Durum.GetDisplayName()}).";
                return RedirectToAction(nameof(Index));
            }

            muayene.HayvanId = randevu.HayvanId;
            muayene.VeterinerId = randevu.VeterinerId;
            muayene.RandevuId = randevu.Id;
            muayene.Sikayet = randevu.Neden;
        }
        else if (hayvanId != null && await _db.Hayvanlar.AnyAsync(h => h.Id == hayvanId))
        {
            muayene.HayvanId = hayvanId.Value;
        }

        await SecimListeleriniDoldurAsync(muayene, mevcutVeterinerId: null);
        return View(muayene);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("HayvanId,VeterinerId,RandevuId,Tarih,Sikayet,Teshis,Tedavi,Notlar,Ucret")] Muayene muayene)
    {
        await MuayeneDogrulaAsync(muayene, mevcutVeterinerId: null, yeniKayit: true);

        if (!ModelState.IsValid)
        {
            await SecimListeleriniDoldurAsync(muayene, mevcutVeterinerId: null);
            return View(muayene);
        }

        _db.Muayeneler.Add(muayene);
        if (muayene.RandevuId != null)
        {
            var randevu = await _db.Randevular.FirstAsync(r => r.Id == muayene.RandevuId);
            randevu.Durum = RandevuDurumu.Tamamlandi;
        }
        await _db.SaveChangesAsync();

        TempData["Mesaj"] = "Muayene kaydedildi. Şimdi reçeteye ilaç ekleyebilirsiniz.";
        return RedirectToAction(nameof(Details), new { id = muayene.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var muayene = await _db.Muayeneler.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (muayene == null) return NotFound();

        await SecimListeleriniDoldurAsync(muayene, muayene.VeterinerId);
        return View(muayene);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,HayvanId,VeterinerId,Tarih,Sikayet,Teshis,Tedavi,Notlar,Ucret")] Muayene muayene)
    {
        if (id != muayene.Id) return NotFound();

        await using var islem = await _db.Database.BeginTransactionAsync();
        var mevcut = await _db.Muayeneler
            .HesapIle()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mevcut == null) return NotFound();

        if (mevcut.RandevuId != null)
        {
            ModelState.Remove(nameof(Muayene.HayvanId));
            muayene.HayvanId = mevcut.HayvanId;
        }

        if (ModelState.GetFieldValidationState(nameof(Muayene.HayvanId)) != ModelValidationState.Invalid &&
            muayene.HayvanId != mevcut.HayvanId && mevcut.Odemeler.Any())
            ModelState.AddModelError(nameof(Muayene.HayvanId), "Ödemesi olan muayenenin hayvanı değiştirilemez.");

        await MuayeneDogrulaAsync(muayene, mevcut.VeterinerId, yeniKayit: false);

        if (ModelState.GetFieldValidationState(nameof(Muayene.Tarih)) != ModelValidationState.Invalid &&
            mevcut.Odemeler.Any(o => o.Tarih.Date < muayene.Tarih.Date))
            ModelState.AddModelError(nameof(Muayene.Tarih), "Muayene tarihi, alınmış ödemelerin tarihinden sonra olamaz.");

        if (ModelState.GetFieldValidationState(nameof(Muayene.Ucret)) != ModelValidationState.Invalid &&
            muayene.Ucret + mevcut.IlacToplami < mevcut.OdenenToplam)
            ModelState.AddModelError(
                nameof(Muayene.Ucret),
                $"Muayene toplamı ödenen tutarın ({mevcut.OdenenToplam.TL()}) altına düşemez.");

        if (!ModelState.IsValid)
        {
            muayene.RandevuId = mevcut.RandevuId;
            await SecimListeleriniDoldurAsync(muayene, mevcut.VeterinerId);
            return View(muayene);
        }

        mevcut.HayvanId = muayene.HayvanId;
        mevcut.VeterinerId = muayene.VeterinerId;
        mevcut.Tarih = muayene.Tarih;
        mevcut.Sikayet = muayene.Sikayet;
        mevcut.Teshis = muayene.Teshis;
        mevcut.Tedavi = muayene.Tedavi;
        mevcut.Notlar = muayene.Notlar;
        mevcut.Ucret = muayene.Ucret;
        await _db.SaveChangesAsync();
        await islem.CommitAsync();

        TempData["Mesaj"] = "Muayene güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var muayene = await _db.Muayeneler.AsNoTracking()
            .Include(m => m.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(m => m.Veteriner)
            .Include(m => m.Randevu)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (muayene == null) return NotFound();

        ViewData["KalemSayisi"] = await _db.ReceteKalemleri.CountAsync(k => k.MuayeneId == id);
        ViewData["OdemeSayisi"] = await _db.Odemeler.CountAsync(o => o.MuayeneId == id);
        return View(muayene);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await using var islem = await _db.Database.BeginTransactionAsync();
        var muayene = await _db.Muayeneler.Include(m => m.Randevu).FirstOrDefaultAsync(m => m.Id == id);
        if (muayene == null) return NotFound();

        if (await _db.Odemeler.AnyAsync(o => o.MuayeneId == id))
        {
            TempData["Hata"] = "Bu muayeneye bağlı ödeme olduğu için silinemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (muayene.Randevu != null && muayene.Randevu.Durum == RandevuDurumu.Tamamlandi)
            muayene.Randevu.Durum = RandevuDurumu.Planlandi;

        _db.Muayeneler.Remove(muayene);
        await _db.SaveChangesAsync();
        await islem.CommitAsync();

        TempData["Mesaj"] = "Muayene silindi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Recete(int? id)
    {
        if (id == null) return NotFound();
        var muayene = await _db.Muayeneler.AsNoTracking()
            .Include(m => m.Hayvan).ThenInclude(h => h!.Sahip)
            .Include(m => m.Veteriner)
            .Include(m => m.ReceteKalemleri.OrderBy(k => k.Id)).ThenInclude(k => k.Ilac)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (muayene == null) return NotFound();

        return View(muayene);
    }
}
