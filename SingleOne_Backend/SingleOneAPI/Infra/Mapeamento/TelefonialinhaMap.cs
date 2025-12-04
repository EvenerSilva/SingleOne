using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TelefonialinhaMap : IEntityTypeConfiguration<Telefonialinha>
    {
        public void Configure(EntityTypeBuilder<Telefonialinha> entity)
        {
            entity.ToTable("telefonialinhas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Emuso).HasColumnName("emuso");
            entity.Property(e => e.Iccid)
                .HasMaxLength(500)
                .HasColumnName("iccid");
            entity.Property(e => e.Numero).HasColumnName("numero");
            entity.Property(e => e.Plano).HasColumnName("plano");

            entity.HasOne(d => d.PlanoNavigation)
                .WithMany(p => p.Telefonialinhas)
                .HasForeignKey(d => d.Plano)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fklinhaplano");
        }
    }
}
