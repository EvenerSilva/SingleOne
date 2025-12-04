using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwlaudoMap : IEntityTypeConfiguration<Vwlaudo>
    {
        public void Configure(EntityTypeBuilder<Vwlaudo> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwlaudos");

            entity.Property(e => e.Centrocusto).HasColumnName("centrocusto");

            entity.Property(e => e.Centrocustonome)
                .HasMaxLength(100)
                .HasColumnName("centrocustonome");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Descricao).HasColumnName("descricao");

            entity.Property(e => e.Dtentrada)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtentrada");

            entity.Property(e => e.Dtlaudo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtlaudo");

            entity.Property(e => e.Empresa).HasColumnName("empresa");

            entity.Property(e => e.Empresanome)
                .HasMaxLength(250)
                .HasColumnName("empresanome");

            entity.Property(e => e.Equipamento).HasColumnName("equipamento");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Laudo).HasColumnName("laudo");

            entity.Property(e => e.Mauuso).HasColumnName("mauuso");

            entity.Property(e => e.Numeroserie)
                .HasMaxLength(100)
                .HasColumnName("numeroserie");

            entity.Property(e => e.Patrimonio)
                .HasMaxLength(100)
                .HasColumnName("patrimonio");

            entity.Property(e => e.Tecnico).HasColumnName("tecnico");

            entity.Property(e => e.Tecniconome)
                .HasMaxLength(200)
                .HasColumnName("tecniconome");

            entity.Property(e => e.Temconserto).HasColumnName("temconserto");

            entity.Property(e => e.Usuario).HasColumnName("usuario");

            entity.Property(e => e.Usuarionome)
                .HasMaxLength(200)
                .HasColumnName("usuarionome");

            entity.Property(e => e.Valormanutencao)
                .HasPrecision(10, 2)
                .HasColumnName("valormanutencao");
        }
    }
}
