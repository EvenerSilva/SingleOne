using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class RequisicaoItemCompartilhadoMap : IEntityTypeConfiguration<RequisicaoItemCompartilhado>
    {
        public void Configure(EntityTypeBuilder<RequisicaoItemCompartilhado> builder)
        {
            builder.ToTable("requisicoes_itens_compartilhados");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.RequisicaoItemId).HasColumnName("requisicao_item_id").IsRequired();
            builder.Property(e => e.ColaboradorId).HasColumnName("colaborador_id").IsRequired();
            builder.Property(e => e.TipoAcesso).HasColumnName("tipo_acesso").HasMaxLength(50).IsRequired();
            builder.Property(e => e.DataInicio).HasColumnName("data_inicio").IsRequired();
            builder.Property(e => e.DataFim).HasColumnName("data_fim");
            builder.Property(e => e.Observacao).HasColumnName("observacao");
            builder.Property(e => e.Ativo).HasColumnName("ativo").IsRequired();
            builder.Property(e => e.CriadoPor).HasColumnName("criado_por").IsRequired();
            builder.Property(e => e.CriadoEm).HasColumnName("criado_em").IsRequired();

            builder.HasOne(e => e.RequisicaoItem)
                .WithMany()
                .HasForeignKey(e => e.RequisicaoItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Colaborador)
                .WithMany()
                .HasForeignKey(e => e.ColaboradorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.CriadoPorUsuario)
                .WithMany()
                .HasForeignKey(e => e.CriadoPor)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}


