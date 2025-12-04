using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.DTO
{
    /// <summary>
    /// DTO para criar nova sinalização de suspeita
    /// </summary>
    public class CriarSinalizacaoDTO
    {
        public int ColaboradorId { get; set; }
        public string CpfConsultado { get; set; } = string.Empty;
        public string MotivoSuspeita { get; set; } = string.Empty;
        public string? DescricaoDetalhada { get; set; }
        public string? NomeVigilante { get; set; }
        public string? ObservacoesVigilante { get; set; }
        public string Prioridade { get; set; } = "media";
        public string? DadosConsulta { get; set; } // JSON com dados da consulta
        
        // Propriedades para auditoria (preenchidas pelo controller)
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// DTO para resposta de sinalização criada
    /// </summary>
    public class SinalizacaoCriadaDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public int SinalizacaoId { get; set; }
        public string NumeroProtocolo { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para listar sinalizações
    /// </summary>
    public class SinalizacaoListaDTO
    {
        public int Id { get; set; }
        public string? NumeroProtocolo { get; set; }
        public int ColaboradorId { get; set; }
        public string ColaboradorNome { get; set; } = string.Empty;
        public string CpfConsultado { get; set; } = string.Empty;
        public string MotivoSuspeita { get; set; } = string.Empty;
        public string MotivoSuspeitaDescricao { get; set; } = string.Empty;
        public string? DescricaoDetalhada { get; set; }
        public string? ObservacoesVigilante { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public DateTime DataSinalizacao { get; set; }
        public DateTime? DataInvestigacao { get; set; }
        public DateTime? DataResolucao { get; set; }
        public int? VigilanteId { get; set; }
        public int? InvestigadorId { get; set; }
        public string? InvestigadorNome { get; set; }
        public string? VigilanteNome { get; set; }
        public string? ResultadoInvestigacao { get; set; }
        public string? AcoesTomadas { get; set; }
        public string? ObservacoesFinais { get; set; }
        public bool TemEvidencias { get; set; }
        public int TotalEvidencias { get; set; }
    }

    /// <summary>
    /// DTO para detalhes de uma sinalização
    /// </summary>
    public class SinalizacaoDetalhesDTO
    {
        public int Id { get; set; }
        public int ColaboradorId { get; set; }
        public string ColaboradorNome { get; set; } = string.Empty;
        public string ColaboradorCpf { get; set; } = string.Empty;
        public string ColaboradorMatricula { get; set; } = string.Empty;
        public string ColaboradorCargo { get; set; } = string.Empty;
        public string ColaboradorSetor { get; set; } = string.Empty;
        public string CpfConsultado { get; set; } = string.Empty;
        public string MotivoSuspeita { get; set; } = string.Empty;
        public string MotivoSuspeitaDescricao { get; set; } = string.Empty;
        public string? DescricaoDetalhada { get; set; }
        public string? NomeVigilante { get; set; }
        public string? NumeroProtocolo { get; set; }
        public string? ObservacoesVigilante { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public string? DadosConsulta { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string[]? EvidenciaUrls { get; set; }
        public DateTime DataSinalizacao { get; set; }
        public DateTime? DataInvestigacao { get; set; }
        public DateTime? DataResolucao { get; set; }
        public int? InvestigadorId { get; set; }
        public string? InvestigadorNome { get; set; }
        public int? VigilanteId { get; set; }
        public string? VigilanteNome { get; set; }
        public string? ResultadoInvestigacao { get; set; }
        public string? AcoesTomadas { get; set; }
        public string? ObservacoesFinais { get; set; }
        public List<HistoricoInvestigacaoDTO> Historico { get; set; } = new List<HistoricoInvestigacaoDTO>();
    }

    /// <summary>
    /// DTO para histórico de investigação
    /// </summary>
    public class HistoricoInvestigacaoDTO
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public string Acao { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? DadosAntes { get; set; }
        public string? DadosDepois { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO para atualizar status de sinalização
    /// </summary>
    public class AtualizarStatusSinalizacaoDTO
    {
        public int SinalizacaoId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public int? InvestigadorId { get; set; }
    }

    /// <summary>
    /// DTO para resolver sinalização
    /// </summary>
    public class ResolverSinalizacaoDTO
    {
        public int SinalizacaoId { get; set; }
        public string ResultadoInvestigacao { get; set; } = string.Empty;
        public string? AcoesTomadas { get; set; }
        public string? ObservacoesFinais { get; set; }
    }

    /// <summary>
    /// DTO para motivos de suspeita
    /// </summary>
    public class MotivoSuspeitaDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? DescricaoDetalhada { get; set; }
        public string PrioridadePadrao { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    /// <summary>
    /// DTO para filtros de busca de sinalizações
    /// </summary>
    public class FiltroSinalizacoesDTO
    {
        public string? Status { get; set; }
        public string? Prioridade { get; set; }
        public string? MotivoSuspeita { get; set; }
        public int? InvestigadorId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? ColaboradorNome { get; set; }
        public string? CpfConsultado { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    /// <summary>
    /// DTO para resposta paginada de sinalizações
    /// </summary>
    public class SinalizacoesPaginadasDTO
    {
        public List<SinalizacaoListaDTO> Sinalizacoes { get; set; } = new List<SinalizacaoListaDTO>();
        public int TotalRegistros { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TamanhoPagina { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas de sinalizações
    /// </summary>
    public class EstatisticasSinalizacoesDTO
    {
        public int TotalSinalizacoes { get; set; }
        public int Pendentes { get; set; }
        public int EmInvestigacao { get; set; }
        public int Resolvidas { get; set; }
        public int Arquivadas { get; set; }
        public int Criticas { get; set; }
        public int Altas { get; set; }
        public int Medias { get; set; }
        public int Baixas { get; set; }
        public Dictionary<string, int> PorMotivo { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PorStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PorPrioridade { get; set; } = new Dictionary<string, int>();
    }
}
