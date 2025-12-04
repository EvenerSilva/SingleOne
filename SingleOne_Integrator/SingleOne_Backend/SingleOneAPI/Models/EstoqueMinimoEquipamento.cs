using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("estoqueminimoequipamentos")]
    public partial class EstoqueMinimoEquipamento
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Column("cliente")]
        public int Cliente { get; set; }
        
        [Required]
        [Column("modelo")]
        public int Modelo { get; set; }
        
        [Required]
        [Column("localidade")]
        public int Localidade { get; set; }
        
        [Required]
        [Column("quantidademinima")]
        public int QuantidadeMinima { get; set; }
        
        [Column("quantidadetotallancada")]
        public int QuantidadeTotalLancada { get; set; } = 0;
        
        [Column("quantidademaxima")]
        public int QuantidadeMaxima { get; set; } = 0;
        
        [Column("observacoes")]
        public string? Observacoes { get; set; }
        
        [Required]
        [Column("ativo")]
        public bool Ativo { get; set; } = true;
        
        [Required]
        [Column("dtcriacao")]
        public DateTime DtCriacao { get; set; } = DateTime.Now;
        
        [Required]
        [Column("usuariocriacao")]
        public int UsuarioCriacao { get; set; }
        
        [Column("dtatualizacao")]
        public DateTime? DtAtualizacao { get; set; }
        
        [Column("usuarioatualizacao")]
        public int? UsuarioAtualizacao { get; set; }

        // Navegação
        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Modelo ModeloNavigation { get; set; }
        public virtual Localidade LocalidadeNavigation { get; set; }
        public virtual Usuario UsuarioCriacaoNavigation { get; set; }
        public virtual Usuario UsuarioAtualizacaoNavigation { get; set; }
    }
}
