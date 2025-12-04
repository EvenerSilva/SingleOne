using System.Collections.Generic;
using SingleOne.Models;

namespace SingleOne.Models.ViewModels
{
    public class RequisicaoVM
    {
        public RequisicaoVM()
        {
            Requisicao = new Requisicoesvm();
            EquipamentosRequisicao = new List<Requisicaoequipamentosvm>();
            RequisicoesDoColaborador = new List<RequisicaoVM>();
            RequisicaoItens = new List<Requisicoesiten>();
        }
        public Requisicoesvm Requisicao { get; set; }
        public List<Requisicaoequipamentosvm> EquipamentosRequisicao { get; set; }
        public int ColaboradorId { get; set; }
        public string Colaborador { get; set; }

        public List<RequisicaoVM> RequisicoesDoColaborador { get; set; }
        public List<Requisicoesiten> RequisicaoItens { get; set; }
        public bool AssinadoEletronicamente { get; set; }
        public bool PodeEntregar { get; set; }
        public int UsuarioDevolucaoId { get; set; }
    }
}
