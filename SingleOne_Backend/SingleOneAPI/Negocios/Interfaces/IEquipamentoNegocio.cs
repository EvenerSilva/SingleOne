using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOne.Util;
using System.Collections.Generic;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IEquipamentoNegocio
    {
        PagedResult<Equipamentovm> ListarEquipamentos(string pesquisa, int cliente, int? contrato, int page, int pageSize, int? modeloId = null, int? localidadeId = null);
        List<Equipamentovm> ListarTodosEquipamentosParaResumo(int cliente);
        List<Equipamentovm> ListarEquipamentosDisponiveis(string pesquisa, int cliente);
        List<Equipamentovm> ListarEquipamentoDisponivelParaLaudos(string pesquisa, int cliente);
        List<Equipamentovm> ListarEquipamentosDisponiveisParaEstoque(int cliente);
        List<Equipamentosstatus> ListarStatusEquipamentos();
        Equipamento BuscarEquipamentoPorId(int id);
        PagedResult<Equipamentovm> BuscarEquipamentoPorNumeroSeriePatrimonio(int cliente, string numeroSerie);
        string SalvarEquipamento(Equipamento eq);
        void ExcluirEquipamento(int id);
        void IncluirAnexo(Equipamentoanexo anexo);
        void ExcluirAnexo(int id);
        List<Equipamentoanexo> AnexosDoEquipamento(int idEquipamento);
        int LiberarParaEstoque(int idUsuario, int idEquipamento);
        void RegistrarBO(Equipamento eqp);
        List<Termoentregavm> EquipamentosDoTermoDeEntrega(int cliente, int idColaborador, bool byod = false);
        List<Vwexportacaoexcel> ExportarParaExcel(int cliente);
        void NotificarRH(Equipamento eqpto, bool perda);
        List<DescarteVM> ListarEquipamentosDisponiveisParaDescarte(int cliente, string pesquisa);
        void RealizarDescarte(List<DescarteVM> descartes);
        void ReativarEquipamento(int id);
        Equipamento VisualizarRecurso(int id);
    }

}
