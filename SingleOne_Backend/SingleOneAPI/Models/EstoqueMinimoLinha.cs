using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SingleOneAPI.Models
{
    [Table("estoqueminimolinhas")]
    public partial class EstoqueMinimoLinha
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Column("cliente")]
        public int Cliente { get; set; }
        
        [Required]
        [Column("operadora")]
        public int Operadora { get; set; }
        
        [Required]
        [Column("plano")]
        public int Plano { get; set; }
        
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
        
        [Column("perfiluso")]
        [StringLength(100)]
        public string PerfilUso { get; set; }
        
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
        public virtual Telefoniaoperadora OperadoraNavigation { get; set; }
        public virtual Telefoniaplano PlanoNavigation { get; set; }
        public virtual Localidade LocalidadeNavigation { get; set; }
        public virtual Usuario UsuarioCriacaoNavigation { get; set; }
        public virtual Usuario UsuarioAtualizacaoNavigation { get; set; }

        // ==============================
        // Propriedades calculadas (não mapeadas)
        // ==============================
        [NotMapped]
        [JsonProperty("estoqueAtual")]
        public int EstoqueAtual { get; set; }

        [NotMapped]
        [JsonProperty("percentualUtilizacao")]
        public decimal PercentualUtilizacao { get; set; }

        [NotMapped]
        [JsonProperty("statusEstoque")]
        public string StatusEstoque { get; set; } = "OK";
    }
}
