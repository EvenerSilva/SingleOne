using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class HistoricoInvestigacaoMap : IEntityTypeConfiguration<HistoricoInvestigacao>
    {
        public void Configure(EntityTypeBuilder<HistoricoInvestigacao> builder)
        {
            builder.ToTable("historico_investigacoes");
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.SinalizacaoId).HasColumnName("sinalizacao_id");
            builder.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            builder.Property(e => e.Acao).HasColumnName("acao").HasMaxLength(100);
            builder.Property(e => e.Descricao).HasColumnName("descricao");
            builder.Property(e => e.DadosAntes).HasColumnName("dados_antes").HasColumnType("jsonb");
            builder.Property(e => e.DadosDepois).HasColumnName("dados_depois").HasColumnType("jsonb");
            builder.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            // Relacionamentos - IMPORTANTE: HasForeignKey especifica qual propriedade é a FK
            builder.HasOne(e => e.Sinalizacao)
                .WithMany(s => s.Historico)
                .HasForeignKey(e => e.SinalizacaoId)
                .HasConstraintName("fk_historico_sinalizacao")
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .HasConstraintName("fk_historico_usuario")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
