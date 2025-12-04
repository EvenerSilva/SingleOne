using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class PoliticaElegibilidadeMap : IEntityTypeConfiguration<PoliticaElegibilidade>
    {
        public void Configure(EntityTypeBuilder<PoliticaElegibilidade> builder)
        {
            builder.ToTable("politicas_elegibilidade");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
                
            builder.Property(e => e.Cliente)
                .HasColumnName("cliente")
                .IsRequired();
                
            builder.Property(e => e.TipoColaborador)
                .HasColumnName("tipo_colaborador")
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(e => e.Cargo)
                .HasColumnName("cargo")
                .HasMaxLength(100);
                
            builder.Property(e => e.UsarPadrao)
                .HasColumnName("usarpadrao")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(e => e.TipoEquipamentoId)
                .HasColumnName("tipo_equipamento_id")
                .IsRequired();
                
            builder.Property(e => e.PermiteAcesso)
                .HasColumnName("permite_acesso")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(e => e.QuantidadeMaxima)
                .HasColumnName("quantidade_maxima");
                
            builder.Property(e => e.Observacoes)
                .HasColumnName("observacoes")
                .HasColumnType("text");
                
            builder.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(e => e.DtCadastro)
                .HasColumnName("dt_cadastro")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(e => e.DtAtualizacao)
                .HasColumnName("dt_atualizacao")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(e => e.UsuarioCadastro)
                .HasColumnName("usuario_cadastro");

            // Relacionamentos
            builder.HasOne(d => d.ClienteNavigation)
                .WithMany()
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_politica_cliente");

            builder.HasOne(d => d.TipoEquipamentoNavigation)
                .WithMany()
                .HasForeignKey(d => d.TipoEquipamentoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_politica_tipo_equipamento");

            builder.HasOne(d => d.UsuarioCadastroNavigation)
                .WithMany()
                .HasForeignKey(d => d.UsuarioCadastro)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_politica_usuario");

            // Índices
            builder.HasIndex(e => e.Cliente)
                .HasDatabaseName("idx_politica_cliente");
                
            builder.HasIndex(e => e.TipoColaborador)
                .HasDatabaseName("idx_politica_tipo_colaborador");
                
            builder.HasIndex(e => e.TipoEquipamentoId)
                .HasDatabaseName("idx_politica_tipo_equipamento");
                
            builder.HasIndex(e => e.Ativo)
                .HasDatabaseName("idx_politica_ativo");
                
            builder.HasIndex(e => e.UsarPadrao)
                .HasDatabaseName("idx_politica_usarpadrao");
        }
    }
}

