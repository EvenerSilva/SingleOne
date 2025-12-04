using System;

namespace SingleOneAPI.Models.DTO
{
    public class CampanhaResumoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public char Status { get; set; }
        public string StatusDescricao { get; set; }
        public string UsuarioCriacaoNome { get; set; }
        public int TotalColaboradores { get; set; }
        public int TotalEnviados { get; set; }
        public int TotalAssinados { get; set; }
        public int TotalPendentes { get; set; }
        public decimal? PercentualAdesao { get; set; }
        public DateTime? DataUltimoEnvio { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string FiltrosJson { get; set; }
    }
}

