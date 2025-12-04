using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class RequisicoMap : IEntityTypeConfiguration<Requisico>
    {
        public void Configure(EntityTypeBuilder<Requisico> entity)
        {
            entity.ToTable("requisicoes");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Assinaturaeletronica).HasColumnName("assinaturaeletronica");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Colaboradorfinal).HasColumnName("colaboradorfinal");

            entity.Property(e => e.Dtassinaturaeletronica)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtassinaturaeletronica");

            entity.Property(e => e.ConteudoTemplateAssinado)
                .HasColumnName("conteudo_template_assinado");

            entity.Property(e => e.TipoTermoAssinado)
                .HasColumnName("tipo_termo_assinado");

            entity.Property(e => e.VersaoTemplateAssinado)
                .HasColumnName("versao_template_assinado");

            entity.Property(e => e.Dtenviotermo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtenviotermo");

            entity.Property(e => e.Dtprocessamento)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtprocessamento");

            entity.Property(e => e.Dtsolicitacao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtsolicitacao");

            entity.Property(e => e.Hashrequisicao)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("hashrequisicao");

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.Property(e => e.Requisicaostatus).HasColumnName("requisicaostatus");

            entity.Property(e => e.Tecnicoresponsavel).HasColumnName("tecnicoresponsavel");

            entity.Property(e => e.Usuariorequisicao).HasColumnName("usuariorequisicao");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Requisicos)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkrequisicaocliente");

            entity.HasOne(d => d.RequisicaostatusNavigation)
                .WithMany(p => p.Requisicos)
                .HasForeignKey(d => d.Requisicaostatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkrequisicaostatus");

            entity.HasOne(d => d.TecnicoresponsavelNavigation)
                .WithMany(p => p.RequisicoTecnicoresponsavelNavigations)
                .HasForeignKey(d => d.Tecnicoresponsavel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkrequisicaotecnico");

            entity.HasOne(d => d.UsuariorequisicaoNavigation)
                .WithMany(p => p.RequisicoUsuariorequisicaoNavigations)
                .HasForeignKey(d => d.Usuariorequisicao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkrequisicaousuario");
        }
    }
}
