using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    public class VisualizarNotaFiscalVM
    {
        public int FornecedorId { get; set; }
        public string Fornecedor { get; set; }
        public int Numero { get; set; }
        public DateTime Dtemissao { get; set; }
        public string Gerouequipamento { get; set; }
        public string Descricao { get; set; }
        public string Valor { get; set; }
        public int QuantidadeItens { get; set; }
        public List<VisualizarNotaFiscalItem> Itens { get; set; }
    }

    public class VisualizarNotaFiscalItem
    {
        public int Id { get; set; }
        public int Notafiscal { get; set; }
        public string Tipoequipamento { get; set; }
        public int TipoequipamentoId { get; set; }
        public string Fabricante { get; set; }
        public int FabricanteId { get; set; }
        public string Modelo { get; set; }
        public int ModeloId { get; set; }
        public int Quantidade { get; set; }
        public string Valorunitario { get; set; }
        public string TipoAquisicao { get; set; }
        public DateTime? Dtlimitegarantia { get; set; }
        public string Contrato { get; set; }
    }
}
