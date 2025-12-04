using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Equipamentohistoricovm
    {
        public int? Id { get; set; }
        public int? Equipamentoid { get; set; }
        public int? Tipoequipamentoid { get; set; }
        public string Tipoequipamento { get; set; }
        public int? Fabricanteid { get; set; }
        public string Fabricante { get; set; }
        public int? Modeloid { get; set; }
        public string Modelo { get; set; }
        public string Numeroserie { get; set; }
        public string Patrimonio { get; set; }
        public int? Equipamentostatusid { get; set; }
        public string Equipamentostatus { get; set; }
        public int? Colaboradorid { get; set; }
        public string Colaborador { get; set; }
        public DateTime? Dtregistro { get; set; }
        public int? Usuarioid { get; set; }
        public string Usuario { get; set; }
        public int? Tecnicoresponsavelid { get; set; }
        public string Tecnicoresponsavel { get; set; }
    }
}
