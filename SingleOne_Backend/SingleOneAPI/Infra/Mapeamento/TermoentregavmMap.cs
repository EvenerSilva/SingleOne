using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TermoentregavmMap : IEntityTypeConfiguration<Termoentregavm>
    {
        public void Configure(EntityTypeBuilder<Termoentregavm> entity)
        {
            entity.HasNoKey();

            entity.ToView("termoentregavm");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Colaboradorfinal).HasColumnName("colaboradorfinal");

            entity.Property(e => e.Dtentrega)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtentrega");

            entity.Property(e => e.Dtprogramadaretorno).HasColumnName("dtprogramadaretorno");

            entity.Property(e => e.Fabricante)
                .HasColumnType("character varying")
                .HasColumnName("fabricante");

            entity.Property(e => e.Hashrequisicao)
                .HasMaxLength(200)
                .HasColumnName("hashrequisicao");

            entity.Property(e => e.Modelo)
                .HasColumnType("character varying")
                .HasColumnName("modelo");

            entity.Property(e => e.Numeroserie)
                .HasColumnType("character varying")
                .HasColumnName("numeroserie");

            entity.Property(e => e.Observacaoentrega)
                .HasMaxLength(500)
                .HasColumnName("observacaoentrega");

            entity.Property(e => e.Patrimonio)
                .HasColumnType("character varying")
                .HasColumnName("patrimonio");

            entity.Property(e => e.Tipoequipamento)
                .HasMaxLength(200)
                .HasColumnName("tipoequipamento");

            //TipoAquisicao
            entity.Property(e => e.TipoAquisicao)
                .HasMaxLength(200)
                .HasColumnName("tipoaquisicao");
        }
    }

}
