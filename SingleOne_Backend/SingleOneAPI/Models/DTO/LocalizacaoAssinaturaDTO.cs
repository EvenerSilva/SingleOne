using System;

namespace SingleOneAPI.Models.DTO
{
    public class LocalizacaoAssinaturaDTO
    {
        public int ColaboradorId { get; set; }
        public string ColaboradorNome { get; set; }
        public int UsuarioLogadoId { get; set; }
        public string IP { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public DateTime Timestamp { get; set; }
        public string Acao { get; set; }
    }
}






































