using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TemplatetipoMap : IEntityTypeConfiguration<Templatetipo>
    {
        public void Configure(EntityTypeBuilder<Templatetipo> entity)
        {
            entity.ToTable("templatetipos");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Descricao)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("descricao");
        }
    }
}
