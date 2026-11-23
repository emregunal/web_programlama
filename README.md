# Veteriner Kliniği Yönetim Sistemi

Üniversite ders projesi. Bir veteriner kliniğinin sahip, hayvan, randevu, muayene, reçete, aşı ve ödeme süreçlerini yöneten web uygulaması.

Teknolojiler: ASP.NET Core MVC (.NET 10, C#), Entity Framework Core 10, SQLite ve PostgreSQL.

## Çalıştırma

```powershell
dotnet tool restore
dotnet run --project VeterinerKlinik
```

- İlk açılışta `Database.Migrate()` çalışır; `klinik.db` otomatik oluşur ve örnek veriler (3 veteriner, 6 ilaç, 4 aşı) yüklenir.
- SQLite migration'ı eklemek için `dotnet ef migrations add <Ad> --project VeterinerKlinik --context KlinikDbContext -- --DATABASE_URL=` komutunu kullanın.
- `*.db` dosyaları git'e eklenmez.

### Neon PostgreSQL kurulumu

1. Neon'da **Connect** ekranını açıp hedef veritabanının bağlantı URL'sini kopyalayın. Uygulama havuzlu Neon URL'sini doğrudan uç noktaya dönüştürür.
2. Proje kökündeki `.env` dosyasına `DATABASE_URL=` anahtarıyla bağlantı URL'sini ekleyin. Dosya yoksa `.env.example` üzerinden oluşturabilirsiniz.
3. İlk `admin` hesabını açmak için `.env` dosyasında `KLINIK_ADMIN_PASSWORD` değerini belirleyin.
4. `dotnet run --project VeterinerKlinik` komutuyla uygulamayı başlatın. EF Core PostgreSQL migration'larını uygular. `DATABASE_URL` boşsa uygulama SQLite kullanır.

PostgreSQL migration'ları `VeterinerKlinik/Migrations/PostgreSql` altında tutulur. Yeni bir migration eklemek için Neon yapılandırması etkin durumdayken `dotnet ef migrations add <Ad> --project VeterinerKlinik --context PostgreSqlKlinikDbContext --output-dir Migrations/PostgreSql` komutunu kullanın.

## Giriş bilgileri (demo)

İlk açılışta hiç kullanıcı yoksa üç demo hesap oluşturulur. Gerçek ortamda parolalar değiştirilmelidir.

| Kullanıcı adı | Parola | Rol |
|---|---|---|
| `admin` | `Admin123!` | Yönetici |
| `veteriner` | `Veteriner123!` | Veteriner (Ayşe Yılmaz) |
| `sekreter` | `Sekreter123!` | Sekreter |

### Rol yetkileri

Okuma: listeleme ve görüntüleme (Index, Details, Takvim, Recete, Fatura). Yazma: diğer tüm işlemler (ekleme, düzenleme, silme).

| Bölüm | Okuma | Yazma |
|---|---|---|
| Ana Sayfa | Hepsi | Hepsi |
| Sahipler | Hepsi | Yönetici, Sekreter |
| Hayvanlar | Hepsi | Yönetici, Sekreter, Veteriner |
| Randevular | Hepsi | Yönetici, Sekreter |
| Muayeneler | Hepsi | Yönetici, Veteriner |
| İlaçlar | Hepsi | Yönetici, Veteriner |
| Aşılar | Hepsi | Yönetici, Veteriner |
| Ödemeler | Yönetici, Sekreter | Yönetici, Sekreter |
| Kullanıcılar | Yönetici | Yönetici |

Yetki tablosu tek yerde tutulur: `VeterinerKlinik/Helpers/YetkiKurallari.cs`. Tabloda olmayan bir bölüme erişim kapalıdır.

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
