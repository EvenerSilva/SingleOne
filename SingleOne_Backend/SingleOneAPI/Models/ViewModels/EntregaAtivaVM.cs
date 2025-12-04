using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    public class EntregaAtivaVM
    {
        public string Colaborador { get; set; }
        public int ColaboradorId { get; set; }
        public string Matricula { get; set; } // 🆔 Matrícula do colaborador
        public bool AssinouUltimaRequisicao { get; set; }
        public List<RequisicaoColaboradorVM> RequisicoesColaborador { get; set; }
    }

    public class RequisicaoColaboradorVM
    {
        public int RequisicaoId { get; set; }
        public string Requisitante { get; set; }
        public string TecnicoResponsavel { get; set; }
        public DateTime DTSolicitacao { get; set; }
        public DateTime? DTProcessamento { get; set; }
        public List<EquipamentoRequisicaoVM> EquipamentosRequisicao { get; set; }
    }

    public class EquipamentoRequisicaoVM
    {
        public int RequisicaoItemId { get; set; }
        public int EquipamentoId { get; set; }
        public string Equipamento { get; set; }
        public string NumeroSerie { get; set; }
        public string Patrimonio { get; set; }
        public DateTime? DTProgramadaRetorno { get; set; }
        public string ObservacaoEntrega { get; set; }
        public int? Usuariodevolucaoid { get; set; }
        // ✅ NOVO: Suporte a linhas telefônicas (datas e técnico de entrega)
        public DateTime? DtEntrega { get; set; }
        public string UsuarioEntregaNome { get; set; }
        // ✅ NOVO: Data de solicitação para linhas telefônicas
        public DateTime? DtSolicitacao { get; set; }
    }
}
