using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwtelefoniaMap : IEntityTypeConfiguration<Vwtelefonium>
    {
        public void Configure(EntityTypeBuilder<Vwtelefonium> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwtelefonia");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Contrato)
                .HasMaxLength(100)
                .HasColumnName("contrato");

            entity.Property(e => e.Emuso).HasColumnName("emuso");

            entity.Property(e => e.Iccid)
                .HasMaxLength(500)
                .HasColumnName("iccid");

            entity.Property(e => e.Numero).HasColumnName("numero");

            entity.Property(e => e.Operadora)
                .HasMaxLength(100)
                .HasColumnName("operadora");

            entity.Property(e => e.Plano)
                .HasMaxLength(150)
                .HasColumnName("plano");

            entity.Property(e => e.Valor)
                .HasPrecision(10, 2)
                .HasColumnName("valor");
        }
    }
}
