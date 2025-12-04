using System.Text.Json.Serialization;

namespace SingleOneAPI.Models
{
    public class PlanosVM
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("operadora")]
        public string Operadora { get; set; } = string.Empty;
        
        [JsonPropertyName("operadoraId")]
        public int OperadoraId { get; set; }
        
        [JsonPropertyName("contrato")]
        public string Contrato { get; set; } = string.Empty;
        
        [JsonPropertyName("contratoId")]
        public int ContratoId { get; set; }
        
        [JsonPropertyName("plano")]
        public string Plano { get; set; } = string.Empty;
        
        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }
        
        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; }
        
        [JsonPropertyName("contlinhas")]
        public long ContLinhas { get; set; }
        
        [JsonPropertyName("contlinhasemuso")]
        public long ContLinhasEmUso { get; set; }
        
        [JsonPropertyName("contlinhaslivres")]
        public long ContLinhasLivres { get; set; }
    }
}
