using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class LaudoMap : IEntityTypeConfiguration<Laudo>
    {
        public void Configure(EntityTypeBuilder<Laudo> entity)
        {
            entity.ToTable("laudos");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Descricao)
                .IsRequired()
                .HasColumnName("descricao");

            entity.Property(e => e.Dtentrada)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtentrada");

            entity.Property(e => e.Dtlaudo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtlaudo");

            entity.Property(e => e.Equipamento).HasColumnName("equipamento");

            entity.Property(e => e.Laudo1).HasColumnName("laudo");

            entity.Property(e => e.Mauuso).HasColumnName("mauuso");

            entity.Property(e => e.Tecnico).HasColumnName("tecnico");

            entity.Property(e => e.Temconserto).HasColumnName("temconserto");

            entity.Property(e => e.Usuario).HasColumnName("usuario");

            entity.Property(e => e.Valormanutencao)
                .HasPrecision(10, 2)
                .HasColumnName("valormanutencao");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Laudos)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fklaudocliente");

            entity.HasOne(d => d.EquipamentoNavigation)
                .WithMany(p => p.Laudos)
                .HasForeignKey(d => d.Equipamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fklaudoequipamento");

            entity.HasOne(d => d.TecnicoNavigation)
                .WithMany(p => p.LaudoTecnicoNavigations)
                .HasForeignKey(d => d.Tecnico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fklaudotecnico");

            entity.HasOne(d => d.UsuarioNavigation)
                .WithMany(p => p.LaudoUsuarioNavigations)
                .HasForeignKey(d => d.Usuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fklaudousuario");
        }
    }
}
