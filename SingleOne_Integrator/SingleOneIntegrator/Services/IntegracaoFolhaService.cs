using Microsoft.Extensions.Caching.Memory;
using RabbitMQ.Client;
using SingleOneIntegrator.Helpers;
using SingleOneIntegrator.Models;
using SingleOneIntegrator.Models.DTOs;
using SingleOneIntegrator.Repository.Integracao;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Services
{
    /// <summary>
    /// Serviço para processar integração de folha
    /// </summary>
    public interface IIntegracaoFolhaService
    {
        Task<IntegracaoFolhaResponse> ProcessarAsync(IntegracaoFolhaRequest request, ClienteIntegracao cliente, string ipOrigem);
    }

    public class IntegracaoFolhaService : IIntegracaoFolhaService
    {
        private const string rabbitQueueKey = "VwInventarioUsuario_Rabbit";
        private readonly ILogger<IntegracaoFolhaService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IIntegracaoFolhaLogRepository _logRepository;
        private readonly IClienteIntegracaoRepository _clienteRepository;

        public IntegracaoFolhaService(
            ILogger<IntegracaoFolhaService> logger,
            IMemoryCache cache,
            IIntegracaoFolhaLogRepository logRepository,
            IClienteIntegracaoRepository clienteRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        }

        /// <summary>
        /// Processa dados da folha
        /// </summary>
        public async Task<IntegracaoFolhaResponse> ProcessarAsync(
            IntegracaoFolhaRequest request, 
            ClienteIntegracao cliente, 
            string ipOrigem)
        {
            var stopwatch = Stopwatch.StartNew();
            var integracaoId = $"int-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var erros = new List<IntegracaoErro>();

            _logger.LogInformation($"[INTEGRACAO-FOLHA] Iniciando processamento {integracaoId} - Cliente: {cliente.ClienteId} - Total: {request.Colaboradores.Count}");

            try
            {
                // 1. Validar colaboradores
                var colaboradoresValidos = new List<VwInventarioUsuario>();
                int linha = 0;

                foreach (var colab in request.Colaboradores)
                {
                    linha++;
                    
                    // Validar CPF
                    var cpfSanitizado = CpfValidator.Sanitize(colab.Cpf);
                    if (!CpfValidator.IsValid(cpfSanitizado))
                    {
                        erros.Add(new IntegracaoErro
                        {
                            Linha = linha,
                            Cpf = colab.Cpf,
                            Nome = colab.NomeCompleto,
                            Erro = "CPF inválido"
                        });
                        continue;
                    }

                    // Converter DTO para modelo interno
                    colaboradoresValidos.Add(new VwInventarioUsuario
                    {
                        NomeCompleto = colab.NomeCompleto,
                        NomeDeUsuario = colab.NomeDeUsuario ?? string.Empty,
                        CentroDeCusto = colab.CentroCusto ?? string.Empty,
                        TxtCentroDeCusto = colab.TxtCentroCusto ?? string.Empty,
                        Cargo = colab.Cargo ?? string.Empty,
                        Matricula = colab.Matricula ?? string.Empty,
                        DataDeAdmissao = colab.DataAdmissao,
                        DataDeDemissao = colab.DataDemissao,
                        Empresa = colab.Empresa ?? string.Empty,
                        Cpf = cpfSanitizado,
                        Cnpj = colab.CnpjEmpresa ?? string.Empty,
                        TipoDeColaborador = colab.TipoColaborador ?? string.Empty,
                        Status = colab.Status ?? string.Empty,
                        Cidade = colab.Cidade ?? string.Empty,
                        Estado = colab.Estado ?? string.Empty,
                        NomeFantasia = colab.NomeFantasia ?? string.Empty,
                        EmailCorporativo = colab.Email ?? string.Empty,
                        Superior = colab.Superior ?? string.Empty
                    });
                }

                _logger.LogInformation($"[INTEGRACAO-FOLHA] {integracaoId} - Válidos: {colaboradoresValidos.Count} | Erros: {erros.Count}");

                // 2. Detectar mudanças (comparar com cache se for INCREMENTAL)
                List<VwInventarioUsuario> colaboradoresParaEnviar;

                if (request.TipoOperacao == "INCREMENTAL")
                {
                    var colaboradoresCache = await GetCacheColaboradores(cliente.ClienteId);
                    colaboradoresParaEnviar = colaboradoresValidos
                        .Except(colaboradoresCache, new VwInventarioUsuarioComparer())
                        .ToList();

                    _logger.LogInformation($"[INTEGRACAO-FOLHA] {integracaoId} - Modo INCREMENTAL: {colaboradoresParaEnviar.Count} mudanças detectadas");
                }
                else
                {
                    // FULL_SYNC: enviar todos
                    colaboradoresParaEnviar = colaboradoresValidos;
                    _logger.LogInformation($"[INTEGRACAO-FOLHA] {integracaoId} - Modo FULL_SYNC: enviando todos os {colaboradoresParaEnviar.Count} colaboradores");
                }

                // 3. Enviar para RabbitMQ (se houver mudanças)
                int novos = 0, atualizados = 0;

                if (colaboradoresParaEnviar.Any())
                {
                    await SendToRabbitMQ(colaboradoresParaEnviar, cliente.ClienteId);
                    
                    // Atualizar cache
                    await SetCacheColaboradores(cliente.ClienteId, colaboradoresValidos);
                    
                    // Por simplicidade, considerar todos como "atualizados"
                    // Em produção, você pode refinar isso comparando com banco de dados
                    atualizados = colaboradoresParaEnviar.Count;
                    
                    _logger.LogInformation($"[INTEGRACAO-FOLHA] {integracaoId} - Enviado para RabbitMQ: {colaboradoresParaEnviar.Count} colaboradores");
                }
                else
                {
                    _logger.LogInformation($"[INTEGRACAO-FOLHA] {integracaoId} - Nenhuma mudança detectada");
                }

                // 4. Atualizar data última sincronização
                await _clienteRepository.UpdateUltimaSincronizacaoAsync(cliente.Id);

                stopwatch.Stop();

                // 5. Salvar log
                await SalvarLog(integracaoId, cliente, ipOrigem, request, colaboradoresValidos.Count, erros.Count, 
                    stopwatch.ElapsedMilliseconds, 200, true, "Processamento concluído com sucesso");

                // 6. Retornar response
                return new IntegracaoFolhaResponse
                {
                    Success = true,
                    IntegracaoId = integracaoId,
                    Timestamp = DateTime.UtcNow,
                    Estatisticas = new IntegracaoEstatisticas
                    {
                        Total = request.Colaboradores.Count,
                        Novos = novos,
                        Atualizados = atualizados,
                        Erros = erros.Count,
                        TempoProcessamento = (int)stopwatch.ElapsedMilliseconds
                    },
                    Erros = erros.Any() ? erros : null,
                    Mensagem = erros.Any() 
                        ? $"Processamento concluído com {erros.Count} erro(s)" 
                        : "Processamento concluído com sucesso"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"[INTEGRACAO-FOLHA] Erro no processamento {integracaoId}");

                await SalvarLog(integracaoId, cliente, ipOrigem, request, 0, request.Colaboradores.Count, 
                    stopwatch.ElapsedMilliseconds, 500, false, ex.Message);

                throw;
            }
        }

        /// <summary>
        /// Envia colaboradores para RabbitMQ
        /// </summary>
        private async Task SendToRabbitMQ(IEnumerable<VwInventarioUsuario> colaboradores, int clienteId)
        {
            await Task.Run(() =>
            {
                try
                {
                    var factory = new ConnectionFactory { HostName = "localhost" };
                    using var connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();

                    // Usar fila específica do cliente
                    var queueName = $"{rabbitQueueKey}_{clienteId}";

                    channel.QueueDeclare(queue: queueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = JsonSerializer.Serialize(colaboradores);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: string.Empty,
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);

                    _logger.LogInformation($"[INTEGRACAO-FOLHA] Enviado para RabbitMQ fila: {queueName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[INTEGRACAO-FOLHA] Erro ao enviar para RabbitMQ");
                    throw;
                }
            });
        }

        /// <summary>
        /// Obtém colaboradores do cache
        /// </summary>
        private Task<IEnumerable<VwInventarioUsuario>> GetCacheColaboradores(int clienteId)
        {
            var cacheKey = $"IntegracaoFolha_Cliente_{clienteId}";
            
            if (_cache.TryGetValue(cacheKey, out IEnumerable<VwInventarioUsuario>? list))
            {
                return Task.FromResult(list ?? Enumerable.Empty<VwInventarioUsuario>());
            }

            return Task.FromResult(Enumerable.Empty<VwInventarioUsuario>());
        }

        /// <summary>
        /// Salva colaboradores no cache
        /// </summary>
        private Task SetCacheColaboradores(int clienteId, IEnumerable<VwInventarioUsuario> colaboradores)
        {
            var cacheKey = $"IntegracaoFolha_Cliente_{clienteId}";
            
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24))
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, colaboradores, cacheOptions);
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Salva log de auditoria
        /// </summary>
        private async Task SalvarLog(string integracaoId, ClienteIntegracao cliente, string ipOrigem,
            IntegracaoFolhaRequest request, int processados, int erros, long tempoMs, int statusCode, 
            bool sucesso, string mensagem)
        {
            try
            {
                var log = new IntegracaoFolhaLog
                {
                    IntegracaoId = integracaoId,
                    ClienteId = cliente.ClienteId,
                    DataHora = DateTime.UtcNow,
                    IpOrigem = ipOrigem,
                    ApiKey = cliente.ApiKey,
                    TipoOperacao = request.TipoOperacao,
                    ColaboradoresEnviados = request.Colaboradores.Count,
                    ColaboradoresProcessados = processados,
                    ColaboradoresErro = erros,
                    Erros = erros > 0 ? JsonSerializer.Serialize(erros) : null,
                    TempoProcessamento = (int)tempoMs,
                    StatusCode = statusCode,
                    Sucesso = sucesso,
                    Mensagem = mensagem
                };

                await _logRepository.CreateAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[INTEGRACAO-FOLHA] Erro ao salvar log de auditoria");
            }
        }
    }
}


