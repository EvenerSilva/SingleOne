using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SingleOneAPI.Services.TinOne
{
    /// <summary>
    /// Serviço de integração com Ollama para processamento de IA/NLP
    /// </summary>
    public interface IOllamaService
    {
        Task<string> GerarRespostaAsync(string pergunta, string contexto);
        Task<bool> VerificarDisponibilidadeAsync();
    }

    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaService> _logger;
        private readonly string _ollamaUrl;
        private readonly string _modelo;

        public OllamaService(ILogger<OllamaService> logger)
        {
            _logger = logger;
            _ollamaUrl = "http://localhost:11434"; // Porta padrão do Ollama
            _modelo = "llama3.2:3b"; // Modelo padrão
            
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5) // Timeout curto para verificações rápidas
            };
        }

        /// <summary>
        /// Verifica se o Ollama está disponível e rodando
        /// </summary>
        public async Task<bool> VerificarDisponibilidadeAsync()
        {
            try
            {
                // Timeout de apenas 2 segundos para verificação rápida
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(2));
                var response = await _httpClient.GetAsync($"{_ollamaUrl}/api/tags", cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[Ollama] Serviço não disponível (normal se não estiver instalado)");
                return false;
            }
        }

        /// <summary>
        /// Gera resposta usando o modelo de IA do Ollama
        /// </summary>
        public async Task<string> GerarRespostaAsync(string pergunta, string contexto)
        {
            try
            {
                _logger.LogInformation($"[Ollama] Gerando resposta para: {pergunta}");

                var prompt = ConstruirPrompt(pergunta, contexto);
                
                var requestBody = new
                {
                    model = _modelo,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3, // Reduzido para ser mais conservador e menos criativo (evita alucinações)
                        top_p = 0.9,
                        max_tokens = 500
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[Ollama] Erro na requisição: {response.StatusCode}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OllamaResponse>(responseJson);

                _logger.LogInformation($"[Ollama] ✅ Resposta gerada com sucesso");
                
                return result?.Response?.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Ollama] Erro ao gerar resposta");
                return null;
            }
        }

        /// <summary>
        /// Constrói o prompt para o modelo de IA com RAG (Retrieval-Augmented Generation)
        /// </summary>
        private string ConstruirPrompt(string pergunta, string contexto)
        {
            var prompt = new StringBuilder();
            
            prompt.AppendLine("Você é o Oni o Sábio 🦉, um assistente virtual especializado no sistema SingleOne de gestão de ativos de TI.");
            prompt.AppendLine();
            prompt.AppendLine("⚠️ **REGRAS CRÍTICAS - LEIA COM ATENÇÃO:**");
            prompt.AppendLine("1. APENAS responda com base no CONTEXTO fornecido abaixo");
            prompt.AppendLine("2. NUNCA invente funcionalidades que não estejam explicitamente mencionadas no contexto");
            prompt.AppendLine("3. NUNCA assuma recursos típicos de sistemas de gestão de TI se não estiverem no contexto");
            prompt.AppendLine("4. Se a pergunta não puder ser respondida com o contexto disponível, diga: 'Desculpe, não tenho informações específicas sobre isso na base de conhecimento.'");
            prompt.AppendLine("5. Seja honesto: se algo NÃO está no contexto, NÃO mencione");
            prompt.AppendLine("6. Responda em português do Brasil de forma objetiva e amigável");
            prompt.AppendLine("7. Use bullet points (•) e emojis ocasionalmente");
            prompt.AppendLine();
            prompt.AppendLine("❌ **PROIBIDO:**");
            prompt.AppendLine("- Inventar funcionalidades não mencionadas no contexto");
            prompt.AppendLine("- Assumir recursos genéricos de sistemas ITSM/ITAM");
            prompt.AppendLine("- Mencionar integrações, automações ou features que não estejam confirmadas");
            prompt.AppendLine();
            
            if (!string.IsNullOrEmpty(contexto))
            {
                prompt.AppendLine("**CONTEXTO (Base de Conhecimento):**");
                prompt.AppendLine(contexto);
                prompt.AppendLine();
            }
            
            prompt.AppendLine("**PERGUNTA DO USUÁRIO:**");
            prompt.AppendLine(pergunta);
            prompt.AppendLine();
            prompt.AppendLine("**SUA RESPOSTA:**");

            return prompt.ToString();
        }

        /// <summary>
        /// Classe para deserializar resposta do Ollama
        /// </summary>
        private class OllamaResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("response")]
            public string Response { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("done")]
            public bool Done { get; set; }
        }
    }
}

