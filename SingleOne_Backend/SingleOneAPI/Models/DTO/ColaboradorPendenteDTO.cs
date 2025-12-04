using System;

namespace SingleOneAPI.Models.DTO
{
    public class ColaboradorPendenteDTO
    {
        public int ColaboradorId { get; set; }
        public string ColaboradorNome { get; set; }
        public string ColaboradorCpf { get; set; }
        public string ColaboradorEmail { get; set; }
        public string ColaboradorCargo { get; set; }
        public string EmpresaNome { get; set; }
        public string LocalidadeNome { get; set; }
        public char StatusAssinatura { get; set; }
        public string StatusAssinaturaDescricao { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime? DataEnvio { get; set; }
        public DateTime? DataUltimoEnvio { get; set; }
        public int? TotalEnvios { get; set; }
        public int DiasDesdeEnvio { get; set; }
    }
}

