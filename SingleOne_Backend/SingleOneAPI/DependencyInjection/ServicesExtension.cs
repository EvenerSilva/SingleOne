using Microsoft.Extensions.DependencyInjection;
using SingleOneAPI.Services;
using SingleOneAPI.Services.Interface;
using SingleOneAPI.Services.TinOne;

namespace SingleOneAPI.DependencyInjection
{
    public static class ServicesExtension
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IContratoService, ContratoService>();
            services.AddScoped<ISmtpConfigService, SmtpConfigService>();
            services.AddScoped<ITwoFactorService, TwoFactorService>();
            services.AddScoped<EstoqueCalculoService>();
            
            // ✅ TinOne - Assistente Inteligente (Isolado e opcional)
            services.AddScoped<ITinOneConfigService, TinOneConfigService>();
            services.AddScoped<ITinOneService, TinOneService>();
            services.AddSingleton<IOllamaService, OllamaService>(); // Singleton para reutilizar HttpClient
        }
    }
}
