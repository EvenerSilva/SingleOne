using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ImportacaoLogMap : IEntityTypeConfiguration<ImportacaoLog>
    {
        public void Configure(EntityTypeBuilder<ImportacaoLog> builder)
        {
            builder.ToTable("importacao_log");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityColumn();
            
            builder.Property(e => e.LoteId)
                .HasColumnName("lote_id")
                .IsRequired();
            
            builder.Property(e => e.Cliente)
                .HasColumnName("cliente")
                .IsRequired();
            
            builder.Property(e => e.Usuario)
                .HasColumnName("usuario")
                .IsRequired();
            
            builder.Property(e => e.TipoImportacao)
                .HasColumnName("tipo_importacao")
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(e => e.DataInicio)
                .HasColumnName("data_inicio")
                .IsRequired();
            
            builder.Property(e => e.DataFim)
                .HasColumnName("data_fim");
            
            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(e => e.TotalRegistros)
                .HasColumnName("total_registros");
            
            builder.Property(e => e.TotalValidados)
                .HasColumnName("total_validados");
            
            builder.Property(e => e.TotalErros)
                .HasColumnName("total_erros");
            
            builder.Property(e => e.TotalImportados)
                .HasColumnName("total_importados")
                .IsRequired()
                .HasDefaultValue(0);
            
            builder.Property(e => e.NomeArquivo)
                .HasColumnName("nome_arquivo")
                .HasMaxLength(500);
            
            builder.Property(e => e.Observacoes)
                .HasColumnName("observacoes")
                .HasColumnType("text");
            
            // Relacionamentos
            builder.HasOne(e => e.UsuarioNavigation)
                .WithMany()
                .HasForeignKey(e => e.Usuario)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.ClienteNavigation)
                .WithMany()
                .HasForeignKey(e => e.Cliente)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Índices
            builder.HasIndex(e => e.LoteId);
            builder.HasIndex(e => new { e.Cliente, e.TipoImportacao });
            builder.HasIndex(e => e.DataInicio);
        }
    }
}

