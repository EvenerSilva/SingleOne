using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Laudo
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public int Equipamento { get; set; }
        public int Usuario { get; set; }
        public int Tecnico { get; set; }
        public string Descricao { get; set; }
        public string Laudo1 { get; set; }
        public DateTime Dtentrada { get; set; }
        public DateTime? Dtlaudo { get; set; }
        public bool Temconserto { get; set; }
        public bool Mauuso { get; set; }
        public bool Ativo { get; set; }
        public decimal? Valormanutencao { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Equipamento EquipamentoNavigation { get; set; }
        public virtual Usuario TecnicoNavigation { get; set; }
        public virtual Usuario UsuarioNavigation { get; set; }
    }
}
