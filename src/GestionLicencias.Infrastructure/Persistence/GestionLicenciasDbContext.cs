using GestionLicencias.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionLicencias.Infrastructure.Persistence;

public class GestionLicenciasDbContext : DbContext
{
    public GestionLicenciasDbContext(DbContextOptions<GestionLicenciasDbContext> options)
        : base(options) { }

    public DbSet<TramiteLicencia> TramitesLicencia { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.NombreUsuario).HasMaxLength(100);
            entity.Property(u => u.NombreCompleto).HasMaxLength(200);
            entity.Property(u => u.Rol).HasMaxLength(50);
            entity.Property(u => u.RowVersion).IsRowVersion().IsConcurrencyToken();
            entity.HasIndex(u => u.NombreUsuario).IsUnique();
        });

        modelBuilder.Entity<BaseTramite>(entity =>
        {
            entity.ToTable("Tramites");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.RUT).HasMaxLength(20);
            entity.Property(t => t.Nombre).HasMaxLength(200);
            entity.Property(t => t.Email).HasMaxLength(200);
            entity.Property(t => t.Telefono).HasMaxLength(50);
            entity.Property(t => t.RowVersion).IsRowVersion().IsConcurrencyToken();
            entity.HasIndex(t => t.RUT);
            entity.HasDiscriminator(t => t.TipoModulo)
                  .HasValue<TramiteLicencia>("TramiteLicencia");
        });

        base.OnModelCreating(modelBuilder);
    }
}
