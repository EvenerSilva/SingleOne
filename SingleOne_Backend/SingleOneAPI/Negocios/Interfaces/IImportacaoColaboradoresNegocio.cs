using Microsoft.AspNetCore.Http;
using SingleOneAPI.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IImportacaoColaboradoresNegocio
    {
        /// <summary>
        /// Processa o arquivo Excel e insere dados na staging
        /// </summary>
        Task<ResultadoValidacaoColaboradoresDTO> ProcessarArquivo(IFormFile arquivo, int clienteId, int usuarioId);

        /// <summary>
        /// Obtém detalhes da validação de um lote
        /// </summary>
        Task<List<DetalheColaboradorStagingDTO>> ObterDetalhesValidacao(Guid loteId, int clienteId, string filtroStatus = null);

        /// <summary>
        /// Obtém resumo da validação de um lote
        /// </summary>
        Task<ResumoValidacaoColaboradoresDTO> ObterResumoValidacao(Guid loteId, int clienteId);

        /// <summary>
        /// Efetiva a importação criando os registros no banco
        /// </summary>
        Task<ResultadoImportacaoColaboradoresDTO> EfetivarImportacao(Guid loteId, int clienteId, int usuarioId);

        /// <summary>
        /// Cancela e limpa dados de staging de um lote
        /// </summary>
        Task<bool> LimparStaging(Guid loteId, int clienteId);

        /// <summary>
        /// Obtém histórico de importações
        /// </summary>
        Task<List<HistoricoImportacaoDTO>> ObterHistorico(int clienteId, int? limite = 50);

        /// <summary>
        /// Gera template Excel para importação
        /// </summary>
        byte[] GerarTemplateExcel();

        /// <summary>
        /// Recriptografa documentos de colaboradores de um cliente
        /// </summary>
        Task<RecriptografarDocumentosResultadoDTO> RecriptografarDocumentosCliente(int clienteId, int usuarioId, bool incluirEmails = true);
    }
}

