using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SingleOne.Models;
using SingleOne.Negocios;
using SingleOneAPI.Infra.Repositorio.Views;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Negocios;
using SingleOneAPI.Models;
using SingleOneAPI.Services;

namespace SingleOneAPI.DependencyInjection
{
    public static class DIAntigasExtension
    {
        public static void AddClassesAntigasDI(this IServiceCollection services)
        {
            //Repositório
            services.AddTransient<IRepository<Colaboradore>, Repository<Colaboradore>>();
            services.AddTransient<IRepository<Usuario>, Repository<Usuario>>();
            services.AddTransient<IRepository<Template>, Repository<Template>>();
            services.AddTransient<IRepository<Requisico>, Repository<Requisico>>();
            services.AddTransient<IRepository<Empresa>, Repository<Empresa>>();
            services.AddTransient<IRepository<Descartecargo>, Repository<Descartecargo>>();
            services.AddTransient<IRepository<DescarteEvidencia>, Repository<DescarteEvidencia>>();
            services.AddTransient<IRepository<ProtocoloDescarte>, Repository<ProtocoloDescarte>>();
            services.AddTransient<IRepository<ProtocoloDescarteItem>, Repository<ProtocoloDescarteItem>>();
            services.AddTransient<IRepository<CargoConfianca>, Repository<CargoConfianca>>();
            services.AddTransient<IRepository<PoliticaElegibilidade>, Repository<PoliticaElegibilidade>>();
            services.AddTransient<IRepository<Cliente>, Repository<Cliente>>();
            services.AddTransient<IRepository<Tipoequipamento>, Repository<Tipoequipamento>>();
            services.AddTransient<IRepository<Tipoequipamentoscliente>, Repository<Tipoequipamentoscliente>>();
            services.AddTransient<IRepository<Centrocusto>, Repository<Centrocusto>>();
            services.AddTransient<IRepository<Fornecedore>, Repository<Fornecedore>>();
            services.AddTransient<IRepository<Fabricante>, Repository<Fabricante>>();
            services.AddTransient<IRepository<Modelo>, Repository<Modelo>>();
            services.AddTransient<IRepository<Notasfiscai>, Repository<Notasfiscai>>();
            services.AddTransient<IRepository<Notasfiscaisiten>, Repository<Notasfiscaisiten>>();
            services.AddTransient<IRepository<Equipamento>, Repository<Equipamento>>();
            services.AddTransient<IRepository<Equipamentohistorico>, Repository<Equipamentohistorico>>();
            services.AddTransient<IRepository<Equipamentosstatus>, Repository<Equipamentosstatus>>();
            services.AddTransient<IRepository<Laudo>, Repository<Laudo>>();
            services.AddTransient<IRepository<LaudoEvidencia>, Repository<LaudoEvidencia>>();
            services.AddTransient<IRepository<Requisicoesiten>, Repository<Requisicoesiten>>();
            services.AddTransient<IRepository<Localidade>, Repository<Localidade>>();
            services.AddTransient<IRepository<Filial>, Repository<Filial>>();
            services.AddTransient<IRepository<Templatetipo>, Repository<Templatetipo>>();
            services.AddTransient<IRepository<Parametro>, Repository<Parametro>>();
            services.AddTransient<IRepository<Contrato>, Repository<Contrato>>();
            services.AddTransient<IRepository<Equipamentoanexo>, Repository<Equipamentoanexo>>();
            services.AddTransient<IRepository<Telefoniacontrato>, Repository<Telefoniacontrato>>();
            services.AddTransient<IRepository<Telefonialinha>, Repository<Telefonialinha>>();
            services.AddTransient<IRepository<Telefoniaoperadora>, Repository<Telefoniaoperadora>>();
            services.AddTransient<IRepository<Telefoniaplano>, Repository<Telefoniaplano>>();
            services.AddTransient<IRepository<RegrasTemplate>, Repository<RegrasTemplate>>();
            services.AddTransient<IRepository<GeolocalizacaoAssinatura>, Repository<GeolocalizacaoAssinatura>>();
            services.AddTransient<IRepository<EstoqueMinimoEquipamento>, Repository<EstoqueMinimoEquipamento>>();
            services.AddTransient<IRepository<EstoqueMinimoLinha>, Repository<EstoqueMinimoLinha>>();
            services.AddTransient<IRepository<PatrimonioContestacao>, Repository<PatrimonioContestacao>>();
            services.AddTransient<IRepository<PatrimonioLogAcesso>, Repository<PatrimonioLogAcesso>>();
            services.AddTransient<IRepository<Tipoaquisicao>, Repository<Tipoaquisicao>>();
            services.AddTransient<IRepository<RequisicaoItemCompartilhado>, Repository<RequisicaoItemCompartilhado>>();
            
            // Repositórios para Sinalização de Suspeitas
            services.AddTransient<IRepository<SinalizacaoSuspeita>, Repository<SinalizacaoSuspeita>>();
            services.AddTransient<IRepository<HistoricoInvestigacao>, Repository<HistoricoInvestigacao>>();
            services.AddTransient<IRepository<MotivoSuspeita>, Repository<MotivoSuspeita>>();
            
            // Repositórios para Campanhas de Assinatura
            services.AddTransient<IRepository<CampanhaAssinatura>, Repository<CampanhaAssinatura>>();
            services.AddTransient<IRepository<CampanhaColaborador>, Repository<CampanhaColaborador>>();
            


            //Repositório - Views
            services.AddScoped<IReadOnlyRepository<VwUltimasRequisicaoBYOD>, ViewRepository<VwUltimasRequisicaoBYOD>>();
            services.AddScoped<IReadOnlyRepository<VwUltimasRequisicaoNaoBYOD>, ViewRepository<VwUltimasRequisicaoNaoBYOD>>();
            services.AddScoped<IReadOnlyRepository<Vwnadaconstum>, ViewRepository<Vwnadaconstum>>();
            services.AddScoped<IReadOnlyRepository<Termoscolaboradoresvm>, ViewRepository<Termoscolaboradoresvm>>();
            services.AddScoped<IReadOnlyRepository<Tipoaquisicao>, Repository<Tipoaquisicao>>();
            services.AddScoped<IReadOnlyRepository<Vwlaudo>, ViewRepository<Vwlaudo>>();
            services.AddScoped<IReadOnlyRepository<Equipamentovm>, ViewRepository<Equipamentovm>>();
            services.AddScoped<IReadOnlyRepository<Termoentregavm>, ViewRepository<Termoentregavm>>();
            services.AddScoped<IReadOnlyRepository<Vwexportacaoexcel>, ViewRepository<Vwexportacaoexcel>>();
            services.AddScoped<IReadOnlyRepository<Equipamentohistoricovm>, ViewRepository<Equipamentohistoricovm>>();
            services.AddScoped<IReadOnlyRepository<Requisicoesvm>, ViewRepository<Requisicoesvm>>();
            services.AddScoped<IReadOnlyRepository<Colaboradorhistoricovm>, ViewRepository<Colaboradorhistoricovm>>();
            services.AddScoped<IReadOnlyRepository<Vwequipamentosdetalhe>, ViewRepository<Vwequipamentosdetalhe>>();
            services.AddScoped<IReadOnlyRepository<Vwdevolucaoprogramadum>, ViewRepository<Vwdevolucaoprogramadum>>();
            services.AddScoped<IReadOnlyRepository<Vwequipamentoscomcolaboradoresdesligado>, ViewRepository<Vwequipamentoscomcolaboradoresdesligado>>();
            services.AddScoped<IReadOnlyRepository<Vwequipamentosstatus>, ViewRepository<Vwequipamentosstatus>>();
            services.AddScoped<IReadOnlyRepository<Requisicaoequipamentosvm>, ViewRepository<Requisicaoequipamentosvm>>();
            services.AddScoped<IReadOnlyRepository<Vwtelefonium>, ViewRepository<Vwtelefonium>>();
            services.AddScoped<IReadOnlyRepository<ColaboradoresVM>, ViewRepository<ColaboradoresVM>>();
            services.AddScoped<IReadOnlyRepository<CentrocustoVM>, ViewRepository<CentrocustoVM>>();
            services.AddScoped<IReadOnlyRepository<PlanosVM>, ViewRepository<PlanosVM>>();

            //Negócio
            services.AddScoped<SingleOneAPI.Negocios.Interfaces.IEquipamentoNegocio>(provider =>
                new SingleOne.Negocios.EquipamentoNegocio(
                    provider.GetRequiredService<EnvironmentApiSettings>(),
                    provider.GetRequiredService<IMapper>(),
                    provider.GetRequiredService<IRepository<Colaboradore>>(),
                    provider.GetRequiredService<IRepository<Equipamento>>(),
                    provider.GetRequiredService<IRepository<Equipamentohistorico>>(),
                    provider.GetRequiredService<IRepository<Equipamentosstatus>>(),
                    provider.GetRequiredService<IRepository<Laudo>>(),
                    provider.GetRequiredService<IRepository<Requisico>>(),
                    provider.GetRequiredService<IRepository<Requisicoesiten>>(),
                    provider.GetRequiredService<IRepository<Equipamentoanexo>>(),
                    provider.GetRequiredService<IRepository<Parametro>>(),
                    provider.GetRequiredService<IRepository<Contrato>>(),
                    provider.GetRequiredService<IReadOnlyRepository<Equipamentovm>>(),
                    provider.GetRequiredService<IReadOnlyRepository<Termoentregavm>>(),
                    provider.GetRequiredService<IReadOnlyRepository<Vwexportacaoexcel>>(),
                    provider.GetRequiredService<IRepository<Telefonialinha>>(),
                    provider.GetRequiredService<IRepository<Telefoniaplano>>(),
                    provider.GetRequiredService<IRepository<Telefoniacontrato>>(),
                    provider.GetRequiredService<IRepository<Telefoniaoperadora>>(),
                    provider.GetRequiredService<IRepository<Centrocusto>>(),
                    provider.GetRequiredService<IRepository<CargoConfianca>>(),
                    provider.GetRequiredService<IRepository<DescarteEvidencia>>()
                ));
            services.AddScoped<SingleOneAPI.Negocios.Interfaces.IConfiguracoesNegocio, SingleOne.Negocios.ConfiguracoesNegocio>();
            services.AddScoped<IColaboradorNegocio>(provider => 
                new ColaboradorNegocio(
                    provider.GetRequiredService<EnvironmentApiSettings>(),
                    provider.GetRequiredService<IRepository<Colaboradore>>(),
                    provider.GetRequiredService<IRepository<Usuario>>(),
                    provider.GetRequiredService<IRepository<Template>>(),
                    provider.GetRequiredService<IRepository<Requisico>>(),
                    provider.GetRequiredService<IRepository<Empresa>>(),
                    provider.GetRequiredService<IRepository<Descartecargo>>(),
                    provider.GetRequiredService<IRepository<CargoConfianca>>(),
                    provider.GetRequiredService<IReadOnlyRepository<VwUltimasRequisicaoBYOD>>(),
                    provider.GetRequiredService<IReadOnlyRepository<VwUltimasRequisicaoNaoBYOD>>(),
                    provider.GetRequiredService<IReadOnlyRepository<Vwnadaconstum>>(),
                    provider.GetRequiredService<IReadOnlyRepository<Termoscolaboradoresvm>>(),
                    provider.GetRequiredService<IReadOnlyRepository<ColaboradoresVM>>(),
                    provider.GetRequiredService<IRepository<GeolocalizacaoAssinatura>>(),
                    provider.GetRequiredService<IEquipamentoNegocio>(),
                    provider.GetRequiredService<ISmtpConfigService>(),
                    provider.GetRequiredService<IRepository<Requisicoesiten>>(),
                    provider.GetRequiredService<IRepository<Equipamento>>(),
                    provider.GetRequiredService<IRepository<RequisicaoItemCompartilhado>>()
                ));
            services.AddScoped<IRelatorioNegocio, RelatorioNegocio>();
            services.AddScoped<IRequisicoesNegocio, RequisicoesNegocio>();
            services.AddScoped<ITelefoniaNegocio, TelefoniaNegocio>();
            services.AddScoped<PatrimonioNegocio>(provider => 
                new PatrimonioNegocio(
                    provider.GetRequiredService<IRepository<PatrimonioContestacao>>(),
                    provider.GetRequiredService<IRepository<PatrimonioLogAcesso>>(),
                    provider.GetRequiredService<IRepository<Colaboradore>>(),
                    provider.GetRequiredService<IRepository<Equipamento>>(),
                    provider.GetRequiredService<IRepository<Telefonialinha>>()
                ));
            services.AddScoped<IProtocoloDescarteNegocio>(provider => 
                new ProtocoloDescarteNegocio(
                    provider.GetRequiredService<IRepository<ProtocoloDescarte>>(),
                    provider.GetRequiredService<IRepository<ProtocoloDescarteItem>>(),
                    provider.GetRequiredService<IRepository<DescarteEvidencia>>(),
                    provider.GetRequiredService<IRepository<Equipamento>>(),
                    provider.GetRequiredService<IRepository<Cliente>>(),
                    provider.GetRequiredService<IRepository<Usuario>>(),
                    provider.GetRequiredService<IRepository<CargoConfianca>>(),
                    provider.GetRequiredService<IRepository<Equipamentohistorico>>(),
                    provider.GetRequiredService<IRepository<Colaboradore>>(),
                    provider.GetRequiredService<IRepository<Template>>()
                ));
            services.AddScoped<IUsuarioNegocio>(provider => 
                new UsuarioNegocio(
                    provider.GetRequiredService<EnvironmentApiSettings>(),
                    provider.GetRequiredService<IRepository<Usuario>>(),
                    provider.GetRequiredService<IRepository<Parametro>>(),
                    provider.GetRequiredService<ISmtpConfigService>()
                ));
            
            // Negócio de Sinalização de Suspeitas
            services.AddScoped<ISinalizacaoSuspeitaNegocio, SinalizacaoSuspeitaNegocio>();
            
            // Negócio de Campanhas de Assinatura
            services.AddScoped<ICampanhaAssinaturaNegocio>(provider => 
                new CampanhaAssinaturaNegocio(
                    provider.GetRequiredService<IRepository<CampanhaAssinatura>>(),
                    provider.GetRequiredService<IRepository<CampanhaColaborador>>(),
                    provider.GetRequiredService<IRepository<Colaboradore>>(),
                    provider.GetRequiredService<IRepository<Usuario>>(),
                    provider.GetRequiredService<IRepository<Empresa>>(),
                    provider.GetRequiredService<IRepository<Localidade>>(),
                    provider.GetRequiredService<IColaboradorNegocio>(),
                    provider.GetRequiredService<IRepository<Equipamentohistorico>>()
                ));
            
            // Negócio de Importação de Linhas
            services.AddScoped<IImportacaoLinhasNegocio, ImportacaoLinhasNegocio>();
            
            // Negócio de Importação de Colaboradores
            services.AddScoped<IImportacaoColaboradoresNegocio, ImportacaoColaboradoresNegocio>();
        }
    }
}
