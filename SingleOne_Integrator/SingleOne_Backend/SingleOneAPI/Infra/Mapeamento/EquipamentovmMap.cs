using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EquipamentovmMap : IEntityTypeConfiguration<Equipamentovm>
    {
        public void Configure(EntityTypeBuilder<Equipamentovm> entity)
        {
            entity.HasNoKey();

            entity.ToView("equipamentovm");

            entity.Property(e => e.TipoAquisicao).HasColumnName("tipoaquisicao");
            entity.Property(e => e.TipoAquisicaoNome).HasColumnName("TipoAquisicao");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Centrocusto)
                .HasMaxLength(100)
                .HasColumnName("centrocusto");

            entity.Property(e => e.Centrocustoid).HasColumnName("centrocustoid");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Colaboradorid).HasColumnName("colaboradorid");

            entity.Property(e => e.Colaboradornome)
                .HasMaxLength(300)
                .HasColumnName("colaboradornome");

            entity.Property(e => e.Descricaobo).HasColumnName("descricaobo");

            entity.Property(e => e.Dtcadastro)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtcadastro");

            entity.Property(e => e.Dtlimitegarantia).HasColumnName("dtlimitegarantia");

            entity.Property(e => e.Empresa)
                .HasMaxLength(250)
                .HasColumnName("empresa");

            entity.Property(e => e.Empresaid).HasColumnName("empresaid");

            entity.Property(e => e.Equipamentostatus)
                .HasMaxLength(100)
                .HasColumnName("equipamentostatus");

            entity.Property(e => e.Equipamentostatusid).HasColumnName("equipamentostatusid");

            entity.Property(e => e.Fabricante)
                .HasMaxLength(200)
                .HasColumnName("fabricante");

            entity.Property(e => e.Fabricanteid).HasColumnName("fabricanteid");

            entity.Property(e => e.Fornecedor).HasColumnName("fornecedor");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Localizacao)
                .HasMaxLength(300)
                .HasColumnName("localizacao");

            entity.Property(e => e.Localizacaoid).HasColumnName("localizacaoid");

            entity.Property(e => e.Modelo)
                .HasMaxLength(200)
                .HasColumnName("modelo");

            entity.Property(e => e.Modeloid).HasColumnName("modeloid");

            entity.Property(e => e.Notafiscalid).HasColumnName("notafiscalid");

            entity.Property(e => e.Numeroserie)
                .HasMaxLength(100)
                .HasColumnName("numeroserie");

            entity.Property(e => e.Patrimonio)
                .HasColumnType("character varying")
                .HasColumnName("patrimonio");

            entity.Property(e => e.Possuibo).HasColumnName("possuibo");

            entity.Property(e => e.Requisicaoid).HasColumnName("requisicaoid");

            entity.Property(e => e.Tipoequipamento)
                .HasMaxLength(200)
                .HasColumnName("tipoequipamento");

            entity.Property(e => e.Tipoequipamentoid).HasColumnName("tipoequipamentoid");

            entity.Property(e => e.Usuario)
                .HasMaxLength(200)
                .HasColumnName("usuario");

            entity.Property(e => e.Usuarioid).HasColumnName("usuarioid");
            
            entity.Property(e => e.Contratoid).HasColumnName("contratoid");
            entity.Property(e => e.Contrato).HasColumnName("contrato");
        }
    }
}
