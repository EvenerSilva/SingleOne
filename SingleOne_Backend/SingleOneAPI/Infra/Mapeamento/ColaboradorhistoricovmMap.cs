using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ColaboradorhistoricovmMap : IEntityTypeConfiguration<Colaboradorhistoricovm>
    {
        public void Configure(EntityTypeBuilder<Colaboradorhistoricovm> entity)
        {
            entity.HasNoKey();

            entity.ToView("colaboradorhistoricovm");

            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .HasColumnName("cargo");

            entity.Property(e => e.Centrocustoantigoid).HasColumnName("centrocustoantigoid");

            entity.Property(e => e.Centrocustoatualid).HasColumnName("centrocustoatualid");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Codigoccantigo)
                .HasMaxLength(10)
                .HasColumnName("codigoccantigo");

            entity.Property(e => e.Codigoccatual)
                .HasMaxLength(10)
                .HasColumnName("codigoccatual");

            entity.Property(e => e.Cpf)
                .HasMaxLength(50)
                .HasColumnName("cpf");

            entity.Property(e => e.Dtatualizacao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtatualizacao");

            entity.Property(e => e.Dtatualizacaocentrocusto)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtatualizacaocentrocusto");

            entity.Property(e => e.Dtatualizacaoempresa)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtatualizacaoempresa");

            entity.Property(e => e.Dtatualizacaolocalidade)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtatualizacaolocalidade");

            entity.Property(e => e.Email)
                .HasMaxLength(300)
                .HasColumnName("email");

            entity.Property(e => e.Empresaantiga)
                .HasMaxLength(250)
                .HasColumnName("empresaantiga");

            entity.Property(e => e.Empresaantigaid).HasColumnName("empresaantigaid");

            entity.Property(e => e.Empresaatual)
                .HasMaxLength(250)
                .HasColumnName("empresaatual");

            entity.Property(e => e.Empresaatualid).HasColumnName("empresaatualid");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Localidadeantiga)
                .HasMaxLength(300)
                .HasColumnName("localidadeantiga");

            entity.Property(e => e.Localidadeantigaid).HasColumnName("localidadeantigaid");

            entity.Property(e => e.Localidadeatual)
                .HasMaxLength(300)
                .HasColumnName("localidadeatual");

            entity.Property(e => e.Localidadeatualid).HasColumnName("localidadeatualid");

            entity.Property(e => e.Matricula)
                .HasMaxLength(50)
                .HasColumnName("matricula");

            entity.Property(e => e.Nome)
                .HasMaxLength(300)
                .HasColumnName("nome");

            entity.Property(e => e.Nomeccantigo)
                .HasMaxLength(100)
                .HasColumnName("nomeccantigo");

            entity.Property(e => e.Nomeccatual)
                .HasMaxLength(100)
                .HasColumnName("nomeccatual");

            entity.Property(e => e.Situacao).HasColumnName("situacao");

            entity.Property(e => e.Situacaoantiga).HasColumnName("situacaoantiga");
        }
    }
}
