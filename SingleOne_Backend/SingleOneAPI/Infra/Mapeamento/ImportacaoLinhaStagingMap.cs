using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ImportacaoLinhaStagingMap : IEntityTypeConfiguration<ImportacaoLinhaStaging>
    {
        public void Configure(EntityTypeBuilder<ImportacaoLinhaStaging> builder)
        {
            builder.ToTable("importacao_linha_staging");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityColumn();
            
            builder.Property(e => e.Cliente)
                .HasColumnName("cliente")
                .IsRequired();
            
            builder.Property(e => e.LoteId)
                .HasColumnName("lote_id")
                .IsRequired();
            
            builder.Property(e => e.UsuarioImportacao)
                .HasColumnName("usuario_importacao")
                .IsRequired();
            
            builder.Property(e => e.DataImportacao)
                .HasColumnName("data_importacao")
                .IsRequired();
            
            builder.Property(e => e.OperadoraNome)
                .HasColumnName("operadora_nome")
                .HasMaxLength(200);
            
            builder.Property(e => e.ContratoNome)
                .HasColumnName("contrato_nome")
                .HasMaxLength(200);
            
            builder.Property(e => e.PlanoNome)
                .HasColumnName("plano_nome")
                .HasMaxLength(200);
            
            builder.Property(e => e.PlanoValor)
                .HasColumnName("plano_valor")
                .HasColumnType("decimal(18,2)");
            
            builder.Property(e => e.NumeroLinha)
                .HasColumnName("numero_linha")
                .HasColumnType("decimal(18,0)");
            
            builder.Property(e => e.Iccid)
                .HasColumnName("iccid")
                .HasMaxLength(50);
            
            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(1)
                .IsRequired();
            
            builder.Property(e => e.MensagensValidacao)
                .HasColumnName("mensagens_validacao")
                .HasColumnType("text");
            
            builder.Property(e => e.LinhaArquivo)
                .HasColumnName("linha_arquivo");
            
            builder.Property(e => e.OperadoraId)
                .HasColumnName("operadora_id");
            
            builder.Property(e => e.ContratoId)
                .HasColumnName("contrato_id");
            
            builder.Property(e => e.PlanoId)
                .HasColumnName("plano_id");
            
            builder.Property(e => e.CriarOperadora)
                .HasColumnName("criar_operadora")
                .HasDefaultValue(false);
            
            builder.Property(e => e.CriarContrato)
                .HasColumnName("criar_contrato")
                .HasDefaultValue(false);
            
            builder.Property(e => e.CriarPlano)
                .HasColumnName("criar_plano")
                .HasDefaultValue(false);
            
            // Relacionamentos
            builder.HasOne(e => e.UsuarioImportacaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.UsuarioImportacao)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.ClienteNavigation)
                .WithMany()
                .HasForeignKey(e => e.Cliente)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Índices para performance
            builder.HasIndex(e => e.LoteId);
            builder.HasIndex(e => new { e.Cliente, e.LoteId });
            builder.HasIndex(e => e.Status);
        }
    }
}

