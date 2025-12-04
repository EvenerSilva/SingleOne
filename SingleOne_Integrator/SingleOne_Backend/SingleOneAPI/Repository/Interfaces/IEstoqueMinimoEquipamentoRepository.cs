using SingleOneAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Repository.Interfaces
{
    public interface IEstoqueMinimoEquipamentoRepository
    {
        Task<List<EstoqueMinimoEquipamento>> ListarPorCliente(int clienteId);
        Task<List<EstoqueMinimoEquipamentoDTO>> ListarPorClienteComDadosCalculados(int clienteId);
        Task<EstoqueMinimoEquipamento> BuscarPorId(int id);
        Task<List<EstoqueAlertaVM>> ListarAlertasConsolidados(int clienteId);
        Task<List<EstoqueEquipamentoAlertaVM>> ListarAlertasEquipamentos(int clienteId);
        Task<int> ContarAlertasEquipamentos(int clienteId);
    }
}
