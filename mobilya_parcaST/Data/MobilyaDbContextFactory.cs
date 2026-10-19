using Microsoft.EntityFrameworkCore.Design;

namespace mobilya_parcaST.Data;

public class MobilyaDbContextFactory : IDesignTimeDbContextFactory<MobilyaDbContext>
{
    public MobilyaDbContext CreateDbContext(string[] args)
    {
        return new MobilyaDbContext(VeritabaniYapilandirmasi.DbContextSecenekleriOlustur());
    }
}
