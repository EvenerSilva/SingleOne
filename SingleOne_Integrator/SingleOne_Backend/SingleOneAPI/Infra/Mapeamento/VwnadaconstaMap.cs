using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class VwnadaconstaMap : IEntityTypeConfiguration<Vwnadaconstum>
    {
        public void Configure(EntityTypeBuilder<Vwnadaconstum> entity)
        {
            entity.HasNoKey();

            entity.ToView("vwnadaconsta");

            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .HasColumnName("cargo");

            entity.Property(e => e.Centrocusto)
                .HasMaxLength(100)
                .HasColumnName("centrocusto");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Cpf)
                .HasMaxLength(50)
                .HasColumnName("cpf");

            entity.Property(e => e.Empresa)
                .HasMaxLength(250)
                .HasColumnName("empresa");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Maquinascomcolaborador).HasColumnName("maquinascomcolaborador");

            entity.Property(e => e.Matricula)
                .HasMaxLength(50)
                .HasColumnName("matricula");

            entity.Property(e => e.Nome)
                .HasMaxLength(300)
                .HasColumnName("nome");
        }
    }
}
