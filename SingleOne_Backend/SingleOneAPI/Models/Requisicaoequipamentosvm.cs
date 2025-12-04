using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Requisicaoequipamentosvm
    {
        public int? Id { get; set; }
        public int? Requisicao { get; set; }
        public int? Equipamentoid { get; set; }
        public string Equipamento { get; set; }
        public string Numeroserie { get; set; }
        public string Patrimonio { get; set; }
        public int? Usuarioentregaid { get; set; }
        public string Usuarioentrega { get; set; }
        public int? Usuariodevolucaoid { get; set; }
        public string Usuariodevolucao { get; set; }
        public DateTime? Dtentrega { get; set; }
        public DateTime? Dtdevolucao { get; set; }
        public string Observacaoentrega { get; set; }
        public DateTime? Dtprogramadaretorno { get; set; }
        public int? Equipamentostatus { get; set; }
        public decimal? Numero { get; set; }
        public int? Linhaid { get; set; }
        public int TipoAquisicao { get; set; }
    }
}
