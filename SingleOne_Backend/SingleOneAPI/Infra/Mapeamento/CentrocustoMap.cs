using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class CentrocustoMap : IEntityTypeConfiguration<Centrocusto>
    {
        public void Configure(EntityTypeBuilder<Centrocusto> entity)
        {
            entity.ToTable("centrocusto");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("codigo");

            entity.Property(e => e.Empresa).HasColumnName("empresa");

            entity.Property(e => e.FilialId).HasColumnName("filial_id");

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("nome");

            entity.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.EmpresaNavigation)
                .WithMany(p => p.Centrocustos)
                .HasForeignKey(d => d.Empresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkccempresa");

            // Removendo o relacionamento complexo com Filial para evitar conflitos
            // Apenas mapeando a propriedade FilialId para a coluna filial_id
        }
    }
}
