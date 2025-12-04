using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TipoaquisicaoMap : IEntityTypeConfiguration<Tipoaquisicao>
    {
        public void Configure(EntityTypeBuilder<Tipoaquisicao> entity)
        {
            entity.ToTable("tipoaquisicao");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("nome");
        }
    }
}
