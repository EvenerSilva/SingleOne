using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    /// <summary>
    /// Modelo para logs de acesso ao sistema
    /// </summary>
    [Table("patrimonio_logs_acesso")]
    public class PatrimonioLogAcesso
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("tipo_acesso")]
        public string TipoAcesso { get; set; } = string.Empty; // "passcheck" ou "patrimonio"
        
        [Column("colaborador_id")]
        public int? ColaboradorId { get; set; }
        
        [Column("cpf_consultado")]
        public string CpfConsultado { get; set; } = string.Empty;
        
        [Column("ip_address")]
        public string IpAddress { get; set; } = string.Empty;
        
        [Column("user_agent")]
        public string UserAgent { get; set; } = string.Empty;
        
        [Column("dados_consultados")]
        public string DadosConsultados { get; set; } = string.Empty; // JSON
        
        [Column("sucesso")]
        public bool Sucesso { get; set; } = true;
        
        [Column("mensagem_erro")]
        public string MensagemErro { get; set; } = string.Empty;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Propriedades de navegação
        public virtual Colaboradore? Colaborador { get; set; }
    }
}
