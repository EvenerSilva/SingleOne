using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EmpresaMap : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_empresa");

            entity.ToTable("empresas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cliente).HasColumnName("cliente");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Cnpj).HasColumnName("cnpj");
            entity.Property(e => e.LocalidadeId).HasColumnName("localidade_id");
            entity.Property(e => e.Migrateid).HasColumnName("migrateid");
            
            // ✅ REMOVIDO: Coluna localizacao não existe mais no banco, foi substituída por localidade_id
            // entity.Property<string>("localizacao")
            //     .HasColumnName("localizacao")
            //     .IsRequired(false);
            
            // Configuração dos campos de timestamp
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Localidade)
                .WithMany()
                .HasForeignKey(e => e.LocalidadeId)
                .HasConstraintName("fk_empresas_localidade");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Empresas)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkempresacliente");

            // Relacionamento com Filiais - definido explicitamente para evitar propriedades shadow
            entity.HasMany(e => e.Filiais)
                .WithOne(f => f.Empresa)
                .HasForeignKey(f => f.EmpresaId)
                .HasConstraintName("fk_filial_empresa");
        }
    }
}
