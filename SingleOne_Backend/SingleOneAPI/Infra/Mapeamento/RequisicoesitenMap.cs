using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class RequisicoesitenMap : IEntityTypeConfiguration<Requisicoesiten>
    {
        public void Configure(EntityTypeBuilder<Requisicoesiten> entity)
        {
            entity.ToTable("requisicoesitens");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Dtdevolucao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtdevolucao");

            entity.Property(e => e.Dtentrega)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtentrega");

            entity.Property(e => e.Dtprogramadaretorno).HasColumnName("dtprogramadaretorno");

            entity.Property(e => e.Equipamento).HasColumnName("equipamento");

            entity.Property(e => e.Linhatelefonica).HasColumnName("linhatelefonica");

            entity.Property(e => e.Observacaoentrega)
                .HasMaxLength(500)
                .HasColumnName("observacaoentrega");

            entity.Property(e => e.Requisicao).HasColumnName("requisicao");

            entity.Property(e => e.Usuariodevolucao).HasColumnName("usuariodevolucao");

            entity.Property(e => e.Usuarioentrega).HasColumnName("usuarioentrega");

            entity.HasOne(d => d.EquipamentoNavigation)
                .WithMany(p => p.Requisicoesitens)
                .HasForeignKey(d => d.Equipamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkriequipamento")
                ;

            entity.HasOne(d => d.LinhatelefonicaNavigation)
                .WithMany()
                .HasForeignKey(d => d.Linhatelefonica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkrilinhatelefonica");

            entity.HasOne(d => d.RequisicaoNavigation)
                .WithMany(p => p.Requisicoesitens)
                .HasForeignKey(d => d.Requisicao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkrirequisicao");
        }
    }
}
