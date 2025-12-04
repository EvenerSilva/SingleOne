using SingleOneIntegrator.Models;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Repository.Integracao
{
    /// <summary>
    /// Interface para repositório de ClienteIntegracao
    /// </summary>
    public interface IClienteIntegracaoRepository
    {
        Task<ClienteIntegracao?> GetByApiKeyAsync(string apiKey);
        Task<ClienteIntegracao?> GetByClienteIdAsync(int clienteId);
        Task<ClienteIntegracao> CreateAsync(ClienteIntegracao entity);
        Task UpdateAsync(ClienteIntegracao entity);
        Task UpdateUltimaSincronizacaoAsync(int id);
    }
}


