using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Vwlaudo
    {
        public int? Id { get; set; }
        public int? Cliente { get; set; }
        public string Equipamento { get; set; }
        public string Numeroserie { get; set; }
        public string Patrimonio { get; set; }
        public string Descricao { get; set; }
        public string Laudo { get; set; }
        public DateTime? Dtentrada { get; set; }
        public DateTime? Dtlaudo { get; set; }
        public bool? Mauuso { get; set; }
        public bool? Temconserto { get; set; }
        public int? Usuario { get; set; }
        public string Usuarionome { get; set; }
        public int? Tecnico { get; set; }
        public string Tecniconome { get; set; }
        public decimal? Valormanutencao { get; set; }
        public int? Empresa { get; set; }
        public string Empresanome { get; set; }
        public int? Centrocusto { get; set; }
        public string Centrocustonome { get; set; }
    }
}
