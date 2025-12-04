using SingleOneAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Repository.Interfaces
{
    public interface IEstoqueMinimoLinhaRepository
    {
        Task<List<EstoqueMinimoLinha>> ListarPorCliente(int clienteId);
        Task<EstoqueMinimoLinha> BuscarPorId(int id);
        Task<List<EstoqueLinhaAlertaVM>> ListarAlertasLinhas(int clienteId);
        Task<int> ContarAlertasLinhas(int clienteId);
    }
}
