using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Data;
using VeterinerKlinik.Helpers;
using VeterinerKlinik.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
{
    o.ModelBinderProviders.Insert(0, new OndalikModelBinderProvider());
    TurkceDogrulamaMesajlari.Uygula(o.ModelBindingMessageProvider);
    o.Filters.Add<YetkiFiltresi>();
});
builder.Services.AddScoped<YetkiFiltresi>();
builder.Services.AddScoped<IPasswordHasher<Kullanici>, PasswordHasher<Kullanici>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Hesap/Giris";
        o.LogoutPath = "/Hesap/Cikis";
        o.AccessDeniedPath = "/Hesap/ErisimEngellendi";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.Name = "VeterinerKlinik.Oturum";
        o.Events.OnValidatePrincipal = OturumDogrulayici.DogrulaAsync;
    });
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
var baglanti = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
if (!Path.IsPathRooted(baglanti.DataSource))
    baglanti.DataSource = Path.Combine(builder.Environment.ContentRootPath, baglanti.DataSource);

builder.Services.AddDbContext<KlinikDbContext>(options =>
    options.UseSqlite(baglanti.ToString(), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KlinikDbContext>();
    db.Database.Migrate();
    await KullaniciTohumu.OlusturAsync(scope.ServiceProvider);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
