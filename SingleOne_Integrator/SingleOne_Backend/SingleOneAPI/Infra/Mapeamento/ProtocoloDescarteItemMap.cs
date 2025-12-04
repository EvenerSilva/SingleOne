using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    /// <summary>
    /// Mapeamento Entity Framework para ProtocoloDescarteItem
    /// </summary>
    public class ProtocoloDescarteItemMap : IEntityTypeConfiguration<ProtocoloDescarteItem>
    {
        public void Configure(EntityTypeBuilder<ProtocoloDescarteItem> builder)
        {
            builder.ToTable("protocolo_descarte_itens");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ProtocoloId)
                .HasColumnName("protocolo_id")
                .IsRequired();

            builder.Property(e => e.Equipamento)
                .HasColumnName("equipamento")
                .IsRequired();

            builder.Property(e => e.ProcessoSanitizacao)
                .HasColumnName("processo_sanitizacao")
                .HasDefaultValue(false);

            builder.Property(e => e.ProcessoDescaracterizacao)
                .HasColumnName("processo_descaracterizacao")
                .HasDefaultValue(false);

            builder.Property(e => e.ProcessoPerfuracaoDisco)
                .HasColumnName("processo_perfuracao_disco")
                .HasDefaultValue(false);

            builder.Property(e => e.EvidenciasObrigatorias)
                .HasColumnName("evidencias_obrigatorias")
                .HasDefaultValue(false);

            builder.Property(e => e.EvidenciasExecutadas)
                .HasColumnName("evidencias_executadas")
                .HasDefaultValue(false);

            builder.Property(e => e.ValorEstimado)
                .HasColumnName("valor_estimado")
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.ObservacoesItem)
                .HasColumnName("observacoes_item");

            builder.Property(e => e.DataProcessoIniciado)
                .HasColumnName("data_processo_iniciado");

            builder.Property(e => e.DataProcessoConcluido)
                .HasColumnName("data_processo_concluido");

            builder.Property(e => e.StatusItem)
                .HasColumnName("status_item")
                .HasMaxLength(30)
                .HasDefaultValue("PENDENTE")
                .IsRequired();

            builder.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .HasDefaultValue(true);

            // Índices
            builder.HasIndex(e => e.ProtocoloId)
                .HasDatabaseName("idx_protocolo_itens_protocolo");

            builder.HasIndex(e => e.Equipamento)
                .HasDatabaseName("idx_protocolo_itens_equipamento");

            builder.HasIndex(e => e.StatusItem)
                .HasDatabaseName("idx_protocolo_itens_status");

            // Relacionamentos
            builder.HasOne(e => e.Protocolo)
                .WithMany(p => p.Itens)
                .HasForeignKey(e => e.ProtocoloId)
                .HasConstraintName("fk_protocolo_itens_protocolo")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.EquipamentoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Equipamento)
                .HasConstraintName("fk_protocolo_itens_equipamento");
        }
    }
}
