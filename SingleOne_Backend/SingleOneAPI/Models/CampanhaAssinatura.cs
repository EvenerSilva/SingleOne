using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("campanhasassinaturas")]
    public partial class CampanhaAssinatura
    {
        public int Id { get; set; }
        
        public int Cliente { get; set; }
        
        public int UsuarioCriacao { get; set; }
        
        public string Nome { get; set; } = string.Empty;
        
        public string? Descricao { get; set; }
        
        public DateTime DataCriacao { get; set; }
        
        public DateTime? DataInicio { get; set; }
        
        public DateTime? DataFim { get; set; }
        
        // Status: A=Ativa, I=Inativa, C=Concluída, G=Agendada
        public char Status { get; set; }
        
        // JSON com os filtros aplicados (empresas, localidades, tipos, etc)
        public string? FiltrosJson { get; set; }
        
        // Estatísticas
        public int TotalColaboradores { get; set; }
        
        public int TotalEnviados { get; set; }
        
        public int TotalAssinados { get; set; }
        
        public int TotalPendentes { get; set; }
        
        public decimal? PercentualAdesao { get; set; }
        
        // Controle
        public DateTime? DataUltimoEnvio { get; set; }
        
        public DateTime? DataConclusao { get; set; }
        
        // Propriedades de navegação
        public virtual Cliente ClienteNavigation { get; set; } = null!;
        
        public virtual Usuario UsuarioCriacaoNavigation { get; set; } = null!;
        
        public virtual ICollection<CampanhaColaborador> CampanhaColaboradores { get; set; } = new List<CampanhaColaborador>();
    }
}

