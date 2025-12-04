namespace SingleOneAPI
{
    public class EnvironmentApiSettings
    {
        public string SiteUrl { get; private set; }
        public bool SMTPEnabled { get; private set; }
        public string SMTPHost { get; private set; }
        public int SMTPPort { get; private set; }
        public string SMTPLogin { get; private set; }
        public string SMTPPassword { get; private set; }
        public bool SMTPEnableSSL { get; private set; }
        public string SMTPEmailFrom { get; private set; }
        public DatabaseConfiguration DatabaseConfiguration { get; set; }

        public EnvironmentApiSettings(string siteUrl, string smtpHost, int? smtpPort, string smtpLogin, string smtpPassword, string smtpEmailFrom, bool smtpEnableSSL = false, bool smtpEnabled = false)
        {
            SiteUrl = (!string.IsNullOrEmpty(siteUrl) ? siteUrl : "http://localhost:4200");
            SMTPEnabled = smtpEnabled;
            SMTPHost = smtpHost; // Sem fallback hardcoded
            SMTPPort = smtpPort ?? 0; // Sem fallback hardcoded
            SMTPLogin = smtpLogin; // Sem fallback hardcoded
            SMTPPassword = smtpPassword; // Sem fallback hardcoded
            SMTPEnableSSL = smtpEnableSSL;
            SMTPEmailFrom = smtpEmailFrom; // Sem fallback hardcoded
        }

        // Método para atualizar configurações SMTP do banco de dados
        public void UpdateSmtpSettings(string smtpHost, int? smtpPort, string smtpLogin, string smtpPassword, string smtpEmailFrom, bool? smtpEnableSSL, bool? smtpEnabled)
        {
            if (smtpEnabled.HasValue)
                SMTPEnabled = smtpEnabled.Value;
            
            if (!string.IsNullOrEmpty(smtpHost))
                SMTPHost = smtpHost;
            
            if (smtpPort.HasValue && smtpPort.Value > 0)
                SMTPPort = smtpPort.Value;
            
            if (!string.IsNullOrEmpty(smtpLogin))
                SMTPLogin = smtpLogin;
            
            if (!string.IsNullOrEmpty(smtpPassword))
                SMTPPassword = smtpPassword;
            
            if (!string.IsNullOrEmpty(smtpEmailFrom))
                SMTPEmailFrom = smtpEmailFrom;
            
            if (smtpEnableSSL.HasValue)
                SMTPEnableSSL = smtpEnableSSL.Value;
        }
    }

    public class DatabaseConfiguration
    {
        public string Host { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public DatabaseConfiguration(string host, string username, string password)
        {
            Host = !string.IsNullOrEmpty(host) ? host : "localhost";
            Username = !string.IsNullOrEmpty(username) ? username : "postgres";
            Password = !string.IsNullOrEmpty(password) ? password : "password";
        }
    }
}
