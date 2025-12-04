using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Equipamentoanexo
    {
        public int Id { get; set; }
        public int Equipamento { get; set; }
        public int Usuario { get; set; }
        public int? Laudo { get; set; }
        public string Arquivo { get; set; }
        public string Nome { get; set; }
        public bool Isbo { get; set; }
        public bool Islaudo { get; set; }
        public DateTime Dtregistro { get; set; }

        public virtual Equipamento EquipamentoNavigation { get; set; }
        public virtual Usuario UsuarioNavigation { get; set; }
    }
}
