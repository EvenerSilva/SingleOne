using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EstoqueMinimoLinhaMap : IEntityTypeConfiguration<EstoqueMinimoLinha>
    {
        public void Configure(EntityTypeBuilder<EstoqueMinimoLinha> builder)
        {
            builder.ToTable("estoqueminimolinhas");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Cliente)
                .HasColumnName("cliente")
                .IsRequired();

            builder.Property(e => e.Operadora)
                .HasColumnName("operadora")
                .IsRequired();

            builder.Property(e => e.Plano)
                .HasColumnName("plano")
                .IsRequired();

            builder.Property(e => e.Localidade)
                .HasColumnName("localidade")
                .IsRequired();

            builder.Property(e => e.QuantidadeMinima)
                .HasColumnName("quantidademinima")
                .IsRequired();

            // Mapeamentos adicionais de quantidades
            builder.Property(e => e.QuantidadeMaxima)
                .HasColumnName("quantidademaxima");

            builder.Property(e => e.QuantidadeTotalLancada)
                .HasColumnName("quantidadetotallancada");

            builder.Property(e => e.PerfilUso)
                .HasColumnName("perfiluso");

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

            builder.HasOne(e => e.OperadoraNavigation)
                .WithMany()
                .HasForeignKey(e => e.Operadora)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.PlanoNavigation)
                .WithMany()
                .HasForeignKey(e => e.Plano)
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
