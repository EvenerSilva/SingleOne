using System;

namespace SingleOne.Models
{
    public partial class Colaboradorhistoricovm
    {
        public int? Id { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Matricula { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }
        public string Situacao { get; set; }
        public string Situacaoantiga { get; set; }
        public DateTime? Dtatualizacao { get; set; }
        public int? Empresaatualid { get; set; }
        public string Empresaatual { get; set; }
        public int? Empresaantigaid { get; set; }
        public string Empresaantiga { get; set; }
        public DateTime? Dtatualizacaoempresa { get; set; }
        public int? Localidadeatualid { get; set; }
        public string Localidadeatual { get; set; }
        public int? Localidadeantigaid { get; set; }
        public string Localidadeantiga { get; set; }
        public DateTime? Dtatualizacaolocalidade { get; set; }
        public int? Centrocustoatualid { get; set; }
        public string Codigoccatual { get; set; }
        public string Nomeccatual { get; set; }
        public int? Centrocustoantigoid { get; set; }
        public string Codigoccantigo { get; set; }
        public string Nomeccantigo { get; set; }
        public DateTime? Dtatualizacaocentrocusto { get; set; }
        public int? Cliente { get; set; }
    }
}
