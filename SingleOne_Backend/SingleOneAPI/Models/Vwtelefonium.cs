using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Vwtelefonium
    {
        public string Operadora { get; set; }
        public string Contrato { get; set; }
        public string Plano { get; set; }
        public decimal? Valor { get; set; }
        public decimal? Numero { get; set; }
        public string Iccid { get; set; }
        public bool? Emuso { get; set; }
        public bool? Ativo { get; set; }
        public int? Cliente { get; set; }
    }
}
