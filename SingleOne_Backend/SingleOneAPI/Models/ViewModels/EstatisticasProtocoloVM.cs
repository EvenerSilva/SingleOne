namespace SingleOneAPI.Models.ViewModels
{
    /// <summary>
    /// ViewModel para estatísticas do protocolo
    /// </summary>
    public class EstatisticasProtocoloVM
    {
        /// <summary>
        /// ID do protocolo
        /// </summary>
        public int ProtocoloId { get; set; }

        /// <summary>
        /// Número do protocolo
        /// </summary>
        public string NumeroProtocolo { get; set; }

        /// <summary>
        /// Total de equipamentos no protocolo
        /// </summary>
        public int TotalEquipamentos { get; set; }

        /// <summary>
        /// Equipamentos pendentes
        /// </summary>
        public int EquipamentosPendentes { get; set; }

        /// <summary>
        /// Equipamentos em processo
        /// </summary>
        public int EquipamentosEmProcesso { get; set; }

        /// <summary>
        /// Equipamentos concluídos
        /// </summary>
        public int EquipamentosConcluidos { get; set; }

        /// <summary>
        /// Percentual de conclusão
        /// </summary>
        public decimal PercentualConclusao { get; set; }

        /// <summary>
        /// Total de evidências anexadas
        /// </summary>
        public int TotalEvidencias { get; set; }

        /// <summary>
        /// Valor total estimado (para vendas)
        /// </summary>
        public decimal? ValorTotalEstimado { get; set; }

        /// <summary>
        /// Indica se pode ser finalizado
        /// </summary>
        public bool PodeFinalizar { get; set; }

        /// <summary>
        /// Status do protocolo
        /// </summary>
        public string StatusProtocolo { get; set; }

        /// <summary>
        /// Data de criação
        /// </summary>
        public System.DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data de conclusão (se finalizado)
        /// </summary>
        public System.DateTime? DataConclusao { get; set; }
    }
}
