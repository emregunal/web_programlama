using Microsoft.AspNetCore.WebUtilities;
using Npgsql;

namespace VeterinerKlinik.Data;

internal static class PostgreSqlBaglantisi
{
    public static string Olustur(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("postgresql" or "postgres") ||
            string.IsNullOrEmpty(uri.Host) || uri.AbsolutePath.Length <= 1)
        {
            throw new InvalidOperationException("DATABASE_URL geçerli bir PostgreSQL bağlantı URL'si olmalı.");
        }

        var kimlik = uri.UserInfo.Split(':', 2);
        if (kimlik.Length != 2 || kimlik[0].Length == 0 || kimlik[1].Length == 0)
            throw new InvalidOperationException("DATABASE_URL kullanıcı adı ve parola içermeli.");

        var sunucu = uri.IdnHost;
        const string havuzSoneki = "-pooler";
        var adres = sunucu.Split('.', 2);
        if (sunucu.EndsWith(".neon.tech", StringComparison.OrdinalIgnoreCase) &&
            adres[0].EndsWith(havuzSoneki, StringComparison.OrdinalIgnoreCase))
        {
            // EF Core migration'ları için Neon'un doğrudan uç noktasını kullan.
            sunucu = adres[0][..^havuzSoneki.Length] + "." + adres[1];
        }

        var parametreler = QueryHelpers.ParseQuery(uri.Query);
        var baglanti = new NpgsqlConnectionStringBuilder
        {
            Host = sunucu,
            Port = uri.Port < 0 ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath[1..]),
            Username = Uri.UnescapeDataString(kimlik[0]),
            Password = Uri.UnescapeDataString(kimlik[1]),
            SslMode = parametreler.GetValueOrDefault("sslmode").ToString().ToLowerInvariant() switch
            {
                "disable" => SslMode.Disable,
                "allow" => SslMode.Allow,
                "prefer" => SslMode.Prefer,
                "require" => SslMode.Require,
                "verify-ca" => SslMode.VerifyCA,
                "" or "verify-full" => SslMode.VerifyFull,
                _ => throw new InvalidOperationException("DATABASE_URL içindeki sslmode değeri geçersiz.")
            },
            ChannelBinding = parametreler.GetValueOrDefault("channel_binding").ToString().ToLowerInvariant() switch
            {
                "disable" => ChannelBinding.Disable,
                "" or "prefer" => ChannelBinding.Prefer,
                "require" => ChannelBinding.Require,
                _ => throw new InvalidOperationException("DATABASE_URL içindeki channel_binding değeri geçersiz.")
            }
        };

        return baglanti.ConnectionString;
    }
}
