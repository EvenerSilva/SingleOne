using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class DescartecargoMap : IEntityTypeConfiguration<Descartecargo>
    {
        public void Configure(EntityTypeBuilder<Descartecargo> builder)
        {
            builder.ToTable("descartecargos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");
            
            builder.Property(e => e.Cliente).HasColumnName("cliente");

            builder.HasOne(e => e.ClienteNavigation)
                   .WithMany(c => c.Descartecargos)
                   .HasForeignKey(e => e.Cliente)
                   .OnDelete(DeleteBehavior.ClientSetNull)
                   .HasConstraintName("fkCargoCliente");

            builder.Property(e => e.Cargo).HasColumnName("cargo");

        }
    }
}
