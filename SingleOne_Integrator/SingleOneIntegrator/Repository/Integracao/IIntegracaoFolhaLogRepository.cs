using SingleOneIntegrator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Repository.Integracao
{
    /// <summary>
    /// Interface para repositório de IntegracaoFolhaLog
    /// </summary>
    public interface IIntegracaoFolhaLogRepository
    {
        Task<IntegracaoFolhaLog> CreateAsync(IntegracaoFolhaLog entity);
        Task<IEnumerable<IntegracaoFolhaLog>> GetByClienteIdAsync(int clienteId, int limit = 100);
        Task<IntegracaoFolhaLog?> GetByIntegracaoIdAsync(string integracaoId);
    }
}


