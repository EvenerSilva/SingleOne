using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class LocalidadeMap : IEntityTypeConfiguration<Localidade>
    {
        public void Configure(EntityTypeBuilder<Localidade> entity)
        {
            entity.ToTable("localidades");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Descricao)
                .HasMaxLength(300)
                .HasColumnName("descricao")
                .HasDefaultValue(""); // Valor padrão para evitar null

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.Property(e => e.Cidade)
                .HasMaxLength(100)
                .HasColumnName("cidade");

            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasColumnName("estado");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Localidades)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fklocalidadecliente");
        }
    }
}
