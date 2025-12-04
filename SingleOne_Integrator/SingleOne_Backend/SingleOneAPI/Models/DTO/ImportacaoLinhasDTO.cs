using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.DTO
{
    /// <summary>
    /// DTO para resultado da validação do arquivo de importação
    /// </summary>
    public class ResultadoValidacaoDTO
    {
        public Guid LoteId { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalValidos { get; set; }
        public int TotalAvisos { get; set; }
        public int TotalErros { get; set; }
        public int NovasOperadoras { get; set; }
        public int NovosContratos { get; set; }
        public int NovosPlanos { get; set; }
        public bool PodeImportar { get; set; }
        public string Mensagem { get; set; }
    }

    /// <summary>
    /// DTO para detalhes de um registro na staging
    /// </summary>
    public class DetalheLinhaStagingDTO
    {
        public int Id { get; set; }
        public int LinhaArquivo { get; set; }
        public string OperadoraNome { get; set; }
        public string ContratoNome { get; set; }
        public string PlanoNome { get; set; }
        public decimal PlanoValor { get; set; }
        public decimal NumeroLinha { get; set; }
        public string Iccid { get; set; }
        public string Status { get; set; }
        public string StatusDescricao { get; set; }
        public List<string> Erros { get; set; }
        public List<string> Avisos { get; set; }
        public bool CriarOperadora { get; set; }
        public bool CriarContrato { get; set; }
        public bool CriarPlano { get; set; }
    }

    /// <summary>
    /// DTO para resultado da importação efetivada
    /// </summary>
    public class ResultadoImportacaoDTO
    {
        public Guid LoteId { get; set; }
        public int OperadorasCriadas { get; set; }
        public int ContratosCriados { get; set; }
        public int PlanosCriados { get; set; }
        public int LinhasCriadas { get; set; }
        public int TotalProcessado { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Mensagem { get; set; }
        public bool Sucesso { get; set; }
    }

    /// <summary>
    /// DTO para histórico de importações
    /// </summary>
    public class HistoricoImportacaoDTO
    {
        public int Id { get; set; }
        public Guid LoteId { get; set; }
        public string TipoImportacao { get; set; }
        public string NomeArquivo { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Status { get; set; }
        public string StatusDescricao { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalValidados { get; set; }
        public int TotalErros { get; set; }
        public int TotalImportados { get; set; }
        public string UsuarioNome { get; set; }
        public string UsuarioEmail { get; set; }
        public string Observacoes { get; set; }
    }

    /// <summary>
    /// DTO para resumo de validação por lote
    /// </summary>
    public class ResumoValidacaoDTO
    {
        public Guid LoteId { get; set; }
        public int Total { get; set; }
        public int Validos { get; set; }
        public int Avisos { get; set; }
        public int Erros { get; set; }
        public int Pendentes { get; set; }
        public int Importados { get; set; }
        
        // Contadores de novos registros
        public int NovasOperadoras { get; set; }
        public int NovosContratos { get; set; }
        public int NovosPlanos { get; set; }
        
        // Lista dos nomes das novas entidades
        public List<string> NomesOperadorasNovas { get; set; }
        public List<string> NomesContratosNovos { get; set; }
        public List<string> NomesPlanosNovos { get; set; }
    }

    /// <summary>
    /// DTO para upload de arquivo
    /// </summary>
    public class ArquivoImportacaoDTO
    {
        public string NomeArquivo { get; set; }
        public long TamanhoBytes { get; set; }
        public string TipoConteudo { get; set; }
        public DateTime DataUpload { get; set; }
    }
}

