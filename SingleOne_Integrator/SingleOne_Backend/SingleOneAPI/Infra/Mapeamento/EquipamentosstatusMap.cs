using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EquipamentosstatusMap : IEntityTypeConfiguration<Equipamentosstatus>
    {
        public void Configure(EntityTypeBuilder<Equipamentosstatus> entity)
        {
            entity.ToTable("equipamentosstatus");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Descricao)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("descricao");
        }
    }

}
