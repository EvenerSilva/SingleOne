using Microsoft.Extensions.DependencyInjection;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Repository;
using SingleOneAPI.Repository.Interfaces;
using SingleOneAPI.Negocios;
using SingleOneAPI.Negocios.Interfaces;

namespace SingleOneAPI.DependencyInjection
{
    public static class RepositoryExtension
    {
        public static void AddCustomRepositories(this IServiceCollection services)
        {
            services.AddTransient<IRepository<StatusContrato>, Repository<StatusContrato>>();
            services.AddTransient<IRepository<Contrato>, Repository<Contrato>>();
            services.AddTransient<ILaudoEvidenciaRepository, LaudoEvidenciaRepository>();
            
            // Repositórios de Estoque Mínimo
            services.AddTransient<IEstoqueMinimoEquipamentoRepository, EstoqueMinimoEquipamentoRepository>();
            services.AddTransient<IEstoqueMinimoLinhaRepository, EstoqueMinimoLinhaRepository>();
            
            // Negócio de Estoque Mínimo
            services.AddTransient<IEstoqueMinimoNegocio, EstoqueMinimoNegocio>();
        }
    }
}
