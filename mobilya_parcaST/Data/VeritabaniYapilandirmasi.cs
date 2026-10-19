using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace mobilya_parcaST.Data;

public static class VeritabaniYapilandirmasi
{
    // Hard-coded ServerVersion (AutoDetect Migration design-time'da bağlantı gerektirir)
    // XAMPP MariaDB 10.4 ile uyumlu; production'da gerekirse değiştir
    private static readonly ServerVersion MySqlSurumu = new MySqlServerVersion(new Version(10, 4, 0));

    public static string BaglantiDizesiOlustur()
    {
        IConfiguration yapilandirma = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var baglanti = new MySqlConnectionStringBuilder
        {
            Server = yapilandirma["Database:Server"] ?? "127.0.0.1",
            Port = uint.TryParse(yapilandirma["Database:Port"], out uint port) ? port : 3306,
            Database = yapilandirma["Database:Name"] ?? "MobilyaStokDb",
            UserID = yapilandirma["Database:User"] ?? "root",
            Password = yapilandirma["Database:Password"] ?? string.Empty
        };

        return baglanti.ConnectionString;
    }

    public static DbContextOptions<MobilyaDbContext> DbContextSecenekleriOlustur()
    {
        string baglantiDizesi = BaglantiDizesiOlustur();
        var secenekler = new DbContextOptionsBuilder<MobilyaDbContext>();
        secenekler.UseMySql(baglantiDizesi, MySqlSurumu);
        return secenekler.Options;
    }
}
