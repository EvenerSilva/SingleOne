using Microsoft.AspNetCore.Mvc;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleOneAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampanhaAssinaturaController : ControllerBase
    {
        private readonly ICampanhaAssinaturaNegocio _negocio;
        private readonly HangfireJobService _hangfireJobService;

        public CampanhaAssinaturaController(
            ICampanhaAssinaturaNegocio negocio,
            HangfireJobService hangfireJobService)
        {
            _negocio = negocio;
            _hangfireJobService = hangfireJobService;
        }

        // ==================== CRUD BÁSICO ====================

        /// <summary>
        /// Criar nova campanha de assinaturas
        /// </summary>
        [HttpPost("CriarCampanha")]
        public IActionResult CriarCampanha([FromBody] CriarCampanhaRequest request)
        {
            try
            {
                var campanha = new CampanhaAssinatura
                {
                    Cliente = request.ClienteId,
                    UsuarioCriacao = request.UsuarioCriacaoId,
                    Nome = request.Nome,
                    Descricao = request.Descricao,
                    DataInicio = request.DataInicio,
                    DataFim = request.DataFim,
                    FiltrosJson = request.FiltrosJson
                };

                var resultado = _negocio.CriarCampanha(campanha, request.ColaboradoresIds);

                string jobIdConclusao = null;
                if (request.DataFim.HasValue)
                {
                    jobIdConclusao = _hangfireJobService.AgendarConclusaoCampanha(
                        resultado.Id,
                        request.DataFim.Value
                    );
                }

                string jobId = null;
                if (request.EnviarAutomaticamente)
                {
                    if (request.DataEnvioAgendado.HasValue)
                    {
                        jobId = _hangfireJobService.AgendarEnvioCampanha(
                            resultado.Id,
                            request.ColaboradoresIds,
                            request.DataEnvioAgendado.Value,
                            request.UsuarioCriacaoId,
                            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                            "Sistema"
                        );
                    }
                    else
                    {
                        jobId = _hangfireJobService.EnviarEmailsImediato(
                            resultado.Id,
                            request.ColaboradoresIds,
                            request.UsuarioCriacaoId,
                            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                            "Sistema"
                        );
                    }
                }

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Campanha criada com sucesso",
                    campanha = resultado,
                    jobId = jobId, // ID do job no Hangfire (para tracking de emails)
                    jobIdConclusao = jobIdConclusao, // ID do job de conclusão automática
                    envioAgendado = request.EnviarAutomaticamente,
                    dataEnvio = request.DataEnvioAgendado,
                    conclusaoAgendada = request.DataFim.HasValue,
                    dataConclusao = request.DataFim
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = $"Erro ao criar campanha: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obter campanha por ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult ObterCampanhaPorId(int id)
        {
            try
            {
                var campanha = _negocio.ObterCampanhaPorId(id);

                if (campanha == null)
                {
                    return NotFound(new
                    {
                        Sucesso = false,
                        Mensagem = "Campanha não encontrada"
                    });
                }

                return Ok(campanha);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao buscar campanha: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Listar campanhas por cliente
        /// </summary>
        [HttpGet("Cliente/{clienteId}")]
        public IActionResult ListarCampanhasPorCliente(int clienteId, [FromQuery] char? status = null)
        {
            try
            {
                var campanhas = _negocio.ListarCampanhasPorCliente(clienteId, status);
                return Ok(campanhas);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao listar campanhas: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Atualizar campanha
        /// </summary>
        [HttpPut("Atualizar")]
        public IActionResult AtualizarCampanha([FromBody] CampanhaAssinatura campanha)
        {
            try
            {
                _negocio.AtualizarCampanha(campanha);

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Campanha atualizada com sucesso"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao atualizar campanha: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Inativar campanha
        /// </summary>
        [HttpPut("Inativar/{id}")]
        public IActionResult InativarCampanha(int id)
        {
            try
            {
                _negocio.InativarCampanha(id);

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Campanha inativada com sucesso"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao inativar campanha: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Concluir campanha
        /// </summary>
        [HttpPut("Concluir/{id}")]
        public IActionResult ConcluirCampanha(int id)
        {
            try
            {
                _negocio.ConcluirCampanha(id);

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Campanha concluída com sucesso"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao concluir campanha: {ex.Message}"
                });
            }
        }

        // ==================== GERENCIAMENTO DE COLABORADORES ====================

        /// <summary>
        /// Adicionar colaboradores na campanha
        /// </summary>
        [HttpPost("{campanhaId}/AdicionarColaboradores")]
        public IActionResult AdicionarColaboradores(int campanhaId, [FromBody] List<int> colaboradoresIds)
        {
            try
            {
                _negocio.AdicionarColaboradoresNaCampanha(campanhaId, colaboradoresIds);

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = $"{colaboradoresIds.Count} colaborador(es) adicionado(s) com sucesso"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao adicionar colaboradores: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Remover colaborador da campanha
        /// </summary>
        [HttpDelete("{campanhaId}/RemoverColaborador/{colaboradorId}")]
        public IActionResult RemoverColaborador(int campanhaId, int colaboradorId)
        {
            try
            {
                _negocio.RemoverColaboradorDaCampanha(campanhaId, colaboradorId);

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Colaborador removido com sucesso"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao remover colaborador: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obter colaboradores da campanha
        /// </summary>
        [HttpGet("{campanhaId}/Colaboradores")]
        public IActionResult ObterColaboradoresDaCampanha(int campanhaId, [FromQuery] char? statusAssinatura = null)
        {
            try
            {
                var colaboradores = _negocio.ObterColaboradoresDaCampanha(campanhaId, statusAssinatura);
                return Ok(colaboradores);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao buscar colaboradores: {ex.Message}"
                });
            }
        }

        // ==================== ENVIO DE TERMOS ====================

        /// <summary>
        /// Enviar termo para colaborador específico
        /// </summary>
        [HttpPost("{campanhaId}/EnviarTermo/{colaboradorId}")]
        public IActionResult EnviarTermo(int campanhaId, int colaboradorId, [FromBody] EnvioTermoRequest request)
        {
            try
            {
                var sucesso = _negocio.EnviarTermoParaColaborador(
                    campanhaId, 
                    colaboradorId, 
                    request.UsuarioEnvioId, 
                    request.Ip, 
                    request.Localizacao
                );

                if (sucesso)
                {
                    return Ok(new
                    {
                        Sucesso = true,
                        Mensagem = "Termo enviado com sucesso"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Sucesso = false,
                        Mensagem = "Falha ao enviar termo"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao enviar termo: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Enviar termos em massa
        /// </summary>
        [HttpPost("{campanhaId}/EnviarTermosEmMassa")]
        public IActionResult EnviarTermosEmMassa(int campanhaId, [FromBody] EnvioMassaRequest request)
        {
            try
            {
                var sucesso = _negocio.EnviarTermosEmMassa(
                    campanhaId, 
                    request.ColaboradoresIds, 
                    request.UsuarioEnvioId, 
                    request.Ip, 
                    request.Localizacao
                );

                return Ok(new
                {
                    Sucesso = sucesso,
                    Mensagem = sucesso ? "Termos enviados com sucesso" : "Alguns envios falharam"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao enviar termos em massa: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Marcar como assinado
        /// </summary>
        [HttpPut("{campanhaId}/MarcarAssinado/{colaboradorId}")]
        public IActionResult MarcarComoAssinado(int campanhaId, int colaboradorId)
        {
            try
            {
                _negocio.MarcarComoAssinado(campanhaId, colaboradorId);

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Marcado como assinado com sucesso"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao marcar como assinado: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Atualizar estatísticas da campanha
        /// </summary>
        [HttpPost("{campanhaId}/AtualizarEstatisticas")]
        public IActionResult AtualizarEstatisticas(int campanhaId)
        {
            try
            {
                _negocio.AtualizarEstatisticasCampanha(campanhaId);

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = "Estatísticas atualizadas com sucesso"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao atualizar estatísticas: {ex.Message}"
                });
            }
        }

        // ==================== RELATÓRIOS ====================

        /// <summary>
        /// Obter resumo da campanha
        /// </summary>
        [HttpGet("{id}/Resumo")]
        public IActionResult ObterResumoCampanha(int id)
        {
            try
            {
                var resumo = _negocio.ObterResumoCampanha(id);

                if (resumo == null)
                {
                    return NotFound(new
                    {
                        Sucesso = false,
                        Mensagem = "Campanha não encontrada"
                    });
                }

                return Ok(resumo);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao obter resumo: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obter resumo de campanhas por cliente
        /// </summary>
        [HttpGet("Cliente/{clienteId}/Resumos")]
        public IActionResult ObterResumosCampanhas(int clienteId)
        {
            try
            {
                var resumos = _negocio.ObterResumoCampanhasPorCliente(clienteId);
                return Ok(resumos);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao obter resumos: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obter relatório de aderência
        /// </summary>
        [HttpGet("{id}/RelatorioAderencia")]
        public IActionResult ObterRelatorioAderencia(int id)
        {
            try
            {
                var relatorio = _negocio.ObterRelatorioAderencia(id);

                if (relatorio == null)
                {
                    return NotFound(new
                    {
                        Sucesso = false,
                        Mensagem = "Campanha não encontrada"
                    });
                }

                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao obter relatório: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obter colaboradores pendentes
        /// </summary>
        [HttpGet("{id}/Pendentes")]
        public IActionResult ObterColaboradoresPendentes(int id)
        {
            try
            {
                var pendentes = _negocio.ObterColaboradoresPendentes(id);
                return Ok(pendentes);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao obter pendentes: {ex.Message}"
                });
            }
        }

        // ==================== HANGFIRE / AGENDAMENTO ====================

        /// <summary>
        /// Obter estatísticas do Hangfire (para dashboard)
        /// </summary>
        [HttpGet("Hangfire/Estatisticas")]
        public IActionResult ObterEstatisticasHangfire()
        {
            try
            {
                var monitoring = Hangfire.JobStorage.Current.GetMonitoringApi();
                    
                var estatisticas = new
                    {
                        // Estatísticas gerais
                        jobsEnfileirados = monitoring.EnqueuedCount("default"),
                        jobsAgendados = monitoring.ScheduledCount(),
                        jobsProcessando = monitoring.ProcessingCount(),
                        jobsSucesso = monitoring.SucceededListCount(),
                        jobsFalhados = monitoring.FailedCount(),
                        
                        // Servidores ativos
                        servidores = monitoring.Servers().Count,
                        
                        // Filas
                        filas = monitoring.Queues().Select(q => new
                        {
                            nome = q.Name,
                            enfileirados = q.Length
                        }).ToList(),
                        
                        // Jobs agendados próximos (próximas 24h)
                        proximosJobs = monitoring.ScheduledJobs(0, 50)
                            .Where(j => j.Value.EnqueueAt < DateTime.UtcNow.AddDays(1))
                            .Select(j => new
                            {
                                jobId = j.Key,
                                metodo = j.Value.Job?.Method?.Name ?? "Desconhecido",
                                dataExecucao = j.Value.EnqueueAt
                            }).ToList(),
                        
                        // Última atualização
                        ultimaAtualizacao = DateTime.Now
                    };
                    
                return Ok(estatisticas);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao obter estatísticas: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Processar manualmente campanhas vencidas (concluir automaticamente)
        /// </summary>
        [HttpPost("ProcessarVencidas")]
        public IActionResult ProcessarCampanhasVencidas([FromBody] ProcessarVencidasRequest request)
        {
            try
            {
                var campanhasProcessadas = new List<object>();
                var dataAtual = DateTime.Now.Date;
                
                // Buscar todas as campanhas ativas que já venceram
                var campanhas = _negocio.ListarCampanhasPorCliente(request.ClienteId, 'A'); // Apenas Ativas
                var campanhasVencidas = campanhas.Where(c => 
                    c.DataFim.HasValue && 
                    c.DataFim.Value.Date <= dataAtual
                ).ToList();
                
                Console.WriteLine($"[CAMPANHA-VENCIDA] Encontradas {campanhasVencidas.Count} campanhas vencidas para processar");
                
                foreach (var campanha in campanhasVencidas)
                {
                    try
                    {
                        Console.WriteLine($"[CAMPANHA-VENCIDA] Processando campanha {campanha.Id} - '{campanha.Nome}' (Venceu em: {campanha.DataFim:dd/MM/yyyy})");
                        
                        _negocio.ConcluirCampanha(campanha.Id);
                        
                        campanhasProcessadas.Add(new
                        {
                            id = campanha.Id,
                            nome = campanha.Nome,
                            dataFim = campanha.DataFim,
                            diasAtraso = (dataAtual - campanha.DataFim.Value.Date).Days,
                            status = "Concluída"
                        });
                        
                        Console.WriteLine($"[CAMPANHA-VENCIDA] ✓ Campanha {campanha.Id} concluída com sucesso");
                    }
                    catch (Exception exCampanha)
                    {
                        Console.WriteLine($"[CAMPANHA-VENCIDA] ✗ Erro ao processar campanha {campanha.Id}: {exCampanha.Message}");
                        campanhasProcessadas.Add(new
                        {
                            id = campanha.Id,
                            nome = campanha.Nome,
                            dataFim = campanha.DataFim,
                            status = "Erro",
                            erro = exCampanha.Message
                        });
                    }
                }
                
                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = $"{campanhasProcessadas.Count} campanha(s) processada(s)",
                    TotalEncontradas = campanhasVencidas.Count,
                    Campanhas = campanhasProcessadas,
                    DataProcessamento = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CAMPANHA-VENCIDA] Erro geral: {ex.Message}");
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao processar campanhas: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Recalcular estatísticas de todas as campanhas ativas (aplica novo filtro de recursos ativos)
        /// </summary>
        [HttpPost("RecalcularEstatisticas")]
        public IActionResult RecalcularEstatisticasCampanhas([FromQuery] int clienteId)
        {
            try
            {
                var todasCampanhas = _negocio.ListarCampanhasPorCliente(clienteId);

                int recalculadas = 0;
                foreach (var campanha in todasCampanhas)
                {
                    try
                    {
                        _negocio.AtualizarEstatisticasCampanha(campanha.Id);
                        recalculadas++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CAMPANHA-CONTROLLER] Erro ao recalcular campanha {campanha.Id}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    Sucesso = true,
                    Mensagem = $"{recalculadas} campanhas recalculadas com sucesso",
                    TotalCampanhas = todasCampanhas.Count,
                    Recalculadas = recalculadas
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao recalcular estatísticas: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cancelar job agendado
        /// </summary>
        [HttpDelete("Hangfire/Job/{jobId}")]
        public IActionResult CancelarJob(string jobId)
        {
            try
            {
                var resultado = _hangfireJobService.CancelarJob(jobId);
                
                if (resultado)
                {
                    return Ok(new
                    {
                        sucesso = true,
                        mensagem = "Job cancelado com sucesso"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        sucesso = false,
                        mensagem = "Não foi possível cancelar o job"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = $"Erro ao cancelar job: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Reprocessar campanha - Envia/reenvia emails de uma campanha existente
        /// Pode ser imediato ou agendado usando Hangfire
        /// </summary>
        [HttpPost("{id}/Reprocessar")]
        public IActionResult ReprocessarCampanha(int id, [FromBody] ReprocessarCampanhaRequest request)
        {
            try
            {
                var campanha = _negocio.ObterCampanhaPorId(id);
                if (campanha == null)
                {
                    return NotFound(new
                    {
                        sucesso = false,
                        mensagem = "Campanha não encontrada"
                    });
                }

                var colaboradoresIds = request.ColaboradoresIds;
                if (colaboradoresIds == null || colaboradoresIds.Count == 0)
                {
                    var colaboradoresCampanha = _negocio.ObterColaboradoresDaCampanha(id);
                    colaboradoresIds = colaboradoresCampanha.Select(cc => cc.ColaboradorId).ToList();
                }

                string jobId = null;

                if (request.DataEnvioAgendado.HasValue)
                {
                    jobId = _hangfireJobService.AgendarEnvioCampanha(
                        id,
                        colaboradoresIds,
                        request.DataEnvioAgendado.Value,
                        request.UsuarioId,
                        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                        "Reprocessamento"
                    );
                }
                else
                {
                    jobId = _hangfireJobService.EnviarEmailsImediato(
                        id,
                        colaboradoresIds,
                        request.UsuarioId,
                        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                        "Reprocessamento"
                    );
                }

                return Ok(new
                {
                    sucesso = true,
                    mensagem = request.DataEnvioAgendado.HasValue 
                        ? $"Campanha agendada para reprocessamento em {request.DataEnvioAgendado.Value:dd/MM/yyyy HH:mm}"
                        : "Campanha em reprocessamento (envio imediato em background)",
                    jobId = jobId,
                    campanhaId = id,
                    totalColaboradores = colaboradoresIds.Count,
                    dataEnvio = request.DataEnvioAgendado
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = $"Erro ao reprocessar campanha: {ex.Message}"
                });
            }
        }
    }

    // ==================== REQUEST MODELS ====================

    public class CriarCampanhaRequest
    {
        public int ClienteId { get; set; }
        public int UsuarioCriacaoId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string FiltrosJson { get; set; }
        public List<int> ColaboradoresIds { get; set; }
        
        // 📧 Novos campos para envio automático/agendado
        public bool EnviarAutomaticamente { get; set; }
        public DateTime? DataEnvioAgendado { get; set; }
    }

    public class EnvioTermoRequest
    {
        public int UsuarioEnvioId { get; set; }
        public string Ip { get; set; }
        public string Localizacao { get; set; }
    }

    public class EnvioMassaRequest
    {
        public List<int> ColaboradoresIds { get; set; }
        public int UsuarioEnvioId { get; set; }
        public string Ip { get; set; }
        public string Localizacao { get; set; }
    }

    public class ReprocessarCampanhaRequest
    {
        /// <summary>
        /// ID do usuário que está reprocessando
        /// </summary>
        public int UsuarioId { get; set; }
        
        /// <summary>
        /// Lista de IDs dos colaboradores para reenviar
        /// Se null ou vazio, reenvia para TODOS os colaboradores da campanha
        /// </summary>
        public List<int> ColaboradoresIds { get; set; }
        
        /// <summary>
        /// Data/hora para agendar o envio
        /// Se null, envia IMEDIATAMENTE em background
        /// </summary>
        public DateTime? DataEnvioAgendado { get; set; }
    }
    
    public class ProcessarVencidasRequest
    {
        public int ClienteId { get; set; }
    }
}

