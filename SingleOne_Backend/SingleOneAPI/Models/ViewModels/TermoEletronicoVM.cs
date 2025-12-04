using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Models.ViewModels
{
    public class TermoEletronicoVM
    {
        public string Cpf { get; set; }
        public string PalavraChave { get; set; } // ✅ CAMPO FALTANTE!
        public string HashRequisicao { get; set; }
        
        // Campos de geolocalização
        public string IpAddress { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Accuracy { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
