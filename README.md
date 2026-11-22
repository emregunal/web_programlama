# Veteriner Kliniği Yönetim Sistemi

Üniversite ders projesi. Bir veteriner kliniğinin sahip, hayvan, randevu, muayene, reçete, aşı ve ödeme süreçlerini yöneten web uygulaması.

Teknolojiler: ASP.NET Core MVC (.NET 10, C#), Entity Framework Core 10, SQLite.

## Çalıştırma

```powershell
dotnet tool restore
dotnet run --project VeterinerKlinik
```

- İlk açılışta `Database.Migrate()` çalışır; `klinik.db` otomatik oluşur ve örnek veriler (3 veteriner, 6 ilaç, 4 aşı) yüklenir.
- Yeni migration eklemek için: `dotnet ef migrations add <Ad> --project VeterinerKlinik`
- `*.db` dosyaları git'e eklenmez.

## Klasör yapısı

```
veteriner-klinik/
├── VeterinerKlinik.slnx
├── dotnet-tools.json          # dotnet-ef yerel aracı
├── README.md
└── VeterinerKlinik/
    ├── Controllers/           # MVC controller'ları
    ├── Data/                  # KlinikDbContext, seed verisi
    ├── Migrations/            # EF Core migration'ları
    ├── Models/                # Varlıklar ve enum'lar
    ├── Views/                 # Razor görünümleri
    ├── wwwroot/               # Statik dosyalar
    ├── appsettings.json       # Bağlantı dizesi (DefaultConnection)
    └── Program.cs
```

## İş bölümü

| Kişi | Sıra | Görev |
|------|------|-------|
| A (Bekir) | 1 | İskelet + modeller + migration |
| A (Bekir) | 2 | Sahip CRUD + arama |
| A (Bekir) | 3 | Muayene + reçete/ilaç |
| A (Bekir) | 4 | Ödeme/fatura |
| B (Arkadaş) | 1 | Giriş/rol sistemi (Admin, Veteriner, Sekreter) |
| B (Arkadaş) | 2 | Hayvan CRUD + geçmiş |
| B (Arkadaş) | 3 | Randevu + takvim + çakışma kontrolü |
| B (Arkadaş) | 4 | Aşı takvimi + hatırlatma + ana sayfa paneli |

## Git akışı

- `main` dalına doğrudan push yapılmaz.
- Her iş için `feature/<konu>` dalı açılır (ör. `feature/sahip-crud`).
- Pull Request'i diğer kişi inceler ve merge eder.
- Küçük ve sık commit atılır.
- Migration eklemeden önce `main` dalı çekilir (`git pull origin main`); aksi halde migration çakışması olur.
