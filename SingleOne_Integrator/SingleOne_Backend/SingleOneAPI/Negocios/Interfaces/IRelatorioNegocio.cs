using SingleOne.Models.ViewModels;
using SingleOne.Models;
using SingleOneAPI.Models.ViewModels;
using System.Collections.Generic;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IRelatorioNegocio
    {
        List<Equipamentohistoricovm> HistoricoEquipamento(int id);
        List<Equipamentohistoricovm> HistoricoEquipamentoPorNumeroSerie(string numeroSerie); // ✅ NOVO
        List<RequisicaoVM> EquipamentosComColaboradores(int id);
        MovimentacoesVM MovimentacoesColaboradores(int cliente, int pagina, string relatorio, string pesquisa);
        List<Vwequipamentosdetalhe> ConsultarDetalhesEquipamentos(Vwequipamentosdetalhe vw);
        DashboardMobileVM DashboardMobile(int cliente);
        DashboardWebVM DashboardWeb(int cliente);
        (List<Vwlaudo>, List<LaudoVM>) LaudosComValor(int cliente, int empresa, int cc);
        List<Equipamentohistoricovm> ListarLinhasTelefonicas(string pesquisa, int cliente);
        List<LogAcessoVM> ConsultarLogsAcesso(LogAcessoFiltroVM filtros);
        List<GarantiaVM> ConsultarGarantias(GarantiaFiltroVM filtros);
        RelatorioNaoConformidadeResultVM ConsultarNaoConformidadeElegibilidade(RelatorioNaoConformidadeFiltroVM filtros);
        List<ColaboradoresSemRecursosVM> ConsultarColaboradoresSemRecursos(ColaboradoresSemRecursosFiltroVM filtros);
        List<dynamic> ObterEmpresasComColaboradores(int clienteId);
        List<dynamic> ObterLocalidadesComColaboradores(int clienteId);
        List<dynamic> ObterCentrosCustoComColaboradores(int clienteId);
        MapaRecursosVM ObterMapaRecursos(MapaRecursosFiltroVM filtros);
    }

}
