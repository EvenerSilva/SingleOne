using SingleOneAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IEstoqueMinimoNegocio
    {
        // =====================================================
        // MÉTODOS PARA EQUIPAMENTOS
        // =====================================================
        
        /// <summary>
        /// Lista todas as configurações de estoque mínimo de equipamentos
        /// </summary>
        Task<List<EstoqueMinimoEquipamento>> ListarEquipamentos(int clienteId);
        
        /// <summary>
        /// Lista todas as configurações de estoque mínimo de equipamentos com dados calculados dinamicamente
        /// </summary>
        Task<List<EstoqueMinimoEquipamentoDTO>> ListarEquipamentosComDadosCalculados(int clienteId);
        
        /// <summary>
        /// Busca configuração de estoque mínimo de equipamento por ID
        /// </summary>
        Task<EstoqueMinimoEquipamento> BuscarEquipamento(int id);
        
        /// <summary>
        /// Salva ou atualiza configuração de estoque mínimo de equipamento
        /// </summary>
        Task SalvarEquipamento(EstoqueMinimoEquipamento estoqueMinimo);
        
        /// <summary>
        /// Exclui configuração de estoque mínimo de equipamento
        /// </summary>
        Task ExcluirEquipamento(int id);

        // =====================================================
        // MÉTODOS PARA LINHAS TELEFÔNICAS
        // =====================================================
        
        /// <summary>
        /// Lista todas as configurações de estoque mínimo de linhas telefônicas
        /// </summary>
        Task<List<EstoqueMinimoLinha>> ListarLinhas(int clienteId);
        
        /// <summary>
        /// Busca configuração de estoque mínimo de linha por ID
        /// </summary>
        Task<EstoqueMinimoLinha> BuscarLinha(int id);
        
        /// <summary>
        /// Salva ou atualiza configuração de estoque mínimo de linha
        /// </summary>
        Task SalvarLinha(EstoqueMinimoLinha estoqueMinimo);
        
        /// <summary>
        /// Exclui configuração de estoque mínimo de linha
        /// </summary>
        Task ExcluirLinha(int id);

        // =====================================================
        // MÉTODOS PARA RELATÓRIOS E ALERTAS
        // =====================================================
        
        /// <summary>
        /// Lista alertas de estoque baixo (consolidado)
        /// </summary>
        Task<List<EstoqueAlertaVM>> ListarAlertas(int clienteId);
        
        /// <summary>
        /// Lista alertas específicos de equipamentos
        /// </summary>
        Task<List<EstoqueEquipamentoAlertaVM>> ListarAlertasEquipamentos(int clienteId);
        
        /// <summary>
        /// Lista alertas específicos de linhas telefônicas
        /// </summary>
        Task<List<EstoqueLinhaAlertaVM>> ListarAlertasLinhas(int clienteId);
        
        /// <summary>
        /// Conta total de alertas por cliente
        /// </summary>
        Task<int> ContarAlertas(int clienteId);
    }
}
