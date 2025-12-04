using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TelefoniaplanoMap : IEntityTypeConfiguration<Telefoniaplano>
    {
        public void Configure(EntityTypeBuilder<Telefoniaplano> entity)
        {
            entity.ToTable("telefoniaplanos");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Contrato).HasColumnName("contrato");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("nome");

            entity.Property(e => e.Valor)
                .HasPrecision(10, 2)
                .HasColumnName("valor");

            entity.HasOne(d => d.ContratoNavigation)
                .WithMany(p => p.Telefoniaplanos)
                .HasForeignKey(d => d.Contrato)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktelefoniaplanoscontrato");
        }
    }
}
