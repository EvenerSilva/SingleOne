using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Equipamentohistorico
    {
        public int Id { get; set; }
        public int Equipamento { get; set; }
        public int Equipamentostatus { get; set; }
        public int Usuario { get; set; }
        public int? Linhatelefonica { get; set; }
        public bool? Linhaemuso { get; set; }
        public int? Requisicao { get; set; }
        public int? Colaborador { get; set; }
        public DateTime Dtregistro { get; set; }

        public virtual Equipamento EquipamentoNavigation { get; set; }
        public virtual Equipamentosstatus EquipamentostatusNavigation { get; set; }
        public virtual Usuario UsuarioNavigation { get; set; }
    }
}
