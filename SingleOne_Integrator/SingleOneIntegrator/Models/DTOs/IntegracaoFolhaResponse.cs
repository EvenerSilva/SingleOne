using System;
using System.Collections.Generic;

namespace SingleOneIntegrator.Models.DTOs
{
    /// <summary>
    /// Response da integração de folha
    /// </summary>
    public class IntegracaoFolhaResponse
    {
        /// <summary>
        /// Sucesso da operação
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ID único da integração
        /// </summary>
        public string IntegracaoId { get; set; }

        /// <summary>
        /// Timestamp do processamento
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Estatísticas do processamento
        /// </summary>
        public IntegracaoEstatisticas Estatisticas { get; set; }

        /// <summary>
        /// Lista de erros (se houver)
        /// </summary>
        public List<IntegracaoErro>? Erros { get; set; }

        /// <summary>
        /// Mensagem adicional
        /// </summary>
        public string? Mensagem { get; set; }
    }

    /// <summary>
    /// Estatísticas do processamento
    /// </summary>
    public class IntegracaoEstatisticas
    {
        /// <summary>
        /// Total de colaboradores recebidos
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Novos colaboradores criados
        /// </summary>
        public int Novos { get; set; }

        /// <summary>
        /// Colaboradores atualizados
        /// </summary>
        public int Atualizados { get; set; }

        /// <summary>
        /// Colaboradores com erro
        /// </summary>
        public int Erros { get; set; }

        /// <summary>
        /// Tempo de processamento em milissegundos
        /// </summary>
        public int TempoProcessamento { get; set; }
    }

    /// <summary>
    /// Detalhes de um erro
    /// </summary>
    public class IntegracaoErro
    {
        /// <summary>
        /// Linha/índice do colaborador com erro
        /// </summary>
        public int Linha { get; set; }

        /// <summary>
        /// CPF do colaborador
        /// </summary>
        public string? Cpf { get; set; }

        /// <summary>
        /// Nome do colaborador
        /// </summary>
        public string? Nome { get; set; }

        /// <summary>
        /// Descrição do erro
        /// </summary>
        public string Erro { get; set; }
    }
}


