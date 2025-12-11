using SingleOneAPI;
using SingleOneAPI.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace SingleOne.Util
{
    public class SendMail
    {
        private readonly EnvironmentApiSettings _environmentApiSettings;
        private readonly ISmtpConfigService _smtpConfigService;

        public SendMail(EnvironmentApiSettings environmentApiSettings)
        {
            _environmentApiSettings = environmentApiSettings;
            _smtpConfigService = null; // Será injetado via construtor alternativo
        }

        public SendMail(EnvironmentApiSettings environmentApiSettings, ISmtpConfigService smtpConfigService)
        {
            _environmentApiSettings = environmentApiSettings;
            _smtpConfigService = smtpConfigService;
        }

        public async Task EnviarAsync(string destinatario, string assunto, string mensagemHtml, byte[] anexo = null, int clienteId = 0)
        {
            // Se temos o serviço de configuração e um clienteId, tentar carregar configurações do banco
            if (_smtpConfigService != null && clienteId > 0)
            {
                await _smtpConfigService.LoadSmtpSettingsFromDatabase(_environmentApiSettings, clienteId);
            }

            Enviar(destinatario, assunto, mensagemHtml, anexo);
        }

        public void Enviar(string destinatario, string assunto, string mensagemHtml, byte[] anexo = null)
        {
            // Path de logo alinhado com a pasta 'Documentos' usada no publish (.csproj)
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "logo.png");

            SmtpClient smtp = new SmtpClient(_environmentApiSettings.SMTPHost, _environmentApiSettings.SMTPPort);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(_environmentApiSettings.SMTPLogin, _environmentApiSettings.SMTPPassword);
            smtp.EnableSsl = _environmentApiSettings.SMTPEnableSSL;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Timeout = 20000;
            
            // Configurar para ignorar problemas de certificado SSL
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(_environmentApiSettings.SMTPEmailFrom, "SingleOne Tech");
            mail.To.Add(destinatario);
            mail.Subject = assunto;
            mail.Body = mensagemHtml;
            mail.IsBodyHtml = true;
            
            // Desabilitar tracking do Brevo/Sendinblue
            mail.Headers.Add("X-Mailer", "SingleOne Tech");
            mail.Headers.Add("X-Brevo-Track", "false");
            mail.Headers.Add("X-Sendinblue-Track", "false");
            mail.Headers.Add("X-Track", "false");
            mail.Headers.Add("X-No-Track", "true");
            mail.Headers.Add("X-Disable-Tracking", "true");
            mail.Headers.Add("List-Unsubscribe", "<mailto:unsubscribe@singleone.tech>");
            mail.Headers.Add("Precedence", "bulk");
            
            // Headers adicionais para desabilitar tracking
            mail.Headers.Add("X-Auto-Response-Suppress", "All");
            mail.Headers.Add("X-Precedence", "bulk");
            mail.Headers.Add("Auto-Submitted", "auto-generated");
            mail.Headers.Add("X-Report-Abuse", "Please report abuse here: abuse@singleone.tech");
            
            // Configurações específicas para evitar tracking
            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.None;
            
            // Headers específicos para Brevo/Sendinblue
            mail.Headers.Add("X-Brevo-No-Track", "true");
            mail.Headers.Add("X-Sendinblue-No-Track", "true");
            mail.Headers.Add("X-Email-Tracking", "disabled");
            mail.Headers.Add("X-Link-Tracking", "disabled");
            
            try
            {
                // Lendo a imagem em bytes
                byte[] imageData;
                using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    imageData = new byte[stream.Length];
                    stream.Read(imageData, 0, (int)stream.Length);
                }

                // Criar a view HTML com encoding correto
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                    mensagemHtml,
                    System.Text.Encoding.UTF8,
                    "text/html"
                );

                // Criar o LinkedResource para a imagem
                MemoryStream imageStream = new MemoryStream(imageData);
                LinkedResource imageResource = new LinkedResource(imageStream, "image/png");
                imageResource.ContentId = "imagem";
                imageResource.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                
                // Adicionar a imagem à view HTML
                htmlView.LinkedResources.Add(imageResource);
                
                // Adicionar a view HTML ao email
                mail.AlternateViews.Add(htmlView);

                if (anexo != null)
                {
                    mail.Attachments.Add(new Attachment(new MemoryStream(anexo), "Termo de entrega.pdf"));
                }

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEND MAIL] Erro ao enviar email para {destinatario}: {ex.Message}");
                throw;
            }
            finally
            {
                smtp.Dispose();
                mail.Dispose();
            }
        }
    }
}
