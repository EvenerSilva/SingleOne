using SingleOne.Models.ViewModels;
using SingleOne.Models;
using SingleOne.Util;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IRequisicoesNegocio
    {
        PagedResult<RequisicaoVM> ListarRequisicoes(string pesquisa, int cliente, int pagina);
        RequisicaoVM BuscarRequisicaoPorId(int id);
        RequisicaoVM ListarEquipamentosDaRequisicao(string hash, bool byod);
        string SalvarRequisicao(Requisico requisicaoSalvar);
        Task<string> AceitarTermoEntrega(TermoEletronicoVM vm);
        List<RequisicaoVM> ListarEntregasDisponiveis(int cliente);
        Requisico BuscarEntregasDisponiveisPorID(int requisicao);
        void RealizarEntrega(Requisico req);
        PagedResult<EntregaAtivaVM> ListarDevolucoesDisponiveis(string pesquisa, int cliente, int pagina, bool byod = false);
        List<UltimaRequisicaoDTO> ObterUltimasRequisicoesColaborador(int cliente, int colaborador, bool byod);
        void AtualizarItemRequisicao(Requisicaoequipamentosvm rivm);
        void AdicionarObservacaoEquipamentoVM(EquipamentoRequisicaoVM equipamentosViewModel);
        void AdicionarAgendamentoEquipamentoVM(EquipamentoRequisicaoVM equipamento);
        void RealizarDevolucaoEquipamento(EquipamentoRequisicaoVM equipamento);
        void RealizarDevolucoesDoColaborador(int idColaborador, int usuarioDevolucao, bool byod);
        string RealizarEntregaMobile(Requisico req);
        void TransferenciaEquipamento(TransferenciaEqpVM vm);

        // Compartilhamentos por item de requisição (co-responsáveis)
        System.Collections.Generic.List<SingleOneAPI.Models.RequisicaoItemCompartilhado> ListarCompartilhadosItem(int requisicaoItemId);
        SingleOneAPI.Models.RequisicaoItemCompartilhado AdicionarCompartilhadoItem(int requisicaoItemId, SingleOneAPI.Models.RequisicaoItemCompartilhado vinculo, int usuarioId);
        SingleOneAPI.Models.RequisicaoItemCompartilhado AtualizarCompartilhadoItem(int vinculoId, SingleOneAPI.Models.RequisicaoItemCompartilhado vinculo);
        void EncerrarCompartilhadoItem(int vinculoId, int usuarioId);

        // Entrega com co-responsáveis (somente NÃO-BYOD)
        void RealizarEntregaComCompartilhados(SingleOneAPI.Models.DTO.RequisicaoDTO dto);
    }

}
