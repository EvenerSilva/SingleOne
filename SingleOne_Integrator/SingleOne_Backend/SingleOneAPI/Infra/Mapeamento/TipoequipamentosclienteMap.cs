using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TipoequipamentosclienteMap : IEntityTypeConfiguration<Tipoequipamentoscliente>
    {
        public void Configure(EntityTypeBuilder<Tipoequipamentoscliente> entity)
        {
            entity.ToTable("tipoequipamentosclientes");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Tipo).HasColumnName("tipo");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Tipoequipamentosclientes)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktipoeqpcliente");

            entity.HasOne(d => d.TipoNavigation)
                .WithMany(p => p.Tipoequipamentosclientes)
                .HasForeignKey(d => d.Tipo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktipoeqpclientetipo");
        }
    }

}
