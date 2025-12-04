using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class CategoriaMap : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> entity)
        {
            entity.ToTable("categorias");

            entity.HasKey(e => e.Id)
                .HasName("pk_categorias");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Nome)
                .HasColumnName("nome")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Descricao)
                .HasColumnName("descricao");

            entity.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .IsRequired();

            entity.Property(e => e.DataCriacao)
                .HasColumnName("data_criacao")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.DataAtualizacao)
                .HasColumnName("data_atualizacao")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índices
            entity.HasIndex(e => e.Nome)
                .HasDatabaseName("ix_categorias_nome")
                .IsUnique();

            entity.HasIndex(e => e.Ativo)
                .HasDatabaseName("ix_categorias_ativo");

            // Relacionamentos
            entity.HasMany(e => e.TiposEquipamento)
                .WithOne(e => e.Categoria)
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
