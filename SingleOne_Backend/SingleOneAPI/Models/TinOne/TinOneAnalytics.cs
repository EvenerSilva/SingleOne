using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models.TinOne
{
    /// <summary>
    /// Modelo para analytics de uso do TinOne
    /// </summary>
    [Table("tinone_analytics")]
    public class TinOneAnalytics
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int? UsuarioId { get; set; }

        [Column("cliente_id")]
        public int? ClienteId { get; set; }

        [Column("sessao_id")]
        [MaxLength(100)]
        public string? SessaoId { get; set; }

        [Column("pagina_url")]
        [MaxLength(500)]
        public string? PaginaUrl { get; set; }

        [Column("pagina_nome")]
        [MaxLength(200)]
        public string? PaginaNome { get; set; }

        [Column("acao_tipo")]
        [MaxLength(100)]
        public string? AcaoTipo { get; set; }

        [Column("pergunta")]
        public string? Pergunta { get; set; }

        [Column("resposta")]
        public string? Resposta { get; set; }

        [Column("tempo_resposta_ms")]
        public int? TempoRespostaMs { get; set; }

        [Column("foi_util")]
        public bool? FoiUtil { get; set; }

        [Column("feedback_texto")]
        public string? FeedbackTexto { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }
    }
}

