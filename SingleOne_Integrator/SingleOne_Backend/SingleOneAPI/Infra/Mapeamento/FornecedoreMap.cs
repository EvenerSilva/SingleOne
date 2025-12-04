using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class FornecedoreMap : IEntityTypeConfiguration<Fornecedore>
    {
        public void Configure(EntityTypeBuilder<Fornecedore> entity)
        {
            entity.ToTable("fornecedores");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Cnpj)
                .HasMaxLength(20)
                .HasColumnName("cnpj");

            entity.Property(e => e.Migrateid).HasColumnName("migrateid");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("nome");

            entity.Property(e => e.DestinadorResiduos)
                .HasColumnName("destinador_residuos");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Fornecedores)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkfornecedorcliente");
        }
    }
}
