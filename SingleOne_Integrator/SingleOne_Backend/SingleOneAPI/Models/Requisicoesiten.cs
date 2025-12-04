using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Requisicoesiten
    {
        public int Id { get; set; }
        public int Requisicao { get; set; }
        public int? Equipamento { get; set; }
        public int? Linhatelefonica { get; set; }
        public int? Usuarioentrega { get; set; }
        public int? Usuariodevolucao { get; set; }
        public DateTime? Dtentrega { get; set; }
        public DateTime? Dtdevolucao { get; set; }
        public string Observacaoentrega { get; set; } = string.Empty;
        public DateTime? Dtprogramadaretorno { get; set; }

        public virtual Equipamento EquipamentoNavigation { get; set; } = null!;
        public virtual Telefonialinha LinhatelefonicaNavigation { get; set; } = null!;
        public virtual Requisico RequisicaoNavigation { get; set; } = null!;
    }
}
