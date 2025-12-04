using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("politicas_elegibilidade")]
    public partial class PoliticaElegibilidade
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("cliente")]
        public int Cliente { get; set; }
        
        [Column("tipo_colaborador")]
        public string TipoColaborador { get; set; } = string.Empty;
        
        [Column("cargo")]
        public string? Cargo { get; set; }
        
        [Column("usarpadrao")]
        public bool UsarPadrao { get; set; } = true; // Se true, usa LIKE '%cargo%'; se false, usa match exato
        
        [Column("tipo_equipamento_id")]
        public int TipoEquipamentoId { get; set; }
        
        [Column("permite_acesso")]
        public bool PermiteAcesso { get; set; }
        
        [Column("quantidade_maxima")]
        public int? QuantidadeMaxima { get; set; }
        
        [Column("observacoes")]
        public string? Observacoes { get; set; }
        
        [Column("ativo")]
        public bool Ativo { get; set; }
        
        [Column("dt_cadastro")]
        public DateTime? DtCadastro { get; set; }
        
        [Column("dt_atualizacao")]
        public DateTime? DtAtualizacao { get; set; }
        
        [Column("usuario_cadastro")]
        public int? UsuarioCadastro { get; set; }

        // Propriedades de navegação
        public virtual Cliente? ClienteNavigation { get; set; }
        public virtual Tipoequipamento? TipoEquipamentoNavigation { get; set; }
        public virtual Usuario? UsuarioCadastroNavigation { get; set; }
    }
}

