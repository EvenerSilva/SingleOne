using SingleOne.Models.ViewModels;
using SingleOne.Models;
using System.Collections.Generic;
using SingleOneAPI.Models.ViewModels;
using SingleOneAPI.Models;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IConfiguracoesNegocio
    {
        /************************************************* CLIENTES ****************************************/
        List<Cliente> ListarClientes(string pesquisa);
        string SalvarCliente(Cliente cli);
        void ExcluirCliente(int id);

        /************************************************* EMPRESAS ****************************************/
        List<Empresa> ListarEmpresas(string pesquisa, int cliente);
        Empresa BuscarEmpresaPeloID(int id);
        string SalvarEmpresa(Empresa empresa);
        string ExcluirEmpresa(int id);

        /******************************************** CENTRO CUSTOS ****************************************/
        List<CentrocustoVM> ListarCentrosDeCustoVM(string pesquisa, int cliente);
        CentrocustoVM BuscarCentroCustoPorId(int id);
        List<CentrocustoVM> BuscarPorEmpresaId(int idEMpresa);
        string SalvarCentroCusto(CentrocustoVM cc);
        string ExcluirCentroCusto(int id);

        /******************************************** FORNECEDORES *****************************************/
        List<Fornecedore> ListarFornecedores(string pesquisa, int cliente);
        List<Fornecedore> ListarFornecedoresDestinadores(int cliente);
        string SalvarFornecedor(Fornecedore fornecedor);
        void ExcluirFornecedor(int id);

        /******************************************** TIPOS DE RECURSOS ************************************/
        List<Tipoequipamento> ListarTiposDeRecursos(string pesquisa, int cliente);
        List<Tipoaquisicao> ListarTiposAquisicao();
        string SalvarTipoRecurso(Tipoequipamento te);
        void ExcluirTipoRecurso(int idTipo, int idCliente);

        /******************************************** FABRICANTES ******************************************/
        List<Fabricante> ListarFabricantes(string pesquisa, int cliente);
        List<Fabricante> ListarFabricantesPorTipoRecurso(int tipo, int cliente);
        string SalvarFabricante(Fabricante fab);
        void ExcluirFabricante(int id);

        /******************************************** MODELOS **********************************************/
        List<Modelo> ListarModelos(string pesquisa, int cliente);
        List<Modelo> ListarModelosDoFabricante(int fabricante, int cliente);
        string SalvarModelo(Modelo md);
        void ExcluirModelo(int id);

        /******************************************** NOTAS FISCAIS ****************************************/
        List<NotaFiscalListagemVM> ListarNotasFiscais(string pesquisa, int cliente);
        Notasfiscai BuscarNotaPorId(int id);
        VisualizarNotaFiscalVM VisualizarNotaFiscal(int id);
        void SalvarNotaFiscal(Notasfiscai nf);
        void AdicionarItemNota(Notasfiscaisiten nfi);
        void ExcluirNotaFiscal(int id);
        void ExcluirItemNota(int id);
        void LiberarParaEstoque(NotaFiscalVM nf);

        /******************************************** LAUDOS ***********************************************/
        List<Vwlaudo> ListarLaudos(string pesquisa, int cliente);
        Laudo BuscarLaudoPorID(int id);
        void SalvarLaudo(Laudo laudo);
        void EncerrarLaudo(Laudo laudo);
        byte[] GerarLaudoEmPDF(int idLaudo, int? templateId = null);

        /**************************************** LAUDO EVIDÊNCIAS *****************************************/
        List<LaudoEvidencia> ListarEvidenciasLaudo(int laudoId);
        void SalvarEvidenciaLaudo(LaudoEvidencia evidencia);
        void ExcluirEvidenciaLaudo(int evidenciaId);
        void ReordenarEvidenciasLaudo(int laudoId, List<int> ordemEvidencias);
        int ObterProximaOrdemEvidencia(int laudoId);
        dynamic ObterEvidenciaPorId(int evidenciaId);

        /******************************************** LOCALIZAÇÃO ******************************************/
        List<Localidade> ListarLocalidade(int cliente);
        void SalvarLocalidade(Localidade local);
        void ExcluirLocalidade(int id);

        /******************************************** FILIAIS **********************************************/
        List<Filial> ListarFiliais(string pesquisa, int cliente);
        Filial BuscarFilialPeloID(int id);
        string SalvarFilial(Filial filial);
        string ExcluirFilial(int id);

        /******************************************** TEMPLATES ********************************************/
        List<Templatetipo> ListarTiposDeTemplate();
        List<Template> ListarTemplates(int cliente);
        List<Template> ListarTemplatesPorTipo(int cliente, int tipo);
        Template ObterTemplatePorId(int id);
        void SalvarTemplate(Template t);
        void ExcluirTemplate(int id);
        byte[] VisualizarTemplate(TemplateVM template);

        /******************************************** PARAMÊTROS *******************************************/
        Parametro ObterParametros(int cliente);
        void SalvarParametro(Parametro p);

        /**************************************** POLÍTICAS DE ELEGIBILIDADE ********************************/
        List<PoliticaElegibilidadeVM> ListarPoliticasElegibilidade(int cliente, string? tipoColaborador = null, int? tipoEquipamentoId = null);
        PoliticaElegibilidadeVM BuscarPoliticaElegibilidadePorId(int id);
        string SalvarPoliticaElegibilidade(PoliticaElegibilidade politica);
        string ExcluirPoliticaElegibilidade(int id);
        bool VerificarElegibilidade(int colaboradorId, int tipoEquipamentoId);
        List<dynamic> ListarTiposColaboradorDistintos();
    }

}
