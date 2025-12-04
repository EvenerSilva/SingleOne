using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class MotivoSuspeitaMap : IEntityTypeConfiguration<MotivoSuspeita>
    {
        public void Configure(EntityTypeBuilder<MotivoSuspeita> builder)
        {
            builder.ToTable("motivos_suspeita");
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Codigo).HasColumnName("codigo").HasMaxLength(50);
            builder.Property(e => e.Descricao).HasColumnName("descricao").HasMaxLength(200);
            builder.Property(e => e.DescricaoDetalhada).HasColumnName("descricao_detalhada");
            builder.Property(e => e.PrioridadePadrao).HasColumnName("prioridade_padrao").HasMaxLength(20);
            builder.Property(e => e.Ativo).HasColumnName("ativo");
            builder.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            // Índice único para código
            builder.HasIndex(e => e.Codigo).IsUnique();
        }
    }
}
