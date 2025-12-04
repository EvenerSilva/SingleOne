using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneIntegrator.Models
{
    /// <summary>
    /// Representa a configuração de integração de um cliente
    /// </summary>
    [Table("ClienteIntegracao")]
    public class ClienteIntegracao
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID do cliente no sistema SingleOne
        /// </summary>
        [Required]
        public int ClienteId { get; set; }

        /// <summary>
        /// API Key pública para identificação (prefixo: sk_live_ ou sk_test_)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ApiKey { get; set; }

        /// <summary>
        /// API Secret para HMAC (não deve ser exposto, apenas armazenado)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ApiSecret { get; set; }

        /// <summary>
        /// IPs permitidos (separados por vírgula) - opcional
        /// Exemplo: "203.0.113.0/24,198.51.100.50"
        /// </summary>
        [MaxLength(500)]
        public string? IpWhitelist { get; set; }

        /// <summary>
        /// URL de callback para notificações (opcional)
        /// </summary>
        [MaxLength(500)]
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Integração está ativa?
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// Data de criação da integração
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime? DataAtualizacao { get; set; }

        /// <summary>
        /// Data da última sincronização bem-sucedida
        /// </summary>
        public DateTime? UltimaSincronizacao { get; set; }

        /// <summary>
        /// Observações sobre a integração
        /// </summary>
        [MaxLength(1000)]
        public string? Observacoes { get; set; }
    }
}


