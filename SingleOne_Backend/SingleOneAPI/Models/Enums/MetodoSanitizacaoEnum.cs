namespace SingleOneAPI.Models.Enums
{
    /// <summary>
    /// Enum para métodos de sanitização conforme Política de Sanitização e Descarte
    /// </summary>
    public static class MetodoSanitizacaoEnum
    {
        /// <summary>
        /// Formatação simples do disco rígido
        /// Tipo: Limpeza apenas
        /// </summary>
        public const string FORMATACAO_SIMPLES = "FORMATACAO_SIMPLES";

        /// <summary>
        /// Sobregravar dados por 7 vezes usando DoD 5220.22-M ou Secure Erase
        /// Tipo: Limpeza e Eliminação
        /// </summary>
        public const string SOBREGRAVAR_MIDIA = "SOBREGRAVAR_MIDIA";

        /// <summary>
        /// Destruição física com picotadores, pulverizadores ou incineradores
        /// Tipo: Destruição
        /// </summary>
        public const string DESTRUICAO_FISICA = "DESTRUICAO_FISICA";

        /// <summary>
        /// Desmagnetização com degausser especializado
        /// Tipo: Eliminação apenas
        /// Aplicável: Fitas e disquetes
        /// </summary>
        public const string DESMAGNETIZACAO = "DESMAGNETIZACAO";

        /// <summary>
        /// Restauração das configurações padrão de fábrica
        /// Tipo: Limpeza apenas
        /// Aplicável: Equipamentos gerenciáveis, celulares, tablets
        /// </summary>
        public const string RESTAURACAO_FABRICA = "RESTAURACAO_FABRICA";
    }
}

