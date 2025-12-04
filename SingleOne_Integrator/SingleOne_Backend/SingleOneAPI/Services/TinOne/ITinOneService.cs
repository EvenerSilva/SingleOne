using SingleOneAPI.DTOs.TinOne;
using System.Threading.Tasks;

namespace SingleOneAPI.Services.TinOne
{
    /// <summary>
    /// Interface para serviço principal do TinOne
    /// </summary>
    public interface ITinOneService
    {
        /// <summary>
        /// Processa uma pergunta do usuário
        /// </summary>
        Task<TinOneRespostaDTO> ProcessarPerguntaAsync(TinOnePerguntaDTO pergunta);

        /// <summary>
        /// Obtém informações sobre um campo específico
        /// </summary>
        Task<TinOneCampoInfoDTO?> GetCampoInfoAsync(string campoId);

        /// <summary>
        /// Obtém um processo guiado
        /// </summary>
        Task<TinOneProcessoDTO?> GetProcessoAsync(string processoId);

        /// <summary>
        /// Registra analytics de uso
        /// </summary>
        Task RegistrarAnalyticsAsync(int? usuarioId, int? clienteId, string? sessaoId, 
            string? paginaUrl, string? paginaNome, string? acaoTipo, 
            string? pergunta, string? resposta, int? tempoRespostaMs);

        /// <summary>
        /// Registra feedback do usuário
        /// </summary>
        Task RegistrarFeedbackAsync(TinOneFeedbackDTO feedback);
    }
}

