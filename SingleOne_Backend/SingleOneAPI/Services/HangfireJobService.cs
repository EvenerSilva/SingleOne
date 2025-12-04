using Hangfire;
using SingleOneAPI.Models;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleOneAPI.Services
{
    /// <summary>
    /// Serviço responsável por gerenciar jobs do Hangfire para campanhas de assinatura
    /// </summary>
    public class HangfireJobService
    {
        private readonly ICampanhaAssinaturaNegocio _campanhaNegocio;
        private readonly IColaboradorNegocio _colaboradorNegocio;

        public HangfireJobService(
            ICampanhaAssinaturaNegocio campanhaNegocio,
            IColaboradorNegocio colaboradorNegocio)
        {
            _campanhaNegocio = campanhaNegocio;
            _colaboradorNegocio = colaboradorNegocio;
        }

        /// <summary>
        /// Agenda o envio de emails para uma campanha
        /// </summary>
        public string AgendarEnvioCampanha(int campanhaId, List<int> colaboradoresIds, DateTime dataEnvio, int usuarioId, string ip, string localizacao)
        {
            Console.WriteLine($"[HANGFIRE] Agendando envio - Campanha: {campanhaId}, Data: {dataEnvio:dd/MM/yyyy HH:mm}");

            var jobId = BackgroundJob.Schedule(
                () => EnviarEmailsCampanha(campanhaId, colaboradoresIds, usuarioId, ip, localizacao),
                dataEnvio
            );

            Console.WriteLine($"[HANGFIRE] Job agendado: {jobId}");
            return jobId;
        }

        /// <summary>
        /// Envia emails imediatamente para uma campanha
        /// </summary>
        public string EnviarEmailsImediato(int campanhaId, List<int> colaboradoresIds, int usuarioId, string ip, string localizacao)
        {
            Console.WriteLine($"[HANGFIRE] Enviando imediato - Campanha: {campanhaId}");

            var jobId = BackgroundJob.Enqueue(
                () => EnviarEmailsCampanha(campanhaId, colaboradoresIds, usuarioId, ip, localizacao)
            );

            Console.WriteLine($"[HANGFIRE] Job enfileirado: {jobId}");
            return jobId;
        }

        /// <summary>
        /// Método executado pelo Hangfire para enviar os emails
        /// Este método é chamado pelo Hangfire automaticamente
        /// </summary>
        public void EnviarEmailsCampanha(int campanhaId, List<int> colaboradoresIds, int usuarioId, string ip, string localizacao)
        {
            try
            {
                Console.WriteLine($"[HANGFIRE-JOB] Iniciando envio - Campanha: {campanhaId}, Colaboradores: {colaboradoresIds.Count}");

                var sucesso = _campanhaNegocio.EnviarTermosEmMassa(
                    campanhaId,
                    colaboradoresIds,
                    usuarioId,
                    ip,
                    localizacao
                );

                if (sucesso)
                {
                    Console.WriteLine($"[HANGFIRE-JOB] Envio concluído com sucesso - Campanha: {campanhaId}");
                    _campanhaNegocio.AtualizarEstatisticasCampanha(campanhaId);
                }
                else
                {
                    Console.WriteLine($"[HANGFIRE-JOB] Alguns envios falharam - Campanha: {campanhaId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HANGFIRE-JOB] ERRO no envio - Campanha: {campanhaId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cancela um job agendado
        /// </summary>
        public bool CancelarJob(string jobId)
        {
            try
            {
                var result = BackgroundJob.Delete(jobId);
                Console.WriteLine($"[HANGFIRE] Job {jobId} cancelado: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HANGFIRE] Erro ao cancelar job {jobId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Agenda a conclusão automática de uma campanha para uma data específica
        /// </summary>
        public string AgendarConclusaoCampanha(int campanhaId, DateTime dataConclusao)
        {
            var dataExecucao = dataConclusao.Date.AddDays(1).AddSeconds(-1);
            
            var jobId = BackgroundJob.Schedule(
                () => ConcluirCampanhaAutomaticamente(campanhaId),
                dataExecucao
            );

            Console.WriteLine($"[HANGFIRE] Conclusão agendada - Campanha: {campanhaId}, Data: {dataExecucao:dd/MM/yyyy HH:mm:ss}, Job: {jobId}");
            return jobId;
        }

        /// <summary>
        /// Método executado pelo Hangfire para concluir automaticamente uma campanha
        /// </summary>
        public void ConcluirCampanhaAutomaticamente(int campanhaId)
        {
            try
            {
                var campanha = _campanhaNegocio.ObterCampanhaPorId(campanhaId);

                if (campanha == null)
                {
                    Console.WriteLine($"[HANGFIRE-JOB] Campanha {campanhaId} não encontrada");
                    return;
                }

                if (campanha.Status == 'C')
                {
                    return;
                }

                if (campanha.DataFim.HasValue && DateTime.Now.Date >= campanha.DataFim.Value.Date)
                {
                    _campanhaNegocio.ConcluirCampanha(campanhaId);
                    Console.WriteLine($"[HANGFIRE-JOB] Campanha {campanhaId} concluída automaticamente");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HANGFIRE-JOB] ERRO ao concluir campanha {campanhaId}: {ex.Message}");
                throw;
            }
        }
    }
}

