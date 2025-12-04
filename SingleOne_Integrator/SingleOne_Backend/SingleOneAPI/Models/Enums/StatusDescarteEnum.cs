namespace SingleOneAPI.Models.Enums
{
    /// <summary>
    /// Define quais status de equipamentos podem ou não ser descartados
    /// Seguindo boas práticas de inventário e controle
    /// </summary>
    public static class StatusDescarteEnum
    {
        /// <summary>
        /// Status que podem ir DIRETAMENTE para descarte
        /// </summary>
        public static readonly int[] StatusPermitidos = new int[]
        {
            9   // Sinistrado - Já passou por laudo técnico e foi avaliado como sem conserto
        };

        /// <summary>
        /// Status que PRECISAM de processo intermediário antes do descarte
        /// </summary>
        public static readonly int[] StatusProcessoIntermediario = new int[]
        {
            1,  // Danificado - Precisa finalizar laudo técnico → Sinistrado → Descarte
            2,  // Devolvido - Precisa avaliar → Se não funcional: Laudo → Sinistrado → Descarte
            3   // Em Estoque - Se não funcional, precisa de Laudo → Sinistrado → Descarte
        };

        /// <summary>
        /// Status BLOQUEADOS para descarte (não fazem sentido ou precisam de processo específico)
        /// </summary>
        public static readonly int[] StatusBloqueados = new int[]
        {
            4,  // Entregue - Precisa devolver primeiro
            5,  // Extraviado - Precisa de processo de baixa patrimonial
            6,  // Novo - Equipamento zero, não pode descartar
            7,  // Requisitado - Precisa cancelar requisição
            8,  // Roubado - Precisa de processo de baixa com B.O.
            10  // Descartado - Já foi descartado
        };

        /// <summary>
        /// Mensagens de orientação por status
        /// </summary>
        public static string ObterMensagemStatus(int statusId)
        {
            return statusId switch
            {
                1 => "Este equipamento está em processo de laudo técnico. Finalize o laudo primeiro.",
                2 => "Equipamento devolvido precisa de avaliação técnica. Se não funcional, crie um laudo técnico.",
                3 => "Equipamento em estoque está apto para uso. Se não funcional, envie para laudo técnico primeiro.",
                4 => "Equipamento entregue ao usuário. Solicite devolução primeiro.",
                5 => "Equipamentos extraviados requerem processo de baixa patrimonial específico.",
                6 => "Equipamento novo não pode ser descartado. Se defeituoso, envie para laudo técnico.",
                7 => "Equipamento requisitado. Cancele a requisição primeiro.",
                8 => "Equipamentos roubados requerem processo de baixa patrimonial com B.O.",
                9 => "Equipamento sinistrado pode ser descartado (laudo técnico já concluído).",
                10 => "Este equipamento já foi descartado.",
                _ => "Status não reconhecido para descarte."
            };
        }

        /// <summary>
        /// Verifica se o status permite descarte direto
        /// </summary>
        public static bool PodeDescartar(int statusId)
        {
            return System.Array.IndexOf(StatusPermitidos, statusId) >= 0;
        }

        /// <summary>
        /// Verifica se o status precisa de processo intermediário
        /// </summary>
        public static bool PrecisaProcessoIntermediario(int statusId)
        {
            return System.Array.IndexOf(StatusProcessoIntermediario, statusId) >= 0;
        }

        /// <summary>
        /// Verifica se o status está bloqueado para descarte
        /// </summary>
        public static bool EstaBloqueado(int statusId)
        {
            return System.Array.IndexOf(StatusBloqueados, statusId) >= 0;
        }

        /// <summary>
        /// Obter tipo de validação do status
        /// </summary>
        public static string ObterTipoValidacao(int statusId)
        {
            if (PodeDescartar(statusId))
                return "PERMITIDO";
            else if (PrecisaProcessoIntermediario(statusId))
                return "PROCESSO_INTERMEDIARIO";
            else if (EstaBloqueado(statusId))
                return "BLOQUEADO";
            else
                return "DESCONHECIDO";
        }
    }
}

