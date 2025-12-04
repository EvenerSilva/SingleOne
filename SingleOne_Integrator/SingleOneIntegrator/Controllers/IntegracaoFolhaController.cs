using Microsoft.AspNetCore.Mvc;
using SingleOneIntegrator.Models;
using SingleOneIntegrator.Models.DTOs;
using SingleOneIntegrator.Services;
using System;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Controllers
{
    /// <summary>
    /// Controller para integração de folha de pagamento
    /// </summary>
    [ApiController]
    [Route("api/integracao/folha")]
    [Produces("application/json")]
    public class IntegracaoFolhaController : ControllerBase
    {
        private readonly ILogger<IntegracaoFolhaController> _logger;
        private readonly IRateLimitService _rateLimitService;
        private readonly IIntegracaoFolhaService _integracaoService;

        public IntegracaoFolhaController(
            ILogger<IntegracaoFolhaController> logger,
            IRateLimitService rateLimitService,
            IIntegracaoFolhaService integracaoService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
            _integracaoService = integracaoService ?? throw new ArgumentNullException(nameof(integracaoService));
        }

        /// <summary>
        /// Recebe dados da folha de pagamento
        /// </summary>
        /// <param name="request">Dados dos colaboradores</param>
        /// <returns>Resultado do processamento</returns>
        /// <response code="200">Processamento concluído com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="401">Não autorizado (API Key inválida ou assinatura inválida)</response>
        /// <response code="429">Limite de requisições excedido</response>
        /// <response code="500">Erro interno no processamento</response>
        [HttpPost]
        [ProducesResponseType(typeof(IntegracaoFolhaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReceberDadosFolha([FromBody] IntegracaoFolhaRequest request)
        {
            try
            {
                // 1. Obter cliente autenticado (setado pelo middleware)
                if (!HttpContext.Items.TryGetValue("ClienteIntegracao", out var clienteObj) || 
                    clienteObj is not ClienteIntegracao cliente)
                {
                    _logger.LogError("[INTEGRACAO-FOLHA] Cliente não encontrado no contexto");
                    return Unauthorized(new { success = false, error = "Cliente não autenticado" });
                }

                var ipOrigem = HttpContext.Items["IpOrigem"]?.ToString() ?? "Unknown";

                _logger.LogInformation($"[INTEGRACAO-FOLHA] Requisição recebida - Cliente: {cliente.ClienteId} - IP: {ipOrigem} - Colaboradores: {request.Colaboradores.Count}");

                // 2. Rate Limiting (10 requisições por minuto)
                if (!await _rateLimitService.CheckLimit(cliente.ApiKey, maxRequests: 10, windowSeconds: 60))
                {
                    _logger.LogWarning($"[INTEGRACAO-FOLHA] Rate limit excedido - Cliente: {cliente.ClienteId}");
                    return StatusCode(429, new 
                    { 
                        success = false, 
                        error = "Limite de requisições excedido. Máximo: 10 requisições por minuto.",
                        retryAfter = 60
                    });
                }

                // 3. Validar modelo (já feito automaticamente pelo [ApiController])
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[INTEGRACAO-FOLHA] Modelo inválido - Cliente: {cliente.ClienteId}");
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = "Dados inválidos",
                        errors = ModelState
                    });
                }

                // 4. Validar limites
                if (request.Colaboradores.Count > 1000)
                {
                    return BadRequest(new 
                    { 
                        success = false, 
                        error = "Máximo de 1000 colaboradores por requisição" 
                    });
                }

                // 5. Processar integração
                var response = await _integracaoService.ProcessarAsync(request, cliente, ipOrigem);

                _logger.LogInformation($"[INTEGRACAO-FOLHA] Processamento concluído - {response.IntegracaoId} - Processados: {response.Estatisticas.Total - response.Estatisticas.Erros} - Erros: {response.Estatisticas.Erros}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[INTEGRACAO-FOLHA] Erro no processamento");
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = "Erro interno no processamento",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Endpoint de health check
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new 
            { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                service = "IntegracaoFolha",
                version = "1.0.0"
            });
        }
    }
}


