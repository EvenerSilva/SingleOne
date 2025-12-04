using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwexportacaoexcelMap : IEntityTypeConfiguration<Vwexportacaoexcel>
    {
        public void Configure(EntityTypeBuilder<Vwexportacaoexcel> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwexportacaoexcel");

            entity.Property(e => e.TipoAquisicao).HasColumnName("tipoaquisicao");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .HasColumnName("cargo");

            entity.Property(e => e.Centrocusto)
                .HasMaxLength(100)
                .HasColumnName("centrocusto");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Colaborador)
                .HasMaxLength(300)
                .HasColumnName("colaborador");

            entity.Property(e => e.Descricaobo).HasColumnName("descricaobo");

            entity.Property(e => e.Dtcadastro)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtcadastro");

            entity.Property(e => e.Empresa)
                .HasMaxLength(250)
                .HasColumnName("empresa");

            entity.Property(e => e.Equipamentostatus)
                .HasMaxLength(100)
                .HasColumnName("equipamentostatus");

            entity.Property(e => e.Equipamentostatusid).HasColumnName("equipamentostatusid");

            entity.Property(e => e.Fabricante)
                .HasMaxLength(200)
                .HasColumnName("fabricante");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Localizacao)
                .HasMaxLength(300)
                .HasColumnName("localizacao");

            entity.Property(e => e.Modelo)
                .HasMaxLength(200)
                .HasColumnName("modelo");

            entity.Property(e => e.Notafiscal).HasColumnName("notafiscal");

            entity.Property(e => e.Numeroserie)
                .HasMaxLength(100)
                .HasColumnName("numeroserie");

            entity.Property(e => e.Patrimonio)
                .HasMaxLength(100)
                .HasColumnName("patrimonio");

            entity.Property(e => e.Possuibo).HasColumnName("possuibo");

            entity.Property(e => e.Tipoequipamento)
                .HasMaxLength(200)
                .HasColumnName("tipoequipamento");

            entity.Property(e => e.Usuariocadastro)
                .HasMaxLength(200)
                .HasColumnName("usuariocadastro");
        }
    }
}
