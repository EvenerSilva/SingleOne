using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ModeloMap : IEntityTypeConfiguration<Modelo>
    {
        public void Configure(EntityTypeBuilder<Modelo> entity)
        {
            entity.ToTable("modelos");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Descricao)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("descricao");

            entity.Property(e => e.Fabricante).HasColumnName("fabricante");

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Modelos)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkmodeloscliente");

            entity.HasOne(d => d.FabricanteNavigation)
                .WithMany(p => p.Modelos)
                .HasForeignKey(d => d.Fabricante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkmodelosfabricantes");
        }
    }
}
