using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class FilialMap : IEntityTypeConfiguration<Filial>
    {
        public void Configure(EntityTypeBuilder<Filial> entity)
        {
            entity.ToTable("filiais");

            entity.HasKey(e => e.Id)
                .HasName("filiais_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("nome");

            entity.Property(e => e.EmpresaId)
                .IsRequired()
                .HasColumnName("empresa_id");

            entity.Property(e => e.LocalidadeId)
                .IsRequired()
                .HasColumnName("localidade_id");

            entity.Property(e => e.Cnpj)
                .HasMaxLength(18)
                .HasColumnName("cnpj");

            entity.Property(e => e.Endereco)
                .HasColumnName("endereco");

            entity.Property(e => e.Telefone)
                .HasMaxLength(20)
                .HasColumnName("telefone");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");

            entity.Property(e => e.Ativo)
                .HasColumnName("ativo")
                .HasDefaultValue(true)
                .IsRequired(false)
                .ValueGeneratedNever();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relacionamentos
            entity.HasOne(d => d.Empresa)
                .WithMany(e => e.Filiais)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_filial_empresa");

            entity.HasOne(d => d.Localidade)
                .WithMany()
                .HasForeignKey(d => d.LocalidadeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_filiais_localidade");

            // Índices
            entity.HasIndex(e => e.Cnpj)
                .HasDatabaseName("idx_filiais_cnpj");

            entity.HasIndex(e => e.EmpresaId)
                .HasDatabaseName("idx_filiais_empresa");

            entity.HasIndex(e => e.LocalidadeId)
                .HasDatabaseName("idx_filiais_localidade");
        }
    }
}
