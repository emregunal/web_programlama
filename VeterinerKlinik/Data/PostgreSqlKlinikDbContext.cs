using Microsoft.EntityFrameworkCore;

namespace VeterinerKlinik.Data;

public sealed class PostgreSqlKlinikDbContext(DbContextOptions<PostgreSqlKlinikDbContext> options)
    : KlinikDbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Formlar ve randevu takvimi yerel saatle çalışır.
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
    }
}
