using Microsoft.Extensions.Caching.Memory;
using SingleOneIntegrator.Helpers;
using SingleOneIntegrator.Models;
using SingleOneIntegrator.Options;
using SingleOneIntegrator.Repository.Colaborador;
using System.Text;
using RabbitMQ.Client;
using System.Text.Json;

namespace SingleOneIntegrator
{
    public class Worker : BackgroundService
    {
        private const string integratorCacheKey = "VwInventarioUsuario_Cache";
        private const string integratorRabbitKey = "VwInventarioUsuario_Rabbit";
        private readonly ILogger<Worker> _logger;
        private readonly IMemoryCache _cache;
        private readonly IVwInventarioUsuarioRepository _repository;

        public Worker(ILogger<Worker> logger, IMemoryCache cache, IVwInventarioUsuarioRepository repository)
        {
            _logger = logger;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Worker Integrator running at: {time}", DateTimeOffset.Now);
                    await IntegrationVW();
                    //await Task.Delay(3600000, stoppingToken);
                    await Task.Delay(10000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }

        private async Task IntegrationVW()
        {
            var colaboradoresVw = await _repository.FindByQueryAsync("select * from \"VW_INVENTARIO_USUARIOS\" ORDER BY \"NomeCompleto\", \"CPF\"");
            var colaboradoresCache = await GetCache();

            var elementosDiferentes = colaboradoresVw.Except(colaboradoresCache, new VwInventarioUsuarioComparer()).AsEnumerable();
            if (elementosDiferentes.Any())
            {
                //Envia diferença para o RabbitMQ
                SendToReceptor(elementosDiferentes);

                //Remover lista antiga do Cache
                await DeleteCache();

                //Inclui nova lista no Cache
                SetCache(colaboradoresVw);
            }
        }

        private void SendToReceptor(IEnumerable<VwInventarioUsuario> elementosDiferentes)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: integratorRabbitKey,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            string message = JsonSerializer.Serialize(elementosDiferentes);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: integratorRabbitKey,
                                 basicProperties: null,
                                 body: body);
        }

        private async Task<IEnumerable<VwInventarioUsuario>> GetCache()
        {
            if (_cache.TryGetValue(integratorCacheKey, out IEnumerable<VwInventarioUsuario>? list))
            {
                return list;
            }
            else
            {
                return Enumerable.Empty<VwInventarioUsuario>();
            }
        }

        private Task DeleteCache()
        {
            if (_cache.TryGetValue(integratorCacheKey, out IEnumerable<VwInventarioUsuario>? list))
            {
                _cache.Remove(integratorCacheKey);
            }

            return Task.CompletedTask;
        }

        private void SetCache(IEnumerable<VwInventarioUsuario> colaboradoresCache)
        {
            //Inclui no Cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                //.SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(36000))
                .SetPriority(CacheItemPriority.Normal);
            _cache.Set(integratorCacheKey, colaboradoresCache, cacheEntryOptions);
        }
    }
}