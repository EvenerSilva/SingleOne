using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class EquipamentoanexoMap : IEntityTypeConfiguration<Equipamentoanexo>
    {
        public void Configure(EntityTypeBuilder<Equipamentoanexo> entity)
        {
            entity.ToTable("equipamentoanexos");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Arquivo).HasColumnName("arquivo");

            entity.Property(e => e.Dtregistro)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtregistro");

            entity.Property(e => e.Equipamento).HasColumnName("equipamento");

            entity.Property(e => e.Isbo).HasColumnName("isbo");

            entity.Property(e => e.Islaudo).HasColumnName("islaudo");

            entity.Property(e => e.Laudo).HasColumnName("laudo");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("nome");

            entity.Property(e => e.Usuario).HasColumnName("usuario");

            entity.HasOne(d => d.EquipamentoNavigation)
                .WithMany(p => p.Equipamentoanexos)
                .HasForeignKey(d => d.Equipamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkequipamentoanexoequipamento");

            entity.HasOne(d => d.UsuarioNavigation)
                .WithMany(p => p.Equipamentoanexos)
                .HasForeignKey(d => d.Usuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkequipamentoanexousuario");
        }
    }
}
