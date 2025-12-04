using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    /// <summary>
    /// Modelo para sinalizações de suspeitas na portaria
    /// </summary>
    [Table("sinalizacoes_suspeitas")]
    public class SinalizacaoSuspeita
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("colaborador_id")]
        public int ColaboradorId { get; set; }

        [Column("vigilante_id")]
        public int? VigilanteId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("cpf_consultado")]
        public string CpfConsultado { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("motivo_suspeita")]
        public string MotivoSuspeita { get; set; } = string.Empty;

        [Column("descricao_detalhada")]
        public string? DescricaoDetalhada { get; set; }

        [Column("observacoes_vigilante")]
        public string? ObservacoesVigilante { get; set; }

        [StringLength(100)]
        [Column("nome_vigilante")]
        public string? NomeVigilante { get; set; }

        [StringLength(20)]
        [Column("numero_protocolo")]
        public string? NumeroProtocolo { get; set; }

        [Required]
        [StringLength(20)]
        [Column("status")]
        public string Status { get; set; } = "pendente";

        [Required]
        [StringLength(10)]
        [Column("prioridade")]
        public string Prioridade { get; set; } = "media";

        [Column("dados_consulta")]
        public string? DadosConsulta { get; set; } // JSON

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }


        [Column("data_sinalizacao")]
        public DateTime DataSinalizacao { get; set; }

        [Column("data_investigacao")]
        public DateTime? DataInvestigacao { get; set; }

        [Column("data_resolucao")]
        public DateTime? DataResolucao { get; set; }

        [Column("investigador_id")]
        public int? InvestigadorId { get; set; }

        [Column("resultado_investigacao")]
        public string? ResultadoInvestigacao { get; set; }

        [Column("acoes_tomadas")]
        public string? AcoesTomadas { get; set; }

        [Column("observacoes_finais")]
        public string? ObservacoesFinais { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navegação para relacionamentos
        [ForeignKey("ColaboradorId")]
        public virtual Colaboradore? Colaborador { get; set; }

        [ForeignKey("VigilanteId")]
        public virtual Usuario? Vigilante { get; set; }

        [ForeignKey("InvestigadorId")]
        public virtual Usuario? Investigador { get; set; }

        // Relacionamento com histórico
        public virtual ICollection<HistoricoInvestigacao> Historico { get; set; } = new List<HistoricoInvestigacao>();
    }

    /// <summary>
    /// Modelo para histórico de investigações
    /// </summary>
    [Table("historico_investigacoes")]
    public class HistoricoInvestigacao
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("sinalizacao_id")]
        public int SinalizacaoId { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("acao")]
        public string Acao { get; set; } = string.Empty;

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("dados_antes")]
        public string? DadosAntes { get; set; } // JSON

        [Column("dados_depois")]
        public string? DadosDepois { get; set; } // JSON

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navegação para relacionamentos
        [ForeignKey("SinalizacaoId")]
        public virtual SinalizacaoSuspeita? Sinalizacao { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }

    /// <summary>
    /// Modelo para motivos de suspeita
    /// </summary>
    [Table("motivos_suspeita")]
    public class MotivoSuspeita
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [Column("descricao_detalhada")]
        public string? DescricaoDetalhada { get; set; }

        [Column("ativo")]
        public bool Ativo { get; set; } = true;

        [Required]
        [StringLength(10)]
        [Column("prioridade_padrao")]
        public string PrioridadePadrao { get; set; } = "media";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
