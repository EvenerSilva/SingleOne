using SingleOneAPI.Models.TinOne;
using SingleOneAPI.DTOs.TinOne;
using System.Collections.Generic;

namespace SingleOneAPI.Services.TinOne
{
    /// <summary>
    /// Interface para serviço de configuração do TinOne
    /// </summary>
    public interface ITinOneConfigService
    {
        /// <summary>
        /// Obtém a configuração do TinOne para um cliente específico ou global
        /// </summary>
        TinOneConfig GetConfig(int? clienteId = null);

        /// <summary>
        /// Verifica se o TinOne está habilitado
        /// </summary>
        bool IsEnabled(int? clienteId = null);

        /// <summary>
        /// Verifica se uma funcionalidade específica está habilitada
        /// </summary>
        bool IsFeatureEnabled(string featureName, int? clienteId = null);

        /// <summary>
        /// Obtém todas as configurações do TinOne
        /// </summary>
        List<TinOneConfigItemDTO> GetAllConfigs(int? clienteId = null);

        /// <summary>
        /// Salva as configurações do TinOne
        /// </summary>
        void SaveConfigs(List<TinOneConfigItemDTO> configuracoes);
    }
}

