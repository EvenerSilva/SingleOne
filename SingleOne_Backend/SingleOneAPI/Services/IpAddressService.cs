using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace SingleOneAPI.Services
{
    /// <summary>
    /// Serviço para obter o endereço IP real do cliente
    /// Considera proxies, load balancers e reverse proxies
    /// </summary>
    public interface IIpAddressService
    {
        string GetClientIpAddress(HttpContext context);
    }

    public class IpAddressService : IIpAddressService
    {
        /// <summary>
        /// Obtém o endereço IP real do cliente, considerando headers de proxy
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        /// <returns>Endereço IP do cliente</returns>
        public string GetClientIpAddress(HttpContext context)
        {
            try
            {
                // 1. Verificar headers de proxy/reverse proxy (ordem de prioridade)
                var forwardedFor = GetHeaderValue(context, "X-Forwarded-For");
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // X-Forwarded-For pode conter múltiplos IPs separados por vírgula
                    // O primeiro IP é geralmente o cliente original
                    var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var clientIp = ips.FirstOrDefault()?.Trim();
                    if (IsValidIpAddress(clientIp))
                    {
                        Console.WriteLine($"[IP_SERVICE] IP capturado via X-Forwarded-For: {clientIp}");
                        return clientIp;
                    }
                }

                var realIp = GetHeaderValue(context, "X-Real-IP");
                if (!string.IsNullOrEmpty(realIp) && IsValidIpAddress(realIp))
                {
                    Console.WriteLine($"[IP_SERVICE] IP capturado via X-Real-IP: {realIp}");
                    return realIp;
                }

                var forwarded = GetHeaderValue(context, "Forwarded");
                if (!string.IsNullOrEmpty(forwarded))
                {
                    // Header Forwarded pode conter: for=192.0.2.60;proto=http;by=203.0.113.43
                    var forPart = forwarded.Split(';')
                        .FirstOrDefault(p => p.Trim().StartsWith("for=", StringComparison.OrdinalIgnoreCase));
                    if (forPart != null)
                    {
                        var clientIp = forPart.Split('=')[1]?.Trim().Trim('"');
                        if (IsValidIpAddress(clientIp))
                        {
                            Console.WriteLine($"[IP_SERVICE] IP capturado via Forwarded: {clientIp}");
                            return clientIp;
                        }
                    }
                }

                // 2. Headers específicos de cloud providers
                var cfConnectingIp = GetHeaderValue(context, "CF-Connecting-IP"); // Cloudflare
                if (!string.IsNullOrEmpty(cfConnectingIp) && IsValidIpAddress(cfConnectingIp))
                {
                    Console.WriteLine($"[IP_SERVICE] IP capturado via CF-Connecting-IP: {cfConnectingIp}");
                    return cfConnectingIp;
                }

                var xClientIp = GetHeaderValue(context, "X-Client-IP");
                if (!string.IsNullOrEmpty(xClientIp) && IsValidIpAddress(xClientIp))
                {
                    Console.WriteLine($"[IP_SERVICE] IP capturado via X-Client-IP: {xClientIp}");
                    return xClientIp;
                }

                // 3. Fallback para RemoteIpAddress (conexão direta)
                var remoteIp = context.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrEmpty(remoteIp))
                {
                    // Filtrar IPs de localhost/loopback
                    if (remoteIp != "::1" && remoteIp != "127.0.0.1" && remoteIp != "localhost")
                    {
                        Console.WriteLine($"[IP_SERVICE] IP capturado via RemoteIpAddress: {remoteIp}");
                        return remoteIp;
                    }
                }

                // 4. Último recurso - usar localhost se nada mais funcionar
                Console.WriteLine($"[IP_SERVICE] ⚠️ Nenhum IP válido encontrado, usando fallback: 127.0.0.1");
                return "127.0.0.1";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IP_SERVICE] ❌ Erro ao obter IP do cliente: {ex.Message}");
                return "127.0.0.1"; // Fallback seguro
            }
        }

        /// <summary>
        /// Obtém valor de um header HTTP de forma segura
        /// </summary>
        private string GetHeaderValue(HttpContext context, string headerName)
        {
            try
            {
                if (context.Request.Headers.TryGetValue(headerName, out var headerValue))
                {
                    return headerValue.ToString();
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Valida se o endereço IP é válido e não é localhost
        /// </summary>
        private bool IsValidIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            // Filtrar IPs inválidos
            if (ip == "::1" || ip == "127.0.0.1" || ip == "localhost" || 
                ip == "unknown" || ip == "-" || ip.Contains("::"))
            {
                return false;
            }

            // Verificar se é um IP válido (IPv4 ou IPv6)
            try
            {
                if (System.Net.IPAddress.TryParse(ip, out var address))
                {
                    // Filtrar IPs privados/reservados em produção (opcional)
                    // Para desenvolvimento, permitir todos os IPs válidos
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
