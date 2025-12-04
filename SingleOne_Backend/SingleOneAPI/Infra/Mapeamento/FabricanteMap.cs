using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class FabricanteMap : IEntityTypeConfiguration<Fabricante>
    {
        public void Configure(EntityTypeBuilder<Fabricante> entity)
        {
            entity.ToTable("fabricantes");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Descricao)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("descricao");

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.Property(e => e.Tipoequipamento).HasColumnName("tipoequipamento");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Fabricantes)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkfabricantecliente");

            entity.HasOne(d => d.TipoequipamentoNavigation)
                .WithMany(p => p.Fabricantes)
                .HasForeignKey(d => d.Tipoequipamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkfabricantetipoeqp");
        }
    }
}
