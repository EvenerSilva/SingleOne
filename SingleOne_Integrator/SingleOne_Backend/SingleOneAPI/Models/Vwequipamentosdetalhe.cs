using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOne.Models
{
    public partial class Vwequipamentosdetalhe
    {
        public int? Id { get; set; }
        public int? Cliente { get; set; }
        public int? Tipoequipamentoid { get; set; }
        public string Tipoequipamento { get; set; }
        public int? Fabricanteid { get; set; }
        public string Fabricante { get; set; }
        public int? Modeloid { get; set; }
        public string Modelo { get; set; }
        public int? Equipamentostatusid { get; set; }
        public string Equipamentostatus { get; set; }
        public int? Localidadeid { get; set; }
        public string Localidade { get; set; }
        public string Numeroserie { get; set; }
        public string Patrimonio { get; set; }
        public int? Empresaid { get; set; }
        public string Empresa { get; set; }
        public int? Centrocustoid { get; set; }
        public string Centrocusto { get; set; }
        // Campos auxiliares para filtros não materializados diretamente na view
        [NotMapped]
        public int? Tipoaquisicao { get; set; }
        [NotMapped]
        public int? Categoriaid { get; set; }
    }
}
