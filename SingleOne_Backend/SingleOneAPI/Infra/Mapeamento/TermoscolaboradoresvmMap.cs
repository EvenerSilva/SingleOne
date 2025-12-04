using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class TermoscolaboradoresvmMap : IEntityTypeConfiguration<Termoscolaboradoresvm>
    {
        public void Configure(EntityTypeBuilder<Termoscolaboradoresvm> entity)
        {
            entity.HasNoKey();

            entity.ToView("termoscolaboradoresvm");

            entity.Property(e => e.Colaboradorfinal)
                .HasMaxLength(300)
                .HasColumnName("colaboradorfinal");

            entity.Property(e => e.Colaboradorfinalid).HasColumnName("colaboradorfinalid");

            entity.Property(e => e.Dtenviotermo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dtenviotermo");

            entity.Property(e => e.Situacao).HasColumnName("situacao");
        }
    }
}
