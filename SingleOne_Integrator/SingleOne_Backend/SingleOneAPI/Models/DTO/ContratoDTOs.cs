using System;

namespace SingleOneAPI.Models.DTO
{
    public class ContratoDTO
    {
        public int Id { get; set; }
        public string Fornecedor { get; set; }
        public int FornecedorId { get; set; }
        public int Numero { get; set; }
        public int Aditivo { get; set; }
        public string Descricao { get; set; }
        public DateTime DTInicioVigencia { get; set; }
        public DateTime? DTFinalVigencia { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; }
        public int QtdeRecursos { get; set; }
        public bool Renovavel { get; set; }
        public int? DiasParaVencimento { get; set; }
        public string ArquivoContrato { get; set; }
        public string NomeArquivoOriginal { get; set; }
        public DateTime? DataUploadArquivo { get; set; }
        public bool TemArquivo => !string.IsNullOrEmpty(ArquivoContrato);
    }

    public class CriarNovoContrato
    {
        public int Cliente { get; set; }
        public int FornecedorId { get; set; }
        public int Numero { get; set; }
        public int Aditivo { get; set; }
        public string Descricao { get; set; }
        public DateTime DTInicioVigencia { get; set; }
        public DateTime? DTFinalVigencia { get; set; }
        public decimal Valor { get; set; }
        public bool GeraNF { get; set; }
        public bool Renovavel { get; set; }
        public int UsuarioCriacao { get; set; }
    }

    public class AtualizarContrato
    {
        public int Id { get; set; }
        public int Fornecedor { get; set; }
        public int? Numero { get; set; }
        public int? Aditivo { get; set; }
        public string Descricao { get; set; }
        public DateTime DTInicioVigencia { get; set; }
        public DateTime? DTFinalVigencia { get; set; }
        public decimal Valor { get; set; }
        public bool GeraNF { get; set; }
        public bool Renovavel { get; set; }
    }

    public class EquipamentoContrato
    {
        public string Tipoequipamento { get; set; }
        public string Fabricante { get; set; }
        public string Modelo { get; set; }
        public string Numeroserie { get; set; }
        public DateTime Dtlimitegarantia { get; set; }
    }
}
