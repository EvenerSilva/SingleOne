using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Termoentregavm
    {
        public string Tipoequipamento { get; set; }
        public string Fabricante { get; set; }
        public string Modelo { get; set; }
        public string Numeroserie { get; set; }
        public string Patrimonio { get; set; }
        public DateTime? Dtentrega { get; set; }
        public string Observacaoentrega { get; set; }
        public DateTime? Dtprogramadaretorno { get; set; }
        public string Hashrequisicao { get; set; }
        public int? Colaboradorfinal { get; set; }
        public int? Cliente { get; set; }
        public int TipoAquisicao { get; set; }
    }
}
