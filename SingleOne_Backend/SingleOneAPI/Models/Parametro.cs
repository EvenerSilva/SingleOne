using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    public partial class Parametro
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        [Column("emailreporte")]
        public string Emailreporte { get; set; }
        
        // Configuração de E-mail para Descontos
        [Column("email_descontos_enabled")]
        public bool? EmailDescontosEnabled { get; set; }
        
        // Configurações de SMTP
        [Column("smtp_enabled")]
        public bool? SmtpEnabled { get; set; }
        [Column("smtp_host")]
        public string SmtpHost { get; set; }
        [Column("smtp_port")]
        public int? SmtpPort { get; set; }
        [Column("smtp_login")]
        public string SmtpLogin { get; set; }
        [Column("smtp_password")]
        public string SmtpPassword { get; set; }
        [Column("smtp_enable_ssl")]
        public bool? SmtpEnableSSL { get; set; }
        [Column("smtp_email_from")]
        public string SmtpEmailFrom { get; set; }

        // Configurações de 2FA (Duplo Fator)
        [Column("two_factor_enabled")]
        public bool? TwoFactorEnabled { get; set; }
        [Column("two_factor_type")]
        public string TwoFactorType { get; set; }
        [Column("two_factor_expiration_minutes")]
        public int? TwoFactorExpirationMinutes { get; set; }
        [Column("two_factor_max_attempts")]
        public int? TwoFactorMaxAttempts { get; set; }
        [Column("two_factor_lockout_minutes")]
        public int? TwoFactorLockoutMinutes { get; set; }
        [Column("two_factor_email_template")]
        public string TwoFactorEmailTemplate { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
    }
}
