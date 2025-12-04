using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EquipamentohistoricovmMap : IEntityTypeConfiguration<Equipamentohistoricovm>
    {
        public void Configure(EntityTypeBuilder<Equipamentohistoricovm> entity)
        {
            entity.HasNoKey();

            entity.ToView("equipamentohistoricovm");

            entity.Property(e => e.Colaborador)
                .HasMaxLength(300)
                .HasColumnName("colaborador");

            entity.Property(e => e.Colaboradorid).HasColumnName("colaboradorid");

            entity.Property(e => e.Dtregistro)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtregistro");

            entity.Property(e => e.Equipamentostatus)
                .HasMaxLength(100)
                .HasColumnName("equipamentostatus");

            entity.Property(e => e.Equipamentostatusid).HasColumnName("equipamentostatusid");

            entity.Property(e => e.Fabricante)
                .HasMaxLength(200)
                .HasColumnName("fabricante");

            entity.Property(e => e.Fabricanteid).HasColumnName("fabricanteid");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Equipamentoid).HasColumnName("equipamentoid");

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

            entity.Property(e => e.Usuario)
                .HasMaxLength(200)
                .HasColumnName("usuario");

            entity.Property(e => e.Usuarioid).HasColumnName("usuarioid");

            entity.Property(e => e.Tecnicoresponsavel)
                .HasMaxLength(300)
                .HasColumnName("tecnicoresponsavel");

            entity.Property(e => e.Tecnicoresponsavelid).HasColumnName("tecnicoresponsavelid");
        }
    }
}
