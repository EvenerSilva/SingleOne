using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Models.ViewModels
{
    public class DevolucaoVM
    {
        public int Id { get; set; }
        public string Requisitante { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public DateTime DataEntrega { get; set; }
        public int ColaboradorFinalId { get; set; }
        public string ColaboradorFinal { get; set; }
        public DateTime? DevolucaoProgramada { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public int RequisicaoStatus { get; set; }
        public List<Requisicoesiten> Equipamentos { get; set; }
    }
}
