using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class NotasfiscaiMap : IEntityTypeConfiguration<Notasfiscai>
    {
        public void Configure(EntityTypeBuilder<Notasfiscai> entity)
        {
            entity.ToTable("notasfiscais");
            entity.HasKey("Id");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Descricao)
                .HasMaxLength(500)
                .HasColumnName("descricao");

            entity.Property(e => e.Dtemissao).HasColumnName("dtemissao");

            entity.Property(e => e.Fornecedor).HasColumnName("fornecedor");

            entity.Property(e => e.Gerouequipamento).HasColumnName("gerouequipamento");

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.Property(e => e.Numero).HasColumnName("numero");

            entity.Property(e => e.Valor)
                .HasPrecision(10, 2)
                .HasColumnName("valor");

            entity.Property(e => e.Virtual).HasColumnName("virtual");

            entity.Property(e => e.ArquivoNotaFiscal)
                .HasMaxLength(500)
                .HasColumnName("arquivonotafiscal");

            entity.Property(e => e.NomeArquivoOriginal)
                .HasMaxLength(255)
                .HasColumnName("nomearquivooriginal");

            entity.Property(e => e.DataUploadArquivo)
                .HasColumnName("datauploadarquivo");

            entity.Property(e => e.UsuarioUploadArquivo)
                .HasColumnName("usuariouploadarquivo");

            entity.Property(e => e.UsuarioRemocaoArquivo)
                .HasColumnName("usuarioremocaoarquivo");

            entity.Property(e => e.DataRemocaoArquivo)
                .HasColumnName("dataremocaoarquivo");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Notasfiscais)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fknfcliente");

            entity.HasOne(d => d.FornecedorNavigation)
                .WithMany(p => p.Notasfiscais)
                .HasForeignKey(d => d.Fornecedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fknffornecedor");

            entity.HasOne(d => d.UsuarioUploadArquivoNavigation)
                .WithMany()
                .HasForeignKey(d => d.UsuarioUploadArquivo)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.UsuarioRemocaoArquivoNavigation)
                .WithMany()
                .HasForeignKey(d => d.UsuarioRemocaoArquivo)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
