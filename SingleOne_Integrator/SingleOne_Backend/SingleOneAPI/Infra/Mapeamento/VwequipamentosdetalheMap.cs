using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwequipamentosdetalheMap : IEntityTypeConfiguration<Vwequipamentosdetalhe>
    {
        public void Configure(EntityTypeBuilder<Vwequipamentosdetalhe> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwequipamentosdetalhes");

            entity.Property(e => e.Centrocusto)
                .HasMaxLength(100)
                .HasColumnName("centrocusto");

            entity.Property(e => e.Centrocustoid).HasColumnName("centrocustoid");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Empresa)
                .HasMaxLength(250)
                .HasColumnName("empresa");

            entity.Property(e => e.Empresaid).HasColumnName("empresaid");

            entity.Property(e => e.Equipamentostatus)
                .HasMaxLength(100)
                .HasColumnName("equipamentostatus");

            entity.Property(e => e.Equipamentostatusid).HasColumnName("equipamentostatusid");

            entity.Property(e => e.Fabricante)
                .HasMaxLength(200)
                .HasColumnName("fabricante");

            entity.Property(e => e.Fabricanteid).HasColumnName("fabricanteid");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Localidade)
                .HasMaxLength(300)
                .HasColumnName("localidade");

            entity.Property(e => e.Localidadeid).HasColumnName("localidadeid");

            entity.Property(e => e.Modelo)
                .HasMaxLength(200)
                .HasColumnName("modelo");

            entity.Property(e => e.Modeloid).HasColumnName("modeloid");

            entity.Property(e => e.Numeroserie)
                .HasMaxLength(100)
                .HasColumnName("numeroserie");

            entity.Property(e => e.Patrimonio)
                .HasMaxLength(100)
                .HasColumnName("patrimonio");

            entity.Property(e => e.Tipoequipamento)
                .HasMaxLength(200)
                .HasColumnName("tipoequipamento");

            entity.Property(e => e.Tipoequipamentoid).HasColumnName("tipoequipamentoid");
        }
    }
}
