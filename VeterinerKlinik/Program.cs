using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
// Göreli veritabanı yolunu proje klasörüne sabitle; uygulama hangi klasörden başlatılırsa başlatılsın aynı klinik.db kullanılır.
var baglanti = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
if (!Path.IsPathRooted(baglanti.DataSource))
    baglanti.DataSource = Path.Combine(builder.Environment.ContentRootPath, baglanti.DataSource);

builder.Services.AddDbContext<KlinikDbContext>(options =>
    options.UseSqlite(baglanti.ToString()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KlinikDbContext>();
    db.Database.Migrate();
}

var trKultur = new CultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(trKultur),
    SupportedCultures = new List<CultureInfo> { trKultur },
    SupportedUICultures = new List<CultureInfo> { trKultur }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
