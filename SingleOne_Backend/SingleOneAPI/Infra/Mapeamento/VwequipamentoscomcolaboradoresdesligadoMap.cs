using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwequipamentoscomcolaboradoresdesligadoMap : IEntityTypeConfiguration<Vwequipamentoscomcolaboradoresdesligado>
    {
        public void Configure(EntityTypeBuilder<Vwequipamentoscomcolaboradoresdesligado> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwequipamentoscomcolaboradoresdesligados");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Dtdemissao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtdemissao");
            //.HasColumnName("dtatualizacao");

            entity.Property(e => e.Nome)
                .HasMaxLength(300)
                .HasColumnName("nome");

            entity.Property(e => e.Qtde).HasColumnName("qtde");
        }
    }

}
