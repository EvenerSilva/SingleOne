using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwdevolucaoprogramadaMap : IEntityTypeConfiguration<Vwdevolucaoprogramadum>
    {
        public void Configure(EntityTypeBuilder<Vwdevolucaoprogramadum> entity)
        {
            // ⚠️ ATENÇÃO: A view vwdevolucaoprogramada no banco possui APENAS 3 colunas
            entity.HasNoKey();

            entity.ToView("vwdevolucaoprogramada");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Nomecolaborador)
                .HasMaxLength(300)
                .HasColumnName("nomecolaborador");

            entity.Property(e => e.Dtprogramadaretorno).HasColumnName("dtprogramadaretorno");
        }
    }

}
