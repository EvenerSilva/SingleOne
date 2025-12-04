using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwUltimasRequisicaoNaoBYODMap : IEntityTypeConfiguration<VwUltimasRequisicaoNaoBYOD>
    {
        public void Configure(EntityTypeBuilder<VwUltimasRequisicaoNaoBYOD> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwUltimasRequisicaoNaoBYOD");

            entity.Property(e => e.RequisicaoId).HasColumnName("requisicaoid");
            entity.Property(e => e.Cliente).HasColumnName("cliente");
            entity.Property(e => e.UsuarioRequisicao).HasColumnName("usuariorequisicao");
            entity.Property(e => e.TecnicoResponsavel).HasColumnName("tecnicoresponsavel");
            entity.Property(e => e.RequisicaoStatus).HasColumnName("requisicaostatus");
            entity.Property(e => e.ColaboradorFinal).HasColumnName("colaboradorfinal");
            entity.Property(e => e.NomeColaboradorFinal).HasColumnName("nomecolaboradorfinal");
            entity.Property(e => e.DtSolicitacao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtsolicitacao");

            entity.Property(e => e.DtProcessamento)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtprocessamento");

            entity.Property(e => e.AssinaturaEletronica)
                .HasColumnName("assinaturaeletronica");

            entity.Property(e => e.DtAssinaturaEletronica)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtassinaturaeletronica");

            entity.Property(e => e.DtEnvioTermo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtenviotermo");

            entity.Property(e => e.HashRequisicao)
                .HasColumnName("hashrequisicao");

            entity.Property(e => e.RequisicaoItemId).HasColumnName("requisicaoitemid");
            entity.Property(e => e.Equipamento).HasColumnName("equipamento");
            entity.Property(e => e.LinhaTelefonica).HasColumnName("linhatelefonica");
            entity.Property(e => e.NumeroLinhaTelefonica).HasColumnName("numero");
            entity.Property(e => e.UsuarioEntrega).HasColumnName("usuarioentrega");
            entity.Property(e => e.UsuarioDevolucao).HasColumnName("usuariodevolucao");
            entity.Property(e => e.DtEntrega)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtentrega");

            entity.Property(e => e.DtDevolucao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtdevolucao");

            entity.Property(e => e.ObservacaoEntrega)
                .HasColumnName("observacaoentrega");

            entity.Property(e => e.DtProgramadaRetorno)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtprogramadaretorno");

            entity.Property(e => e.EquipamentoId).HasColumnName("equipamentoid");
            entity.Property(e => e.TipoAquisicao).HasColumnName("tipoaquisicao");
            entity.Property(e => e.EquipamentoStatus).HasColumnName("equipamentostatus");
            entity.Property(e => e.NumeroSerie).HasColumnName("numeroserie");
            entity.Property(e => e.Patrimonio).HasColumnName("patrimonio");
        }
    }
}
