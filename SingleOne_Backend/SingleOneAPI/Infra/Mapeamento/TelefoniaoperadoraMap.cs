using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TelefoniaoperadoraMap : IEntityTypeConfiguration<Telefoniaoperadora>
    {
        public void Configure(EntityTypeBuilder<Telefoniaoperadora> entity)
        {
            entity.ToTable("telefoniaoperadoras");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("nome");
        }
    }
}
