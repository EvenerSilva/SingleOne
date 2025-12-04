using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ContratoMap : IEntityTypeConfiguration<Contrato>
    {
        public void Configure(EntityTypeBuilder<Contrato> builder)
        {
            builder.ToTable("contratos");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Cliente).HasColumnName("cliente");
            builder.HasOne(e => e.ClienteNavigation)
                   .WithMany()
                   .HasForeignKey(e => e.Cliente);

            builder.Property(e => e.Fornecedor).HasColumnName("fornecedor");
            builder.HasOne(e => e.FornecedorNavigation)
                   .WithMany()
                   .HasForeignKey(e => e.Fornecedor);

            builder.Property(e => e.Numero).HasColumnName("numero");
            builder.Property(e => e.Aditivo).HasColumnName("aditivo");

            builder.Property(e => e.Descricao)
                .HasMaxLength(100)
                .HasColumnName("descricao");
            
            builder.Property(e => e.DTInicioVigencia)
                .IsRequired()
                .HasColumnName("dtiniciovigencia");
            
            builder.Property(e => e.DTFinalVigencia).HasColumnName("dtfinalvigencia");
            builder.Property(e => e.Valor).HasColumnName("valor");
            builder.Property(e => e.Status).HasColumnName("status");
            
            builder.HasOne(e => e.StatusContratoNavigation)
                   .WithMany(s => s.Contratos)
                   .HasForeignKey(e => e.Status);

            builder.Property(e => e.GeraNF).IsRequired().HasColumnName("geranf");
            builder.Property(e => e.DTCriacao).IsRequired().HasColumnName("dtcriacao"); ;
            
            builder.Property(e => e.DTExclusao).HasColumnName("dtexclusao");
            builder.Property(e => e.UsuarioExclusao).HasColumnName("usuarioexclusao");
            
            builder.Property(e => e.UsuarioCriacao).HasColumnName("usuariocriacao");
            builder.HasOne(e => e.UsuarioCriacaoNavigation)
                   .WithMany()
                   .HasForeignKey(e => e.UsuarioCriacao);

            builder.HasOne(e => e.UsuarioExclusaoNavigation)
                   .WithMany()
                   .HasForeignKey(e => e.UsuarioExclusao);

            builder.Property(e => e.Renovavel)
                   .IsRequired()
                   .HasColumnName("renovavel");

            builder.Property(e => e.ArquivoContrato)
                   .HasMaxLength(500)
                   .HasColumnName("arquivocontrato");

            builder.Property(e => e.NomeArquivoOriginal)
                   .HasMaxLength(255)
                   .HasColumnName("nomearquivooriginal");

            builder.Property(e => e.DataUploadArquivo)
                   .HasColumnName("datauploadarquivo");

            builder.Property(e => e.UsuarioUploadArquivo)
                   .HasColumnName("usuariouploadarquivo");

            builder.HasOne(e => e.UsuarioUploadArquivoNavigation)
                   .WithMany()
                   .HasForeignKey(e => e.UsuarioUploadArquivo);

            builder.Property(e => e.UsuarioRemocaoArquivo)
                   .HasColumnName("usuarioremocaoarquivo");

            builder.Property(e => e.DataRemocaoArquivo)
                   .HasColumnName("dataremocaoarquivo");

            builder.HasOne(e => e.UsuarioRemocaoArquivoNavigation)
                   .WithMany()
                   .HasForeignKey(e => e.UsuarioRemocaoArquivo);
        }
    }

}
