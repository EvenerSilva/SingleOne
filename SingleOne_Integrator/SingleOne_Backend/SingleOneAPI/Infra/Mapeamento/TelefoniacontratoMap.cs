using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TelefoniacontratoMap : IEntityTypeConfiguration<Telefoniacontrato>
    {
        public void Configure(EntityTypeBuilder<Telefoniacontrato> entity)
        {
            entity.ToTable("telefoniacontratos");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Descricao)
                .HasMaxLength(250)
                .HasColumnName("descricao");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("nome");

            entity.Property(e => e.Operadora).HasColumnName("operadora");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Telefoniacontratos)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktelcontratocliente");

            entity.HasOne(d => d.OperadoraNavigation)
                .WithMany(p => p.Telefoniacontratos)
                .HasForeignKey(d => d.Operadora)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktelcontratooperadora");
        }
    }
}
