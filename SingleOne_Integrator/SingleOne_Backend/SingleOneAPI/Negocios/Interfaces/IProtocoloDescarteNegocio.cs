using SingleOneAPI.Models;
using SingleOneAPI.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios.Interfaces
{
    /// <summary>
    /// Interface para lógica de negócio de Protocolo de Descarte
    /// </summary>
    public interface IProtocoloDescarteNegocio
    {
        /// <summary>
        /// Listar protocolos de descarte de um cliente
        /// </summary>
        Task<List<ProtocoloDescarteVM>> ListarProtocolos(int clienteId, bool incluirInativos = false);

        /// <summary>
        /// Obter protocolo específico por ID
        /// </summary>
        Task<ProtocoloDescarteVM> ObterProtocolo(int protocoloId);

        /// <summary>
        /// Criar novo protocolo de descarte
        /// </summary>
        Task<ProtocoloDescarteVM> CriarProtocolo(ProtocoloDescarteVM protocolo, int usuarioId);

        /// <summary>
        /// Atualizar protocolo existente
        /// </summary>
        Task<ProtocoloDescarteVM> AtualizarProtocolo(ProtocoloDescarteVM protocolo, int usuarioId);

        /// <summary>
        /// Adicionar equipamento ao protocolo
        /// </summary>
        Task<ProtocoloDescarteItemVM> AdicionarEquipamento(int protocoloId, int equipamentoId, int usuarioId);

        /// <summary>
        /// Remover equipamento do protocolo
        /// </summary>
        Task<bool> RemoverEquipamento(int protocoloId, int equipamentoId, int usuarioId);

        /// <summary>
        /// Atualizar um processo específico de um item
        /// </summary>
        Task<ProtocoloDescarteItemVM> AtualizarProcessoItem(int itemId, string processo, bool valor, int usuarioId);

        /// <summary>
        /// Atualizar campo específico de um item (método sanitização, ferramenta, observações)
        /// </summary>
        Task AtualizarCampoItem(int itemId, string campo, string valor, int usuarioId);

        /// <summary>
        /// Atualizar status de processo de um equipamento no protocolo
        /// </summary>
        Task<ProtocoloDescarteItemVM> AtualizarProcessoEquipamento(int itemId, bool sanitizacao, bool descaracterizacao, bool perfuracao, bool evidencias, int usuarioId);

        /// <summary>
        /// Finalizar protocolo (quando todos os equipamentos estão prontos)
        /// </summary>
        Task<ProtocoloDescarteVM> FinalizarProtocolo(int protocoloId, int usuarioId);

        /// <summary>
        /// Cancelar protocolo
        /// </summary>
        Task<bool> CancelarProtocolo(int protocoloId, int usuarioId);

        /// <summary>
        /// Gerar número de protocolo único
        /// </summary>
        Task<string> GerarNumeroProtocolo();

        /// <summary>
        /// Validar se protocolo pode ser finalizado
        /// </summary>
        Task<bool> ValidarFinalizacaoProtocolo(int protocoloId);

        /// <summary>
        /// Listar equipamentos disponíveis para adicionar ao protocolo
        /// </summary>
        Task<List<EquipamentoDisponivelVM>> ListarEquipamentosDisponiveis(int clienteId, string filtro = "");

        /// <summary>
        /// Obter estatísticas do protocolo
        /// </summary>
        Task<EstatisticasProtocoloVM> ObterEstatisticas(int protocoloId);

        /// <summary>
        /// Gerar documento PDF de descarte
        /// </summary>
        Task<byte[]> GerarDocumentoDescarte(int protocoloId, int usuarioId);
    }
}
