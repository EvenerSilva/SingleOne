using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwcargoMap : IEntityTypeConfiguration<Vwcargo>
    {
        public void Configure(EntityTypeBuilder<Vwcargo> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwcargos");

            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .HasColumnName("cargo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");
        }
    }
}
