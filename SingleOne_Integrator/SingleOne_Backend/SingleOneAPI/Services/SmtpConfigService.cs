using Microsoft.EntityFrameworkCore;
using System;
using SingleOne.Models;
using SingleOneAPI.Infra.Repositorio;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Services
{
    public class SmtpConfigService : ISmtpConfigService
    {
        private readonly IRepository<Parametro> _parametroRepository;

        public SmtpConfigService(IRepository<Parametro> parametroRepository)
        {
            _parametroRepository = parametroRepository;
        }

        public async Task LoadSmtpSettingsFromDatabase(EnvironmentApiSettings environmentApiSettings, int clienteId)
        {
            try
            {
                Console.WriteLine($"[SMTP_CONFIG] Iniciando carregamento de configurações SMTP para cliente: {clienteId}");
                
                // Buscar parâmetros do cliente no banco de dados
                var parametro = await _parametroRepository.Buscar(x => x.Cliente == clienteId).FirstOrDefaultAsync();
                
                Console.WriteLine($"[SMTP_CONFIG] Parâmetro encontrado: {(parametro != null ? "Sim" : "Não")}");
                
                if (parametro != null)
                {
                    Console.WriteLine($"[SMTP_CONFIG] Configurações encontradas:");
                    Console.WriteLine($"[SMTP_CONFIG] - Host: {parametro.SmtpHost}");
                    Console.WriteLine($"[SMTP_CONFIG] - Port: {parametro.SmtpPort}");
                    Console.WriteLine($"[SMTP_CONFIG] - Login: {parametro.SmtpLogin}");
                    Console.WriteLine($"[SMTP_CONFIG] - From: {parametro.SmtpEmailFrom}");
                    Console.WriteLine($"[SMTP_CONFIG] - SSL: {parametro.SmtpEnableSSL}");
                    Console.WriteLine($"[SMTP_CONFIG] - Enabled: {parametro.SmtpEnabled}");
                    
                    // Atualizar as configurações SMTP com os valores do banco
                    environmentApiSettings.UpdateSmtpSettings(
                        parametro.SmtpHost,
                        parametro.SmtpPort,
                        parametro.SmtpLogin,
                        parametro.SmtpPassword,
                        parametro.SmtpEmailFrom,
                        parametro.SmtpEnableSSL,
                        parametro.SmtpEnabled
                    );
                    
                    Console.WriteLine($"[SMTP_CONFIG] Configurações SMTP atualizadas com sucesso");
                }
                else
                {
                    Console.WriteLine($"[SMTP_CONFIG] AVISO: Nenhum parâmetro SMTP encontrado para cliente {clienteId}");
                }
            }
            catch (System.Exception ex)
            {
                // Em caso de erro, manter as configurações padrão
                System.Console.WriteLine($"[SMTP CONFIG] Erro ao carregar configurações do banco: {ex.Message}");
            }
        }
    }
}
