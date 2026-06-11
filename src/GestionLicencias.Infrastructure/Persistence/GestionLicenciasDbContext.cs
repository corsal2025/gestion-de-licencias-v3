using GestionLicencias.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionLicencias.Infrastructure.Persistence;

public class GestionLicenciasDbContext : DbContext
{
    public GestionLicenciasDbContext(DbContextOptions<GestionLicenciasDbContext> options)
        : base(options) { }

    public DbSet<TramiteLicencia> TramitesLicencia { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseTramite>(entity =>
        {
            entity.ToTable("Tramites");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.RUT).HasMaxLength(20);
            entity.Property(t => t.Nombre).HasMaxLength(200);
            entity.Property(t => t.Email).HasMaxLength(200);
            entity.Property(t => t.Telefono).HasMaxLength(50);
            entity.HasIndex(t => t.RUT);
            entity.HasDiscriminator(t => t.TipoModulo)
                  .HasValue<TramiteLicencia>("TramiteLicencia");
        });

        base.OnModelCreating(modelBuilder);
    }
}
