using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class RequisicaoequipamentosvmMap : IEntityTypeConfiguration<Requisicaoequipamentosvm>
    {
        public void Configure(EntityTypeBuilder<Requisicaoequipamentosvm> entity)
        {
            entity.HasNoKey();

            entity.ToView("requisicaoequipamentosvm");

            entity.Property(e => e.Dtdevolucao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtdevolucao");

            entity.Property(e => e.Dtentrega)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtentrega");

            entity.Property(e => e.Dtprogramadaretorno).HasColumnName("dtprogramadaretorno");

            entity.Property(e => e.Equipamento).HasColumnName("equipamento");

            entity.Property(e => e.Equipamentoid).HasColumnName("equipamentoid");

            entity.Property(e => e.Equipamentostatus).HasColumnName("equipamentostatus");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Linhaid).HasColumnName("linhaid");
            entity.Property(e => e.TipoAquisicao).HasColumnName("tipoaquisicao");

            entity.Property(e => e.Numero).HasColumnName("numero");

            entity.Property(e => e.Numeroserie)
                .HasColumnType("character varying")
                .HasColumnName("numeroserie");

            entity.Property(e => e.Observacaoentrega)
                .HasMaxLength(500)
                .HasColumnName("observacaoentrega");

            entity.Property(e => e.Patrimonio)
                .HasMaxLength(100)
                .HasColumnName("patrimonio");

            entity.Property(e => e.Requisicao).HasColumnName("requisicao");

            entity.Property(e => e.Usuariodevolucao)
                .HasMaxLength(200)
                .HasColumnName("usuariodevolucao");

            entity.Property(e => e.Usuariodevolucaoid).HasColumnName("usuariodevolucaoid");

            entity.Property(e => e.Usuarioentrega)
                .HasMaxLength(200)
                .HasColumnName("usuarioentrega");

            entity.Property(e => e.Usuarioentregaid).HasColumnName("usuarioentregaid");
        }
    }

}
