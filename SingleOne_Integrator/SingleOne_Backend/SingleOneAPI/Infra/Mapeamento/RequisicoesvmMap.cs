using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class RequisicoesvmMap : IEntityTypeConfiguration<Requisicoesvm>
    {
        public void Configure(EntityTypeBuilder<Requisicoesvm> entity)
        {
            entity.HasNoKey();

            entity.ToView("requisicoesvm");

            entity.Property(e => e.Assinaturaeletronica).HasColumnName("assinaturaeletronica");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Colaboradorfinal)
                .HasMaxLength(300)
                .HasColumnName("colaboradorfinal");

            entity.Property(e => e.Colaboradorfinalid).HasColumnName("colaboradorfinalid");

            entity.Property(e => e.Dtassinaturaeletronica)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtassinaturaeletronica");

            entity.Property(e => e.Dtenviotermo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtenviotermo");

            entity.Property(e => e.Dtprocessamento)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtprocessamento");

            entity.Property(e => e.Dtsolicitacao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtsolicitacao");

            entity.Property(e => e.Equipamentospendentes).HasColumnName("equipamentospendentes");

            entity.Property(e => e.Hashrequisicao)
                .HasMaxLength(200)
                .HasColumnName("hashrequisicao");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Requisicaostatus)
                .HasMaxLength(100)
                .HasColumnName("requisicaostatus");

            entity.Property(e => e.Requisicaostatusid).HasColumnName("requisicaostatusid");

            entity.Property(e => e.Tecnicoresponsavel)
                .HasMaxLength(200)
                .HasColumnName("tecnicoresponsavel");

            entity.Property(e => e.Tecnicoresponsavelid).HasColumnName("tecnicoresponsavelid");

            entity.Property(e => e.Usuariorequisicao)
                .HasMaxLength(200)
                .HasColumnName("usuariorequisicao");

            entity.Property(e => e.Usuariorequisicaoid).HasColumnName("usuariorequisicaoid");
        }
    }
}
