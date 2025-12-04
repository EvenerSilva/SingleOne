using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Vwequipamentosstatus
    {
        public int? Cliente { get; set; }
        public string Tipoequipamento { get; set; }
        public long? Danificado { get; set; }
        public long? Devolvido { get; set; }
        public long? Emestoque { get; set; }
        public long? Entregue { get; set; }
        public long? Extraviado { get; set; }
        public long? Novo { get; set; }
        public long? Requisitado { get; set; }
        public long? Roubado { get; set; }
        public long? Semconserto { get; set; }
        public long? Migrado { get; set; }
        public long? Descartado { get; set; }
    }
}
