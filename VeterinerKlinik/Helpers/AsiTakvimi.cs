using VeterinerKlinik.Models;
using VeterinerKlinik.ViewModels;

namespace VeterinerKlinik.Helpers;

public static class AsiTakvimi
{
    public const int YaklasanGun = 30;

    public static AsiDurumu DurumHesapla(DateTime? sonrakiTarih, bool yenilendi, DateTime bugun)
    {
        if (yenilendi) return AsiDurumu.Yenilendi;
        if (sonrakiTarih == null) return AsiDurumu.TakipYok;
        if (sonrakiTarih.Value.Date < bugun) return AsiDurumu.Gecikmis;
        if (sonrakiTarih.Value.Date <= bugun.AddDays(YaklasanGun)) return AsiDurumu.Yaklasan;
        return AsiDurumu.Guncel;
    }

    public static List<AsiKaydiListeOgesi> Listele(IEnumerable<AsiKaydi> kayitlar)
    {
        var bugun = DateTime.Today;
        var liste = kayitlar.ToList();

        var sonKayitlar = liste
            .GroupBy(k => new { k.HayvanId, k.AsiId })
            .Select(g => g.OrderByDescending(k => k.UygulamaTarihi).ThenByDescending(k => k.Id).First().Id)
            .ToHashSet();

        return liste
            .Select(k => new AsiKaydiListeOgesi
            {
                Id = k.Id,
                HayvanId = k.HayvanId,
                HayvanAdi = k.Hayvan!.Ad,
                HayvanTuru = k.Hayvan.Tur,
                SahipId = k.Hayvan.SahipId,
                SahipAdSoyad = k.Hayvan.Sahip!.AdSoyad,
                SahipTelefon = k.Hayvan.Sahip.Telefon,
                AsiAdi = k.Asi!.Ad,
                VeterinerAdSoyad = k.Veteriner?.AdSoyad,
                UygulamaTarihi = k.UygulamaTarihi,
                SonrakiTarih = k.SonrakiTarih,
                Durum = DurumHesapla(k.SonrakiTarih, !sonKayitlar.Contains(k.Id), bugun)
            })
            .OrderBy(k => k.Durum)
            .ThenBy(k => k.SonrakiTarih)
            .ThenByDescending(k => k.UygulamaTarihi)
            .ToList();
    }
}
