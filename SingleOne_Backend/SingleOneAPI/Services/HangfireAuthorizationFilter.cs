using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace SingleOneAPI.Services
{
    /// <summary>
    /// Filtro de autorização para o Hangfire Dashboard
    /// Por padrão, permite acesso em ambientes de desenvolvimento
    /// Para produção, pode adicionar validação de token JWT ou autenticação específica
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // ⚠️ DEMO: liberar acesso geral ao Hangfire Dashboard
            // Em produção, substitua por uma validação mais restrita (Admin, IP, VPN, etc.)
            return true;

            var httpContext = context.GetHttpContext();
            
            // Verificar se usuário está autenticado
            if (httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                // Verificar se é Admin (via role ou claim)
                if (httpContext.User.IsInRole("Admin") || 
                    httpContext.User.HasClaim("adm", "true"))
                {
                    return true;
                }
            }
            
            // Permitir acesso local (desenvolvimento e servidor local)
            if (httpContext.Request.IsLocal())
            {
                return true;
            }
            
            // ⚠️ PRODUÇÃO: Permitir acesso via IP do servidor ou VPN
            // Descomente e configure os IPs permitidos:
            // var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
            // var allowedIps = new[] { "SEU_IP_SERVIDOR", "127.0.0.1", "::1" };
            // if (allowedIps.Contains(remoteIp))
            //     return true;
            
            // Por padrão, negar acesso em produção sem autenticação
            return false;
        }
    }

    /// <summary>
    /// Extensão para verificar se é conexão local
    /// </summary>
    public static class HttpRequestExtensions
    {
        public static bool IsLocal(this HttpRequest req)
        {
            var connection = req.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
                else
                {
                    return System.Net.IPAddress.IsLoopback(connection.RemoteIpAddress);
                }
            }

            // Para quando não há conexão remota (testes locais)
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }
    }
}

