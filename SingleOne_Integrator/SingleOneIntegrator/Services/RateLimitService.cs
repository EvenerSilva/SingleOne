using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Services
{
    /// <summary>
    /// Serviço de Rate Limiting para prevenir abuso da API
    /// </summary>
    public interface IRateLimitService
    {
        Task<bool> CheckLimit(string clientKey, int maxRequests = 10, int windowSeconds = 60);
        Task ResetLimit(string clientKey);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitService> _logger;

        public RateLimitService(IMemoryCache cache, ILogger<RateLimitService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Verifica se o cliente está dentro do limite de requisições
        /// </summary>
        /// <param name="clientKey">Identificador do cliente (API Key ou IP)</param>
        /// <param name="maxRequests">Máximo de requisições permitidas</param>
        /// <param name="windowSeconds">Janela de tempo em segundos</param>
        /// <returns>True se está dentro do limite, False se excedeu</returns>
        public Task<bool> CheckLimit(string clientKey, int maxRequests = 10, int windowSeconds = 60)
        {
            if (string.IsNullOrWhiteSpace(clientKey))
                return Task.FromResult(false);

            var cacheKey = $"RateLimit_{clientKey}";
            
            if (!_cache.TryGetValue(cacheKey, out RateLimitInfo? info))
            {
                // Primeira requisição na janela
                info = new RateLimitInfo
                {
                    Count = 1,
                    WindowStart = DateTime.UtcNow
                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(windowSeconds));

                _cache.Set(cacheKey, info, cacheOptions);

                _logger.LogDebug($"Rate Limit: Cliente {clientKey} - 1/{maxRequests} requisições");
                return Task.FromResult(true);
            }

            // Verifica se ainda está na janela de tempo
            var elapsed = (DateTime.UtcNow - info.WindowStart).TotalSeconds;
            
            if (elapsed > windowSeconds)
            {
                // Nova janela
                info.Count = 1;
                info.WindowStart = DateTime.UtcNow;
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(windowSeconds));

                _cache.Set(cacheKey, info, cacheOptions);

                _logger.LogDebug($"Rate Limit: Cliente {clientKey} - Nova janela iniciada");
                return Task.FromResult(true);
            }

            // Incrementa contador
            info.Count++;

            if (info.Count > maxRequests)
            {
                _logger.LogWarning($"Rate Limit EXCEDIDO: Cliente {clientKey} - {info.Count}/{maxRequests} requisições em {elapsed:F0}s");
                return Task.FromResult(false);
            }

            _cache.Set(cacheKey, info);
            _logger.LogDebug($"Rate Limit: Cliente {clientKey} - {info.Count}/{maxRequests} requisições");
            
            return Task.FromResult(true);
        }

        /// <summary>
        /// Reseta o limite de um cliente (usado para testes ou admin)
        /// </summary>
        public Task ResetLimit(string clientKey)
        {
            var cacheKey = $"RateLimit_{clientKey}";
            _cache.Remove(cacheKey);
            _logger.LogInformation($"Rate Limit resetado para cliente {clientKey}");
            return Task.CompletedTask;
        }

        private class RateLimitInfo
        {
            public int Count { get; set; }
            public DateTime WindowStart { get; set; }
        }
    }
}


