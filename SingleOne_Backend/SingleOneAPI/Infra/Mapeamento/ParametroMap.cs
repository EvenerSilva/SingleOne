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

            // Configuração de E-mail para Descontos
            entity.Property(e => e.EmailDescontosEnabled)
                .HasColumnName("email_descontos_enabled");

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

            // Configurações de 2FA (Duplo Fator)
            entity.Property(e => e.TwoFactorEnabled)
                .HasColumnName("two_factor_enabled");

            entity.Property(e => e.TwoFactorType)
                .HasMaxLength(50)
                .HasColumnName("two_factor_type");

            entity.Property(e => e.TwoFactorExpirationMinutes)
                .HasColumnName("two_factor_expiration_minutes");

            entity.Property(e => e.TwoFactorMaxAttempts)
                .HasColumnName("two_factor_max_attempts");

            entity.Property(e => e.TwoFactorLockoutMinutes)
                .HasColumnName("two_factor_lockout_minutes");

            entity.Property(e => e.TwoFactorEmailTemplate)
                .HasColumnName("two_factor_email_template");

            entity.HasOne(d => d.ClienteNavigation)
                .WithMany(p => p.Parametros)
                .HasForeignKey(d => d.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fktemplatecliente");
        }
    }
}
