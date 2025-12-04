using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models.TinOne
{
    /// <summary>
    /// Modelo para conversas do TinOne (histórico de chat)
    /// </summary>
    [Table("tinone_conversas")]
    public class TinOneConversa
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_id")]
        public int? UsuarioId { get; set; }

        [Column("sessao_id")]
        [MaxLength(100)]
        public string? SessaoId { get; set; }

        [Column("tipo_mensagem")]
        [MaxLength(20)]
        public string TipoMensagem { get; set; } = "usuario"; // 'usuario' ou 'assistente'

        [Column("mensagem")]
        [Required]
        public string Mensagem { get; set; } = string.Empty;

        [Column("pagina_contexto")]
        [MaxLength(200)]
        public string? PaginaContexto { get; set; }

        [NotMapped] // 🔥 Temporariamente não mapeado para evitar erro de tipo jsonb
        public string? Metadata { get; set; } // JSON

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}

