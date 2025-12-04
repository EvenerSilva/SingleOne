using SingleOneIntegrator.Options;
using SingleOneIntegrator.Repository.Colaborador;
using SingleOneIntegrator.Repository.Integracao;
using SingleOneIntegrator.Services;

namespace SingleOneIntegrator
{
    public static class ServicesConfiguration
    {
        public static void AddCustomServices(this IServiceCollection services, IConfiguration config)
        {
            // Database Options
            var databaseOptions = new DatabaseOptions();
            config.Bind(nameof(DatabaseOptions), databaseOptions);
            services.AddSingleton(databaseOptions);

            // Repositories (Worker - VIEW)
            services.AddSingleton<IVwInventarioUsuarioRepository, VwInventarioUsuarioRepository>();

            // Repositories (API - Integração)
            services.AddSingleton<IClienteIntegracaoRepository, ClienteIntegracaoRepository>();
            services.AddSingleton<IIntegracaoFolhaLogRepository, IntegracaoFolhaLogRepository>();

            // Services
            services.AddSingleton<IRateLimitService, RateLimitService>();
            services.AddScoped<IIntegracaoFolhaService, IntegracaoFolhaService>();
        }
    }
}
