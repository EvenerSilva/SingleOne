using System.ComponentModel.DataAnnotations;

namespace SingleOneAPI.Models.DTO
{
    /// <summary>
    /// DTO para criação de solicitação de Auto Inventário
    /// </summary>
    public class CriarAutoInventarioDTO
    {
        [Required(ErrorMessage = "ID do colaborador é obrigatório")]
        public int ColaboradorId { get; set; }
        
        [Required(ErrorMessage = "Número de série é obrigatório")]
        [StringLength(100, ErrorMessage = "Número de série deve ter no máximo 100 caracteres")]
        public string NumeroSerie { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
        public string Observacoes { get; set; } = string.Empty;
    }
}
