using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TemplateMap : IEntityTypeConfiguration<Template>
    {
        public void Configure(EntityTypeBuilder<Template> entity)
        {
            entity.ToTable("templates");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Versao).HasColumnName("versao");
            entity.Property(e => e.DataCriacao).HasColumnName("datacriacao");
            entity.Property(e => e.DataAlteracao).HasColumnName("dataalteracao");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Conteudo)
                .IsRequired()
                .HasColumnName("conteudo");

            entity.Property(e => e.Tipo).HasColumnName("tipo");

            entity.Property(e => e.Titulo)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("titulo");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Templates)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktemplatecliente");

            entity.HasOne(d => d.TipoNavigation)
                .WithMany(p => p.Templates)
                .HasForeignKey(d => d.Tipo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktemplatestipo");
        }
    }

}
