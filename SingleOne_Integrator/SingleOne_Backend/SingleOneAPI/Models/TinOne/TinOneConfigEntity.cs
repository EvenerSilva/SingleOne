using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models.TinOne
{
    /// <summary>
    /// Entidade para configurações do TinOne (tabela tinone_config)
    /// </summary>
    [Table("tinone_config")]
    public class TinOneConfigEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("cliente")]
        public int? Cliente { get; set; }

        [Column("chave")]
        [Required]
        [MaxLength(100)]
        public string Chave { get; set; } = string.Empty;

        [Column("valor")]
        public string? Valor { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("ativo")]
        public bool Ativo { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

