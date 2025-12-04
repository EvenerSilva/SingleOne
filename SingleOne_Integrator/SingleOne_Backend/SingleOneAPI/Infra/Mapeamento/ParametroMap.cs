using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SingleOne.Models;

namespace SingleOneAPI.Infra.Mapeamento
{
    public class ParametroMap : IEntityTypeConfiguration<Parametro>
    {
        public void Configure(EntityTypeBuilder<Parametro> entity)
        {
            entity.ToTable("parametros");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Cliente).HasColumnName("cliente");

            entity.Property(e => e.Emailreporte)
                .HasMaxLength(300)
                .HasColumnName("emailreporte");

            // Configurações de SMTP
            entity.Property(e => e.SmtpEnabled)
                .HasColumnName("smtp_enabled");

            entity.Property(e => e.SmtpHost)
                .HasMaxLength(200)
                .HasColumnName("smtp_host");

            entity.Property(e => e.SmtpPort)
                .HasColumnName("smtp_port");

            entity.Property(e => e.SmtpLogin)
                .HasMaxLength(200)
                .HasColumnName("smtp_login");

            entity.Property(e => e.SmtpPassword)
                .HasMaxLength(200)
                .HasColumnName("smtp_password");

            entity.Property(e => e.SmtpEnableSSL)
                .HasColumnName("smtp_enable_ssl");

            entity.Property(e => e.SmtpEmailFrom)
                .HasMaxLength(200)
                .HasColumnName("smtp_email_from");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Parametros)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktemplatecliente");
        }
    }
}
