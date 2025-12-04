using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EquipamentohistoricoMap : IEntityTypeConfiguration<Equipamentohistorico>
    {
        public void Configure(EntityTypeBuilder<Equipamentohistorico> entity)
        {
            entity.ToTable("equipamentohistorico");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Colaborador).HasColumnName("colaborador");

            entity.Property(e => e.Dtregistro)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtregistro");

            entity.Property(e => e.Equipamento).HasColumnName("equipamento");

            entity.Property(e => e.Equipamentostatus).HasColumnName("equipamentostatus");

            entity.Property(e => e.Linhaemuso).HasColumnName("linhaemuso");

            entity.Property(e => e.Linhatelefonica).HasColumnName("linhatelefonica");

            entity.Property(e => e.Requisicao).HasColumnName("requisicao");

            entity.Property(e => e.Usuario).HasColumnName("usuario");

            entity.HasOne(d => d.EquipamentoNavigation)
                .WithMany(p => p.Equipamentohistoricos)
                .HasForeignKey(d => d.Equipamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkeqphistoricoequipamento");

            entity.HasOne(d => d.EquipamentostatusNavigation)
                .WithMany(p => p.Equipamentohistoricos)
                .HasForeignKey(d => d.Equipamentostatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkeqphistoricostatus");

            entity.HasOne(d => d.UsuarioNavigation)
                .WithMany(p => p.Equipamentohistoricos)
                .HasForeignKey(d => d.Usuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkeqphistoricousuario");
        }
    }
}
