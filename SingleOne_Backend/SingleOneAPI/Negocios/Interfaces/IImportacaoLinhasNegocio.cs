using Microsoft.AspNetCore.Http;
using SingleOneAPI.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IImportacaoLinhasNegocio
    {
        /// <summary>
        /// Processa arquivo de importação e valida dados
        /// </summary>
        Task<ResultadoValidacaoDTO> ProcessarArquivo(IFormFile arquivo, int clienteId, int usuarioId);
        
        /// <summary>
        /// Obtém detalhes da validação de um lote
        /// </summary>
        Task<List<DetalheLinhaStagingDTO>> ObterDetalhesValidacao(Guid loteId, int clienteId, string filtroStatus = null);
        
        /// <summary>
        /// Obtém resumo da validação
        /// </summary>
        Task<ResumoValidacaoDTO> ObterResumoValidacao(Guid loteId, int clienteId);
        
        /// <summary>
        /// Efetiva a importação criando registros definitivos
        /// </summary>
        Task<ResultadoImportacaoDTO> EfetivarImportacao(Guid loteId, int clienteId, int usuarioId);
        
        /// <summary>
        /// Cancela e limpa dados de staging de um lote
        /// </summary>
        Task<bool> LimparStaging(Guid loteId, int clienteId);
        
        /// <summary>
        /// Obtém histórico de importações
        /// </summary>
        Task<List<HistoricoImportacaoDTO>> ObterHistorico(int clienteId, int? limite = 50);
        
        /// <summary>
        /// Gera arquivo Excel template para importação
        /// </summary>
        byte[] GerarTemplateExcel();
    }
}

