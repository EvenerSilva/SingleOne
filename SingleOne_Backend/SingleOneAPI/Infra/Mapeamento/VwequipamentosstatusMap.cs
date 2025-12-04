using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwequipamentosstatusMap : IEntityTypeConfiguration<Vwequipamentosstatus>
    {
        public void Configure(EntityTypeBuilder<Vwequipamentosstatus> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwequipamentosstatus");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Danificado).HasColumnName("danificado");

            entity.Property(e => e.Descartado).HasColumnName("descartado");

            entity.Property(e => e.Devolvido).HasColumnName("devolvido");

            entity.Property(e => e.Emestoque).HasColumnName("emestoque");

            entity.Property(e => e.Entregue).HasColumnName("entregue");

            entity.Property(e => e.Extraviado).HasColumnName("extraviado");

            entity.Property(e => e.Migrado).HasColumnName("migrado");

            entity.Property(e => e.Novo).HasColumnName("novo");

            entity.Property(e => e.Requisitado).HasColumnName("requisitado");

            entity.Property(e => e.Roubado).HasColumnName("roubado");

            entity.Property(e => e.Semconserto).HasColumnName("semconserto");

            entity.Property(e => e.Tipoequipamento)
                .HasMaxLength(200)
                .HasColumnName("tipoequipamento");
        }
    }
}
