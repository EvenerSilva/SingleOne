using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Equipamentovm
    {
        public int? Id { get; set; }
        public int? Tipoequipamentoid { get; set; }
        public string Tipoequipamento { get; set; }
        public int? Fabricanteid { get; set; }
        public string Fabricante { get; set; }
        public int? Modeloid { get; set; }
        public string Modelo { get; set; }
        public int? Notafiscalid { get; set; }
        public string Notafiscal { get; set; }
        public int? Equipamentostatusid { get; set; }
        public string Equipamentostatus { get; set; }
        public int? Usuarioid { get; set; }
        public string Usuario { get; set; }
        public int? Localizacaoid { get; set; }
        public string Localizacao { get; set; }
        public bool? Possuibo { get; set; }
        public string Descricaobo { get; set; }
        public string Numeroserie { get; set; }
        public string Patrimonio { get; set; }
        public DateTime? Dtlimitegarantia { get; set; }
        public DateTime? Dtcadastro { get; set; }
        public int TipoAquisicao { get; set; }
        public string TipoAquisicaoNome { get; set; }
        public int? Fornecedor { get; set; }
        public string FornecedorNome { get; set; }
        public int? Cliente { get; set; }
        public int? Colaboradorid { get; set; }
        public string Colaboradornome { get; set; }
        public int? Requisicaoid { get; set; }
        public bool? Ativo { get; set; }
        public int? Empresaid { get; set; }
        public string Empresa { get; set; }
        public int? Centrocustoid { get; set; }
        public string Centrocusto { get; set; }
        public int? Contratoid { get; set; }
        public string Contrato { get; set; }
        public int? Filialid { get; set; }
        public string Filial { get; set; }
    }
}
