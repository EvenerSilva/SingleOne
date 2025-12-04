using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class StatusContratoMap : IEntityTypeConfiguration<StatusContrato>
    {
        public void Configure(EntityTypeBuilder<StatusContrato> builder)
        {
            builder.ToTable("contratostatus");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Nome).HasMaxLength(100).HasColumnName("nome");

            builder.HasMany(e => e.Contratos)
                   .WithOne(c => c.StatusContratoNavigation)
                   .HasForeignKey(c => c.Status);
        }
    }
}
