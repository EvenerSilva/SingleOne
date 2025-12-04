using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    /// <summary>
    /// Modelo para contestações de patrimônio
    /// </summary>
    [Table("patrimonio_contestoes")]
    public class PatrimonioContestacao
    {
        public int Id { get; set; }
        
        [Column("colaborador_id")]
        public int ColaboradorId { get; set; }
        
        [Column("equipamento_id")]
        public int EquipamentoId { get; set; }
        
        public string Motivo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Status { get; set; } = "pendente";
        
        [Column("evidencia_url")]
        public string EvidenciaUrl { get; set; } = string.Empty;
        
        [Column("data_contestacao")]
        public DateTime DataContestacao { get; set; } = DateTime.Now;
        
        [Column("data_resolucao")]
        public DateTime? DataResolucao { get; set; }
        
        [Column("usuario_resolucao")]
        public int? UsuarioResolucao { get; set; }
        
        [Column("observacao_resolucao")]
        public string ObservacaoResolucao { get; set; } = string.Empty;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        [Column("tipo_contestacao")]
        public string TipoContestacao { get; set; } = "contestacao";

        // Propriedades de navegação
        public virtual Colaboradore Colaborador { get; set; } = null!;
        // ✅ CORREÇÃO: Removida propriedade de navegação para Equipamento
        // pois agora equipamento_id pode referenciar tanto equipamentos quanto linhas telefônicas
        // public virtual Equipamento Equipamento { get; set; } = null!;
        public virtual Usuario? UsuarioResolucaoNavigation { get; set; }
    }
}
