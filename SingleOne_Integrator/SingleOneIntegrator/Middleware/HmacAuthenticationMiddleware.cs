using Microsoft.AspNetCore.Http;
using SingleOneIntegrator.Helpers;
using SingleOneIntegrator.Repository.Integracao;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Middleware
{
    /// <summary>
    /// Middleware para autenticação HMAC das requisições de integração
    /// </summary>
    public class HmacAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HmacAuthenticationMiddleware> _logger;

        public HmacAuthenticationMiddleware(RequestDelegate next, ILogger<HmacAuthenticationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context, IClienteIntegracaoRepository clienteRepository)
        {
            // Apenas aplicar middleware em rotas de integração
            if (!context.Request.Path.StartsWithSegments("/api/integracao"))
            {
                await _next(context);
                return;
            }

            // Permitir OPTIONS (CORS preflight)
            if (context.Request.Method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            try
            {
                // 1. Extrair headers
                if (!context.Request.Headers.TryGetValue("X-SingleOne-ApiKey", out var apiKeyHeader) ||
                    string.IsNullOrWhiteSpace(apiKeyHeader))
                {
                    await ReturnUnauthorized(context, "Header X-SingleOne-ApiKey ausente");
                    return;
                }

                if (!context.Request.Headers.TryGetValue("X-SingleOne-Timestamp", out var timestampHeader) ||
                    !long.TryParse(timestampHeader, out var timestamp))
                {
                    await ReturnUnauthorized(context, "Header X-SingleOne-Timestamp inválido");
                    return;
                }

                if (!context.Request.Headers.TryGetValue("X-SingleOne-Signature", out var signatureHeader) ||
                    string.IsNullOrWhiteSpace(signatureHeader))
                {
                    await ReturnUnauthorized(context, "Header X-SingleOne-Signature ausente");
                    return;
                }

                var apiKey = apiKeyHeader.ToString();
                var signature = signatureHeader.ToString();

                // 2. Buscar cliente pela API Key
                var cliente = await clienteRepository.GetByApiKeyAsync(apiKey);
                if (cliente == null)
                {
                    _logger.LogWarning($"[HMAC-AUTH] API Key inválida: {apiKey}");
                    await ReturnUnauthorized(context, "API Key inválida");
                    return;
                }

                if (!cliente.Ativo)
                {
                    _logger.LogWarning($"[HMAC-AUTH] Integração inativa para cliente {cliente.ClienteId}");
                    await ReturnUnauthorized(context, "Integração inativa");
                    return;
                }

                // 3. Validar timestamp (prevenir replay attacks)
                if (!HmacHelper.ValidateTimestamp(timestamp, maxDifferenceSeconds: 300))
                {
                    _logger.LogWarning($"[HMAC-AUTH] Timestamp expirado - Cliente: {cliente.ClienteId}");
                    await ReturnUnauthorized(context, "Timestamp expirado ou inválido");
                    return;
                }

                // 4. Ler body (necessário para validar HMAC)
                context.Request.EnableBuffering();
                string body;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset para o controller poder ler
                }

                // 5. Validar assinatura HMAC
                if (!HmacHelper.ValidateSignature(signature, timestamp, body, cliente.ApiSecret))
                {
                    _logger.LogWarning($"[HMAC-AUTH] Assinatura HMAC inválida - Cliente: {cliente.ClienteId}");
                    await ReturnUnauthorized(context, "Assinatura HMAC inválida");
                    return;
                }

                // 6. Validar IP Whitelist (se configurado)
                if (!string.IsNullOrWhiteSpace(cliente.IpWhitelist))
                {
                    var ipOrigem = GetClientIpAddress(context);
                    var ipsPermitidos = cliente.IpWhitelist.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(ip => ip.Trim())
                        .ToList();

                    if (!IsIpAllowed(ipOrigem, ipsPermitidos))
                    {
                        _logger.LogWarning($"[HMAC-AUTH] IP não autorizado: {ipOrigem} - Cliente: {cliente.ClienteId}");
                        await ReturnUnauthorized(context, "IP não autorizado");
                        return;
                    }
                }

                // 7. Autenticação bem-sucedida - adicionar cliente ao contexto
                context.Items["ClienteIntegracao"] = cliente;
                context.Items["IpOrigem"] = GetClientIpAddress(context);

                _logger.LogInformation($"[HMAC-AUTH] Autenticação bem-sucedida - Cliente: {cliente.ClienteId}");

                // 8. Continuar pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HMAC-AUTH] Erro no middleware de autenticação");
                await ReturnError(context, "Erro interno de autenticação", 500);
            }
        }

        /// <summary>
        /// Retorna erro 401 Unauthorized
        /// </summary>
        private async Task ReturnUnauthorized(HttpContext context, string message)
        {
            await ReturnError(context, message, 401);
        }

        /// <summary>
        /// Retorna erro com status code customizado
        /// </summary>
        private async Task ReturnError(HttpContext context, string message, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                error = message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);
        }

        /// <summary>
        /// Obtém IP do cliente
        /// </summary>
        private string GetClientIpAddress(HttpContext context)
        {
            // Tentar obter IP de headers de proxy
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                return ips[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp))
            {
                return realIp.Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Verifica se IP está na whitelist
        /// </summary>
        private bool IsIpAllowed(string ipOrigem, List<string> ipsPermitidos)
        {
            if (!ipsPermitidos.Any())
                return true;

            // Verificar match exato
            if (ipsPermitidos.Contains(ipOrigem))
                return true;

            // TODO: Implementar verificação de ranges CIDR (203.0.113.0/24)
            // Por enquanto, apenas match exato

            return false;
        }
    }
}


