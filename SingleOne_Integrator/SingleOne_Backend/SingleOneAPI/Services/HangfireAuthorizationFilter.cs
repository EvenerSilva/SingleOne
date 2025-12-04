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
            var httpContext = context.GetHttpContext();
            
            // ⚠️ DESENVOLVIMENTO: Permite acesso livre (remover em produção)
            // TODO: Em produção, adicionar validação de token JWT ou role específica
            
            // Exemplo de validação simples:
            // return httpContext.User.Identity.IsAuthenticated && 
            //        httpContext.User.IsInRole("Admin");
            
            // Por enquanto, permite acesso local
            return httpContext.Request.IsLocal() || 
                   httpContext.Connection.LocalIpAddress?.ToString() == "127.0.0.1" ||
                   httpContext.Connection.LocalIpAddress?.ToString() == "::1";
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

