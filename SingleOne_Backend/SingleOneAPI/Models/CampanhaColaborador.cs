using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    [Table("campanhascolaboradores")]
    public partial class CampanhaColaborador
    {
        public int Id { get; set; }
        
        public int CampanhaId { get; set; }
        
        public int ColaboradorId { get; set; }
        
        public DateTime DataInclusao { get; set; }
        
        // Status: P=Pendente, E=Enviado, A=Assinado, R=Recusado
        public char StatusAssinatura { get; set; }
        
        public DateTime? DataEnvio { get; set; }
        
        public DateTime? DataAssinatura { get; set; }
        
        public int? TotalEnvios { get; set; }
        
        public DateTime? DataUltimoEnvio { get; set; }
        
        // Informações de envio
        public string? IpEnvio { get; set; }
        
        public string? LocalizacaoEnvio { get; set; }
        
        // Propriedades de navegação
        public virtual CampanhaAssinatura Campanha { get; set; } = null!;
        
        public virtual Colaboradore Colaborador { get; set; } = null!;
    }
}

