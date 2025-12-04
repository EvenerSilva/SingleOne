using System;
using SingleOneIntegrator.Helpers;

namespace SingleOneIntegrator.Tools
{
    /// <summary>
    /// Utilitário para gerar API Keys e Secrets
    /// Executar: dotnet run --project SingleOneIntegrator -- generate-keys [--production]
    /// </summary>
    public class ApiKeyGeneratorTool
    {
        public static void Generate(bool isProduction = false)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("  SingleOne - Gerador de API Keys");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            var apiKey = ApiKeyGenerator.GenerateApiKey(isProduction);
            var apiSecret = ApiKeyGenerator.GenerateApiSecret();

            Console.WriteLine($"Ambiente: {(isProduction ? "PRODUÇÃO" : "TESTE")}");
            Console.WriteLine();
            Console.WriteLine("📝 Credenciais geradas:");
            Console.WriteLine();
            Console.WriteLine($"API Key:    {apiKey}");
            Console.WriteLine($"API Secret: {apiSecret}");
            Console.WriteLine();
            Console.WriteLine("⚠️  IMPORTANTE:");
            Console.WriteLine("   - Guarde o API Secret com segurança");
            Console.WriteLine("   - Nunca compartilhe em código ou logs públicos");
            Console.WriteLine("   - Use variáveis de ambiente para armazenar");
            Console.WriteLine();
            Console.WriteLine("📋 SQL para inserir no banco:");
            Console.WriteLine();
            Console.WriteLine("INSERT INTO \"ClienteIntegracao\" ");
            Console.WriteLine("(\"ClienteId\", \"ApiKey\", \"ApiSecret\", \"Ativo\", \"DataCriacao\", \"Observacoes\")");
            Console.WriteLine("VALUES ");
            Console.WriteLine($"([CLIENTE_ID], '{apiKey}', '{apiSecret}', true, NOW(), 'Gerado automaticamente');");
            Console.WriteLine();
            Console.WriteLine("==============================================");
        }

        public static void ShowHelp()
        {
            Console.WriteLine("Uso: dotnet run -- generate-keys [opções]");
            Console.WriteLine();
            Console.WriteLine("Opções:");
            Console.WriteLine("  --production    Gera chaves de produção (sk_live_)");
            Console.WriteLine("  --test          Gera chaves de teste (sk_test_) [padrão]");
            Console.WriteLine("  --help          Mostra esta ajuda");
            Console.WriteLine();
            Console.WriteLine("Exemplos:");
            Console.WriteLine("  dotnet run -- generate-keys");
            Console.WriteLine("  dotnet run -- generate-keys --production");
        }
    }
}


