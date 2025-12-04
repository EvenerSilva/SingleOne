using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    /// <summary>
    /// Modelo que representa um equipamento dentro de um protocolo de descarte
    /// Cada protocolo pode ter múltiplos itens (equipamentos)
    /// </summary>
    public partial class ProtocoloDescarteItem
    {
        /// <summary>
        /// ID único do item
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID do protocolo ao qual este item pertence
        /// </summary>
        [Required]
        public int ProtocoloId { get; set; }

        /// <summary>
        /// ID do equipamento
        /// </summary>
        [Required]
        public int Equipamento { get; set; }

        /// <summary>
        /// Indica se este equipamento tem processos obrigatórios (passou por cargo de confiança)
        /// </summary>
        public bool ProcessosObrigatorios { get; set; }

        /// <summary>
        /// Indica se é obrigatório executar sanitização
        /// </summary>
        public bool ObrigarSanitizacao { get; set; }

        /// <summary>
        /// Indica se é obrigatório executar descaracterização
        /// </summary>
        public bool ObrigarDescaracterizacao { get; set; }

        /// <summary>
        /// Indica se é obrigatório executar perfuração de disco
        /// </summary>
        public bool ObrigarPerfuracaoDisco { get; set; }

        /// <summary>
        /// Indica se são obrigatórias evidências para este equipamento
        /// </summary>
        public bool EvidenciasObrigatorias { get; set; }

        /// <summary>
        /// Indica se o processo de sanitização foi executado
        /// </summary>
        public bool ProcessoSanitizacao { get; set; }

        /// <summary>
        /// Indica se o processo de descaracterização foi executado
        /// </summary>
        public bool ProcessoDescaracterizacao { get; set; }

        /// <summary>
        /// Indica se o processo de perfuração de disco foi executado
        /// </summary>
        public bool ProcessoPerfuracaoDisco { get; set; }

        /// <summary>
        /// Indica se as evidências foram executadas
        /// </summary>
        public bool EvidenciasExecutadas { get; set; }

        /// <summary>
        /// Método de sanitização utilizado (Formatação Simples, Sobregravar Mídia, Destruição Física, etc)
        /// </summary>
        [StringLength(50)]
        public string MetodoSanitizacao { get; set; }

        /// <summary>
        /// Ferramenta ou equipamento utilizado na sanitização (ex: HDDErase v4.0, Perfurador Industrial)
        /// </summary>
        [StringLength(200)]
        public string FerramentaUtilizada { get; set; }

        /// <summary>
        /// Observações adicionais sobre o processo de sanitização
        /// </summary>
        public string ObservacoesSanitizacao { get; set; }

        /// <summary>
        /// Valor estimado do equipamento (principalmente para vendas)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorEstimado { get; set; }

        /// <summary>
        /// Observações específicas deste equipamento
        /// </summary>
        public string ObservacoesItem { get; set; }

        /// <summary>
        /// Data em que o processo foi iniciado
        /// </summary>
        public DateTime? DataProcessoIniciado { get; set; }

        /// <summary>
        /// Data em que o processo foi concluído
        /// </summary>
        public DateTime? DataProcessoConcluido { get; set; }

        /// <summary>
        /// Status do item (PENDENTE, EM_PROCESSO, CONCLUIDO)
        /// </summary>
        [Required]
        [StringLength(30)]
        public string StatusItem { get; set; }

        /// <summary>
        /// Indica se o registro está ativo
        /// </summary>
        public bool Ativo { get; set; }

        // Navegação
        /// <summary>
        /// Protocolo ao qual este item pertence
        /// </summary>
        [ForeignKey("ProtocoloId")]
        public virtual ProtocoloDescarte Protocolo { get; set; }

        /// <summary>
        /// Equipamento relacionado
        /// </summary>
        [ForeignKey("Equipamento")]
        public virtual Equipamento EquipamentoNavigation { get; set; }
    }
}
