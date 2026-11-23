using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Models;

namespace VeterinerKlinik.Helpers;

public static class KullaniciTohumu
{
    public static async Task OlusturAsync(IServiceProvider servisler, string? yoneticiParolasi)
    {
        var db = servisler.GetRequiredService<KlinikDbContext>();
        var postgres = db.Database.IsNpgsql();
        if (postgres && string.IsNullOrWhiteSpace(yoneticiParolasi))
            return;

        if (await db.Kullanicilar.AnyAsync())
            return;

        var hasher = servisler.GetRequiredService<IPasswordHasher<Kullanici>>();
        var yonetici = new Kullanici { KullaniciAdi = "admin", AdSoyad = "Sistem Yöneticisi", Rol = KullaniciRolu.Admin };
        List<(Kullanici Kullanici, string Parola)> kullanicilar = postgres
            ? [(yonetici, yoneticiParolasi!)]
            :
        [
            (yonetici, "Admin123!"),
            (new Kullanici { KullaniciAdi = "veteriner", AdSoyad = "Ayşe Yılmaz", Rol = KullaniciRolu.Veteriner, VeterinerId = 1 },
                "Veteriner123!"),
            (new Kullanici { KullaniciAdi = "sekreter", AdSoyad = "Zeynep Arslan", Rol = KullaniciRolu.Sekreter }, "Sekreter123!")
        ];

        foreach (var (kullanici, parola) in kullanicilar)
        {
            kullanici.Aktif = true;
            kullanici.KayitTarihi = DateTime.Now;
            kullanici.ParolaOzeti = hasher.HashPassword(kullanici, parola);
            db.Kullanicilar.Add(kullanici);
        }

        await db.SaveChangesAsync();
    }
}
