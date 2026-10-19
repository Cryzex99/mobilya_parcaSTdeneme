using Microsoft.EntityFrameworkCore;
using mobilya_parcaST.Models;

namespace mobilya_parcaST.Data;

public class MobilyaDbContext : DbContext
{
    public MobilyaDbContext(DbContextOptions<MobilyaDbContext> options) : base(options)
    {
    }

    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<Urun> Urunler => Set<Urun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategori>().ToTable("Kategoriler");
        modelBuilder.Entity<Urun>().ToTable("Urunler");

        modelBuilder.Entity<Urun>()
            .Property(urun => urun.Fiyat)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Kategori>()
            .HasMany(kategori => kategori.Urunler)
            .WithOne(urun => urun.Kategori)
            .HasForeignKey(urun => urun.KategoriId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
