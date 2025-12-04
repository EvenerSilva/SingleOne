using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class RegrasTemplateMap : IEntityTypeConfiguration<RegrasTemplate>
    {
        public void Configure(EntityTypeBuilder<RegrasTemplate> entity)
        {
            entity.ToTable("regrastemplate");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TipoAquisicao).HasColumnName("tipoaquisicao");
            entity.Property(e => e.TipoTemplate).HasColumnName("tipotemplate");
            entity.HasOne(t => t.TemplatetipoNavigation)
            .WithMany(r => r.Regras)
            .HasForeignKey(f => f.TipoTemplate)
            .HasConstraintName("fkRegrasTemplateTipoTemplate");
        }
    }
}
