using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Models;

namespace VeterinerKlinik.Data;

public class KlinikDbContext : DbContext
{
    public KlinikDbContext(DbContextOptions<KlinikDbContext> options) : this((DbContextOptions)options) { }

    protected KlinikDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Sahip> Sahipler => Set<Sahip>();
    public DbSet<Hayvan> Hayvanlar => Set<Hayvan>();
    public DbSet<Veteriner> Veterinerler => Set<Veteriner>();
    public DbSet<Randevu> Randevular => Set<Randevu>();
    public DbSet<Muayene> Muayeneler => Set<Muayene>();
    public DbSet<Ilac> Ilaclar => Set<Ilac>();
    public DbSet<ReceteKalemi> ReceteKalemleri => Set<ReceteKalemi>();
    public DbSet<Asi> Asilar => Set<Asi>();
    public DbSet<AsiKaydi> AsiKayitlari => Set<AsiKaydi>();
    public DbSet<Odeme> Odemeler => Set<Odeme>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Hayvan>()
            .HasOne(h => h.Sahip).WithMany(s => s.Hayvanlar)
            .HasForeignKey(h => h.SahipId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Randevu>()
            .HasOne(r => r.Hayvan).WithMany(h => h.Randevular)
            .HasForeignKey(r => r.HayvanId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Randevu>()
            .HasOne(r => r.Veteriner).WithMany()
            .HasForeignKey(r => r.VeterinerId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Muayene>()
            .HasOne(m => m.Hayvan).WithMany(h => h.Muayeneler)
            .HasForeignKey(m => m.HayvanId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Muayene>()
            .HasOne(m => m.Veteriner).WithMany()
            .HasForeignKey(m => m.VeterinerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Muayene>()
            .HasOne(m => m.Randevu).WithMany()
            .HasForeignKey(m => m.RandevuId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Muayene>()
            .HasIndex(m => m.RandevuId).IsUnique();

        b.Entity<ReceteKalemi>()
            .HasOne(r => r.Muayene).WithMany(m => m.ReceteKalemleri)
            .HasForeignKey(r => r.MuayeneId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ReceteKalemi>()
            .HasOne(r => r.Ilac).WithMany()
            .HasForeignKey(r => r.IlacId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ReceteKalemi>()
            .HasIndex(r => new { r.MuayeneId, r.IlacId }).IsUnique();

        b.Entity<AsiKaydi>()
            .HasOne(a => a.Hayvan).WithMany(h => h.AsiKayitlari)
            .HasForeignKey(a => a.HayvanId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AsiKaydi>()
            .HasOne(a => a.Asi).WithMany()
            .HasForeignKey(a => a.AsiId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AsiKaydi>()
            .HasOne(a => a.Veteriner).WithMany()
            .HasForeignKey(a => a.VeterinerId).OnDelete(DeleteBehavior.SetNull);

        b.Entity<Odeme>()
            .HasOne(o => o.Sahip).WithMany(s => s.Odemeler)
            .HasForeignKey(o => o.SahipId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Odeme>()
            .HasOne(o => o.Muayene).WithMany(m => m.Odemeler)
            .HasForeignKey(o => o.MuayeneId).OnDelete(DeleteBehavior.SetNull);

        b.Entity<Kullanici>()
            .HasIndex(k => k.KullaniciAdi).IsUnique();
        b.Entity<Kullanici>()
            .HasOne(k => k.Veteriner).WithMany()
            .HasForeignKey(k => k.VeterinerId).OnDelete(DeleteBehavior.SetNull);

        b.Entity<Veteriner>().HasData(
            new Veteriner { Id = 1, Ad = "Ayşe", Soyad = "Yılmaz", Uzmanlik = "Genel Veterinerlik", Telefon = "0532 000 00 01", Aktif = true },
            new Veteriner { Id = 2, Ad = "Mehmet", Soyad = "Demir", Uzmanlik = "Cerrahi", Telefon = "0532 000 00 02", Aktif = true },
            new Veteriner { Id = 3, Ad = "Elif", Soyad = "Kaya", Uzmanlik = "Egzotik Hayvanlar", Telefon = "0532 000 00 03", Aktif = true });

        b.Entity<Ilac>().HasData(
            new Ilac { Id = 1, Ad = "Amoksisilin", Form = "Tablet", Birim = "Tablet", BirimFiyat = 12.50m },
            new Ilac { Id = 2, Ad = "Meloksikam", Form = "Şurup", Birim = "ml", BirimFiyat = 8.75m },
            new Ilac { Id = 3, Ad = "Ivermektin", Form = "Enjeksiyon", Birim = "ml", BirimFiyat = 25.00m },
            new Ilac { Id = 4, Ad = "Prazikuantel", Form = "Tablet", Birim = "Tablet", BirimFiyat = 15.00m },
            new Ilac { Id = 5, Ad = "Serum Fizyolojik", Form = "Enjeksiyon", Birim = "Flakon", BirimFiyat = 30.00m },
            new Ilac { Id = 6, Ad = "Probiyotik", Form = "Şurup", Birim = "ml", BirimFiyat = 18.00m });

        b.Entity<Asi>().HasData(
            new Asi { Id = 1, Ad = "Kuduz", TekrarAraligiGun = 365 },
            new Asi { Id = 2, Ad = "Karma", TekrarAraligiGun = 365 },
            new Asi { Id = 3, Ad = "İç parazit", TekrarAraligiGun = 90 },
            new Asi { Id = 4, Ad = "Dış parazit", TekrarAraligiGun = 30 });
    }
}
