using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ClienteMap : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> entity) 
        {
            entity.ToTable("clientes");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Ativo).HasColumnName("ativo");

            entity.Property(e => e.Cnpj)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("cnpj");

            entity.Property(e => e.Razaosocial)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("razaosocial");

            entity.Property(e => e.Logo)
                .HasMaxLength(500)
                .HasColumnName("logo");

            entity.Property(e => e.SiteUrl)
                .HasMaxLength(500)
                .HasColumnName("site_url");
        }
    }
}
