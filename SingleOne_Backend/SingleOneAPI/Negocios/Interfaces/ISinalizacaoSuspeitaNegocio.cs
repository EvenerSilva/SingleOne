using SingleOneAPI.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios.Interfaces
{
    /// <summary>
    /// Interface para lógica de negócio de sinalizações de suspeitas
    /// </summary>
    public interface ISinalizacaoSuspeitaNegocio
    {
        /// <summary>
        /// Criar nova sinalização de suspeita
        /// </summary>
        Task<SinalizacaoCriadaDTO> CriarSinalizacaoAsync(CriarSinalizacaoDTO dto);

        /// <summary>
        /// Listar sinalizações com filtros e paginação
        /// </summary>
        Task<SinalizacoesPaginadasDTO> ListarSinalizacoesAsync(FiltroSinalizacoesDTO filtros);

        /// <summary>
        /// Obter detalhes de uma sinalização específica
        /// </summary>
        Task<SinalizacaoDetalhesDTO?> ObterDetalhesAsync(int id);

        /// <summary>
        /// Atualizar status de uma sinalização
        /// </summary>
        Task<bool> AtualizarStatusAsync(AtualizarStatusSinalizacaoDTO dto);

        /// <summary>
        /// Resolver uma sinalização (finalizar investigação)
        /// </summary>
        Task<bool> ResolverSinalizacaoAsync(ResolverSinalizacaoDTO dto);

        /// <summary>
        /// Obter motivos de suspeita disponíveis
        /// </summary>
        Task<List<MotivoSuspeitaDTO>> ObterMotivosSuspeitaAsync();

        /// <summary>
        /// Obter estatísticas de sinalizações
        /// </summary>
        Task<EstatisticasSinalizacoesDTO> ObterEstatisticasAsync();

        /// <summary>
        /// Atribuir sinalização para investigador
        /// </summary>
        Task<bool> AtribuirInvestigadorAsync(int sinalizacaoId, int investigadorId);

        /// <summary>
        /// Arquivar sinalização
        /// </summary>
        Task<bool> ArquivarSinalizacaoAsync(int id);
    }
}
