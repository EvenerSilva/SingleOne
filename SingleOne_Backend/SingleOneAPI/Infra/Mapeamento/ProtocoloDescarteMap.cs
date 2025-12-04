using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    /// <summary>
    /// Mapeamento Entity Framework para ProtocoloDescarte
    /// </summary>
    public class ProtocoloDescarteMap : IEntityTypeConfiguration<ProtocoloDescarte>
    {
        public void Configure(EntityTypeBuilder<ProtocoloDescarte> builder)
        {
            builder.ToTable("protocolos_descarte");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Protocolo)
                .HasColumnName("protocolo")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.Cliente)
                .HasColumnName("cliente")
                .IsRequired();

            builder.Property(e => e.TipoDescarte)
                .HasColumnName("tipo_descarte")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.MotivoDescarte)
                .HasColumnName("motivo_descarte");

            builder.Property(e => e.DestinoFinal)
                .HasColumnName("destino_final")
                .HasMaxLength(500);

            builder.Property(e => e.ResponsavelProtocolo)
                .HasColumnName("responsavel_protocolo")
                .IsRequired();

            builder.Property(e => e.DataCriacao)
                .HasColumnName("data_criacao")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.DataConclusao)
                .HasColumnName("data_conclusao");

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(30)
                .HasDefaultValue("EM_ANDAMENTO")
                .IsRequired();

            builder.Property(e => e.ValorTotalEstimado)
                .HasColumnName("valor_total_estimado")
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.DocumentoGerado)
                .HasColumnName("documento_gerado")
                .HasDefaultValue(false);

            builder.Property(e => e.CaminhoDocumento)
                .HasColumnName("caminho_documento")
                .HasMaxLength(500);

            builder.Property(e => e.Observacoes)
                .HasColumnName("observacoes");

            builder.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .HasDefaultValue(true);

            // Índices
            builder.HasIndex(e => e.Protocolo)
                .IsUnique()
                .HasDatabaseName("IX_protocolos_descarte_protocolo");

            builder.HasIndex(e => e.Cliente)
                .HasDatabaseName("idx_protocolos_descarte_cliente");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("idx_protocolos_descarte_status");

            builder.HasIndex(e => e.DataCriacao)
                .HasDatabaseName("idx_protocolos_descarte_data_criacao");

            builder.HasIndex(e => e.TipoDescarte)
                .HasDatabaseName("idx_protocolos_descarte_tipo");

            // Relacionamentos
            builder.HasOne(e => e.ClienteNavigation)
                .WithMany()
                .HasForeignKey(e => e.Cliente)
                .HasConstraintName("fk_protocolos_descarte_cliente");

            builder.HasOne(e => e.ResponsavelNavigation)
                .WithMany()
                .HasForeignKey(e => e.ResponsavelProtocolo)
                .HasConstraintName("fk_protocolos_descarte_responsavel");

            builder.HasMany(e => e.Itens)
                .WithOne(i => i.Protocolo)
                .HasForeignKey(i => i.ProtocoloId)
                .HasConstraintName("fk_protocolo_itens_protocolo");

            builder.HasMany(e => e.Evidencias)
                .WithOne()
                .HasForeignKey(e => e.ProtocoloId)
                .HasConstraintName("fk_descarteevidencias_protocolo");
        }
    }
}
