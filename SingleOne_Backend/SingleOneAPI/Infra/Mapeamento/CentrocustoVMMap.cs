using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class CentrocustoVMMap : IEntityTypeConfiguration<CentrocustoVM>
    {
        public void Configure(EntityTypeBuilder<CentrocustoVM> builder)
        {
            builder.HasNoKey();
            builder.ToView("centrocustovm");
            builder.Property(c => c.Id).HasColumnName("id");
            builder.Property(c => c.Empresa).HasColumnName("empresa");
            builder.Property(c => c.EmpresaId).HasColumnName("empresaid");
            builder.Property(c => c.Codigo).HasColumnName("codigo");
            builder.Property(c => c.Nome).HasColumnName("nome");
        }
    }
}
