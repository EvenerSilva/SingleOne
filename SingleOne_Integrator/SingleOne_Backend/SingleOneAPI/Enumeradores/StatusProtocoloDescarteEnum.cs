using System.ComponentModel;

namespace SingleOne.Enumeradores
{
    /// <summary>
    /// Enum que define os status possíveis de um protocolo de descarte
    /// </summary>
    public enum StatusProtocoloDescarteEnum
    {
        /// <summary>
        /// Protocolo criado e em andamento
        /// </summary>
        [Description("Em Andamento")]
        EM_ANDAMENTO = 1,

        /// <summary>
        /// Protocolo concluído com sucesso
        /// </summary>
        [Description("Concluído")]
        CONCLUIDO = 2,

        /// <summary>
        /// Protocolo cancelado
        /// </summary>
        [Description("Cancelado")]
        CANCELADO = 3
    }

    /// <summary>
    /// Enum que define os status possíveis de um item dentro do protocolo
    /// </summary>
    public enum StatusItemProtocoloEnum
    {
        /// <summary>
        /// Item pendente de processamento
        /// </summary>
        [Description("Pendente")]
        PENDENTE = 1,

        /// <summary>
        /// Item em processo de descarte
        /// </summary>
        [Description("Em Processo")]
        EM_PROCESSO = 2,

        /// <summary>
        /// Item concluído com sucesso
        /// </summary>
        [Description("Concluído")]
        CONCLUIDO = 3
    }
}
