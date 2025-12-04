namespace SingleOneAPI.Models.ViewModels
{
    /// <summary>
    /// ViewModel para equipamentos disponíveis para adicionar ao protocolo
    /// </summary>
    public class EquipamentoDisponivelVM
    {
        /// <summary>
        /// ID do equipamento
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número de série
        /// </summary>
        public string NumeroSerie { get; set; }

        /// <summary>
        /// Patrimônio
        /// </summary>
        public string Patrimonio { get; set; }

        /// <summary>
        /// Descrição do equipamento
        /// </summary>
        public string Descricao { get; set; }

        /// <summary>
        /// Fabricante
        /// </summary>
        public string Fabricante { get; set; }

        /// <summary>
        /// Modelo
        /// </summary>
        public string Modelo { get; set; }

        /// <summary>
        /// Status do equipamento
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Tipo do equipamento
        /// </summary>
        public string TipoEquipamento { get; set; }

        /// <summary>
        /// Tipo de aquisição
        /// </summary>
        public string TipoAquisicao { get; set; }

        /// <summary>
        /// Indica se já está em algum protocolo
        /// </summary>
        public bool JaEstaEmProtocolo { get; set; }

        /// <summary>
        /// ID do protocolo em que está (se já estiver)
        /// </summary>
        public int? ProtocoloId { get; set; }

        /// <summary>
        /// Número do protocolo em que está
        /// </summary>
        public string NumeroProtocolo { get; set; }

        /// <summary>
        /// Indica se o equipamento tem processos obrigatórios
        /// </summary>
        public bool ProcessosObrigatorios { get; set; }

        /// <summary>
        /// Indica se obriga sanitização
        /// </summary>
        public bool ObrigarSanitizacao { get; set; }

        /// <summary>
        /// Indica se obriga descaracterização
        /// </summary>
        public bool ObrigarDescaracterizacao { get; set; }

        /// <summary>
        /// Indica se obriga perfuração de disco
        /// </summary>
        public bool ObrigarPerfuracaoDisco { get; set; }

        /// <summary>
        /// Indica se obriga evidências
        /// </summary>
        public bool ObrigarEvidencias { get; set; }
    }
}
