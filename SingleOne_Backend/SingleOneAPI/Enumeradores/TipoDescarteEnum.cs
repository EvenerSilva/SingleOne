using System.ComponentModel;

namespace SingleOne.Enumeradores
{
    /// <summary>
    /// Enum que define os tipos de descarte disponíveis no sistema
    /// </summary>
    public enum TipoDescarteEnum
    {
        /// <summary>
        /// Doação do equipamento para instituições
        /// </summary>
        [Description("Doação")]
        DOACAO = 1,

        /// <summary>
        /// Venda do equipamento para terceiros
        /// </summary>
        [Description("Venda")]
        VENDA = 2,

        /// <summary>
        /// Devolução do equipamento para o fornecedor
        /// </summary>
        [Description("Devolução")]
        DEVOLUCAO = 3,

        /// <summary>
        /// Logística reversa para reciclagem
        /// </summary>
        [Description("Logística Reversa")]
        LOGISTICA_REVERSA = 4,

        /// <summary>
        /// Descarte final como lixo eletrônico
        /// </summary>
        [Description("Descarte Geral (destruição)")]
        DESCARTE_FINAL = 5
    }
}
