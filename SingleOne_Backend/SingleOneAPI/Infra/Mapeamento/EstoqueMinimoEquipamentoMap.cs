using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EstoqueMinimoEquipamentoMap : IEntityTypeConfiguration<EstoqueMinimoEquipamento>
    {
        public void Configure(EntityTypeBuilder<EstoqueMinimoEquipamento> builder)
        {
            builder.ToTable("estoqueminimoequipamentos");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Cliente)
                .HasColumnName("cliente")
                .IsRequired();

            builder.Property(e => e.Modelo)
                .HasColumnName("modelo")
                .IsRequired();

            builder.Property(e => e.Localidade)
                .HasColumnName("localidade")
                .IsRequired();

            builder.Property(e => e.QuantidadeMinima)
                .HasColumnName("quantidademinima")
                .IsRequired();

            builder.Property(e => e.QuantidadeMaxima)
                .HasColumnName("quantidademaxima")
                .HasDefaultValue(0);

            builder.Property(e => e.QuantidadeTotalLancada)
                .HasColumnName("quantidadetotallancada")
                .HasDefaultValue(0);

            builder.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.DtCriacao)
                .HasColumnName("dtcriacao")
                .IsRequired();

            builder.Property(e => e.UsuarioCriacao)
                .HasColumnName("usuariocriacao")
                .IsRequired();

            builder.Property(e => e.DtAtualizacao)
                .HasColumnName("dtatualizacao");

            builder.Property(e => e.UsuarioAtualizacao)
                .HasColumnName("usuarioatualizacao");

            builder.Property(e => e.Observacoes)
                .HasColumnName("observacoes");

            // Configurações de navegação (opcionais)
            builder.HasOne(e => e.ClienteNavigation)
                .WithMany()
                .HasForeignKey(e => e.Cliente)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ModeloNavigation)
                .WithMany()
                .HasForeignKey(e => e.Modelo)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.LocalidadeNavigation)
                .WithMany()
                .HasForeignKey(e => e.Localidade)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.UsuarioCriacaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.UsuarioCriacao)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.UsuarioAtualizacaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.UsuarioAtualizacao)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
