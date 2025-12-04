using System;

namespace SingleOneAPI.Models
{
    public class EstoqueAlertaVM
    {
        public string Tipo { get; set; }
        public int Cliente { get; set; }
        public string Localidade { get; set; }
        public string Descricao { get; set; }
        public int EstoqueAtual { get; set; }
        public int EstoqueMinimo { get; set; }
        public int EstoqueMaximo { get; set; }
        public int TotalLancado { get; set; }
        public int QuantidadeFaltante { get; set; }
        public int QuantidadeExcesso { get; set; }
        public decimal PercentualUtilizacao { get; set; }
        public string Status { get; set; }
        public string Prioridade { get; set; }
        public string? Observacoes { get; set; }
    }

    public class EstoqueEquipamentoAlertaVM
    {
        public int Cliente { get; set; }
        public string Localidade { get; set; }
        public string TipoEquipamento { get; set; }
        public string Fabricante { get; set; }
        public string Modelo { get; set; }
        public int EstoqueAtual { get; set; }
        public int EstoqueMinimo { get; set; }
        public int QuantidadeFaltante { get; set; }
        public string Status { get; set; }
    }

    public class EstoqueLinhaAlertaVM
    {
        public int Cliente { get; set; }
        public string Localidade { get; set; }
        public string Operadora { get; set; }
        public string Contrato { get; set; }
        public string Plano { get; set; }
        public string PerfilUso { get; set; }
        public int EstoqueAtual { get; set; }
        public int EstoqueMinimo { get; set; }
        public int QuantidadeFaltante { get; set; }
        public string Status { get; set; }
    }
}
