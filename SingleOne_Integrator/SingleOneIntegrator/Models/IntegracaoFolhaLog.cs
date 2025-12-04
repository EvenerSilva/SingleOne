using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneIntegrator.Models
{
    /// <summary>
    /// Log de auditoria das integrações de folha
    /// </summary>
    [Table("IntegracaoFolhaLog")]
    public class IntegracaoFolhaLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID único da integração
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string IntegracaoId { get; set; }

        /// <summary>
        /// ID do cliente que enviou os dados
        /// </summary>
        [Required]
        public int ClienteId { get; set; }

        /// <summary>
        /// Data e hora da requisição
        /// </summary>
        [Required]
        public DateTime DataHora { get; set; }

        /// <summary>
        /// Endereço IP de origem
        /// </summary>
        [MaxLength(50)]
        public string? IpOrigem { get; set; }

        /// <summary>
        /// API Key utilizada
        /// </summary>
        [MaxLength(100)]
        public string? ApiKey { get; set; }

        /// <summary>
        /// Tipo de operação (FULL_SYNC ou INCREMENTAL)
        /// </summary>
        [MaxLength(20)]
        public string? TipoOperacao { get; set; }

        /// <summary>
        /// Total de colaboradores enviados
        /// </summary>
        public int ColaboradoresEnviados { get; set; }

        /// <summary>
        /// Total de colaboradores processados com sucesso
        /// </summary>
        public int ColaboradoresProcessados { get; set; }

        /// <summary>
        /// Total de colaboradores com erro
        /// </summary>
        public int ColaboradoresErro { get; set; }

        /// <summary>
        /// Detalhes dos erros (JSON)
        /// </summary>
        [Column(TypeName = "text")]
        public string? Erros { get; set; }

        /// <summary>
        /// Tempo de processamento em milissegundos
        /// </summary>
        public int TempoProcessamento { get; set; }

        /// <summary>
        /// Status HTTP retornado
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Sucesso da operação
        /// </summary>
        public bool Sucesso { get; set; }

        /// <summary>
        /// Mensagem adicional
        /// </summary>
        [MaxLength(500)]
        public string? Mensagem { get; set; }
    }
}


