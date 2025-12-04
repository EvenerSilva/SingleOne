using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TipoequipamentoMap : IEntityTypeConfiguration<Tipoequipamento>
    {
        public void Configure(EntityTypeBuilder<Tipoequipamento> entity)
        {
            entity.ToTable("tipoequipamentos");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Descricao)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("descricao");

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");

            entity.Property(e => e.TransitoLivre)
                .HasColumnName("transitolivre")
                .HasDefaultValue(false);
        }
    }
}
