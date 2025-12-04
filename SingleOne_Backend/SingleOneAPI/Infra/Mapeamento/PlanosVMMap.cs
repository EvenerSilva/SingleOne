using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOneAPI.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class PlanosVMMap : IEntityTypeConfiguration<PlanosVM>
    {
        public void Configure(EntityTypeBuilder<PlanosVM> builder)
        {
            builder.HasNoKey();
            builder.ToView("planosvm");
            
            // ✅ Mapeamento correto das propriedades
            builder.Property(c => c.Id).HasColumnName("id");
            builder.Property(c => c.Plano).HasColumnName("plano");
            builder.Property(c => c.Ativo).HasColumnName("ativo");
            builder.Property(c => c.Valor).HasColumnName("valor");
            builder.Property(c => c.Contrato).HasColumnName("contrato");
            builder.Property(c => c.ContratoId).HasColumnName("contratoid");
            builder.Property(c => c.Operadora).HasColumnName("operadora");
            builder.Property(c => c.OperadoraId).HasColumnName("operadoraid");
            
            // ✅ Mapeamento correto dos campos de contagem
            builder.Property(c => c.ContLinhas).HasColumnName("contlinhas");
            builder.Property(c => c.ContLinhasEmUso).HasColumnName("contlinhasemuso");
            builder.Property(c => c.ContLinhasLivres).HasColumnName("contlinhaslivres");
        }
    }
}
