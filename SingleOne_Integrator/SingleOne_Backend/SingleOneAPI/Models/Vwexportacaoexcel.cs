using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Vwexportacaoexcel
    {
        public int? Id { get; set; }
        public string Colaborador { get; set; }
        public string Cargo { get; set; }
        public string Tipoequipamento { get; set; }
        public string Fabricante { get; set; }
        public string Modelo { get; set; }
        public string Notafiscal { get; set; }
        public string Equipamentostatus { get; set; }
        public int? Equipamentostatusid { get; set; }
        public string Usuariocadastro { get; set; }
        public string Localizacao { get; set; }
        public string Possuibo { get; set; }
        public string Descricaobo { get; set; }
        public string Numeroserie { get; set; }
        public string Patrimonio { get; set; }
        public DateTime? Dtcadastro { get; set; }
        public string TipoAquisicao { get; set; }
        public int? Cliente { get; set; }
        public bool? Ativo { get; set; }
        public string Empresa { get; set; }
        public string Centrocusto { get; set; }
    }
}
