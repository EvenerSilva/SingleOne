using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SingleOne.Enumeradores;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOneAPI.Models.ViewModels;
using SingleOne.Util;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Services;
using SingleOneAPI.Util;
using SingleOneAPI.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SingleOne.Negocios
{
    public class ConfiguracoesNegocio : IConfiguracoesNegocio
    {
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<Cliente> _clienteRepository;
        private readonly IRepository<Tipoequipamento> _tipoequipamentoRepository;
        private readonly IRepository<Tipoequipamentoscliente> _tipoequipamentoClienteRepository;
        private readonly IRepository<Empresa> _empresaRepository;
        private readonly IRepository<Centrocusto> _centroCustoRepository;
        private readonly IRepository<Fornecedore> _fornecedorRepository;
        private readonly IRepository<Fabricante> _fabricanteRepository;
        private readonly IRepository<Modelo> _modeloRepository;
        private readonly IRepository<Notasfiscai> _notafiscalRepository;
        private readonly IRepository<Notasfiscaisiten> _itensNotafiscalRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Equipamentohistorico> _equipamentohistoricoRepository;
        private readonly IRepository<Laudo> _laudoRepository;
        private readonly IRepository<LaudoEvidencia> _laudoEvidenciaRepository;
        private readonly IRepository<Requisico> _requisicaoRepository;
        private readonly IRepository<Requisicoesiten> _requisicaoItensRepository;
        private readonly IRepository<Localidade> _localidadeRepository;
        private readonly IRepository<Filial> _filialRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<Template> _templateRepository;
        private readonly IRepository<Templatetipo> _templatetipoRepository;
        private readonly IRepository<Parametro> _parametroRepository;
        private readonly IRepository<Contrato> _contratoRepository;
        private readonly IReadOnlyRepository<Tipoaquisicao> _tipoaquisicaoRepository;
        private readonly IReadOnlyRepository<Vwlaudo> _vwLaudoRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly IRepository<PoliticaElegibilidade> _politicaElegibilidadeRepository;

        private readonly IEquipamentoNegocio _equipamentoNegocio;

        public ConfiguracoesNegocio(
            IRepository<Usuario> usuarioRepository,
            IRepository<Cliente> clienteRepository,
            IRepository<Tipoequipamento> tipoequipamentoRepository,
            IRepository<Tipoequipamentoscliente> tipoequipamentoClienteRepository,
            IRepository<Empresa> empresaRepository,
            IRepository<Centrocusto> centroCustoRepository,
            IRepository<Fornecedore> fornecedorRepository,
            IRepository<Fabricante> fabricanteRepository,
            IRepository<Modelo> modeloRepository,
            IRepository<Notasfiscai> notafiscalRepository,
            IRepository<Notasfiscaisiten> itensNotafiscalRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Equipamentohistorico> equipamentohistoricoRepository,
            IRepository<Laudo> laudoRepository,
            IRepository<LaudoEvidencia> laudoEvidenciaRepository,
            IRepository<Requisico> requisicaoRepository,
            IRepository<Requisicoesiten> requisicaoItensRepository,
            IRepository<Localidade> localidadeRepository,
            IRepository<Filial> filialRepository,
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<Template> templateRepository,
            IRepository<Templatetipo> templatetipoRepository,
            IRepository<Parametro> parametroRepository,
            IRepository<Contrato> contratoRepository,
            IReadOnlyRepository<Tipoaquisicao> tipoaquisicaoRepository,
            IReadOnlyRepository<Vwlaudo> vwLaudoRepository,
            IFileUploadService fileUploadService,
            IRepository<PoliticaElegibilidade> politicaElegibilidadeRepository,

            IEquipamentoNegocio equipamentoNegocio)
        {
            _usuarioRepository = usuarioRepository;
            _clienteRepository = clienteRepository;
            _tipoequipamentoRepository = tipoequipamentoRepository;
            _tipoequipamentoClienteRepository = tipoequipamentoClienteRepository;
            _empresaRepository = empresaRepository;
            _centroCustoRepository = centroCustoRepository;
            _fornecedorRepository = fornecedorRepository;
            _fabricanteRepository = fabricanteRepository;
            _modeloRepository = modeloRepository;
            _notafiscalRepository = notafiscalRepository;
            _itensNotafiscalRepository = itensNotafiscalRepository;
            _equipamentoRepository = equipamentoRepository;
            _equipamentohistoricoRepository = equipamentohistoricoRepository;
            _laudoRepository = laudoRepository;
            _laudoEvidenciaRepository = laudoEvidenciaRepository;
            _requisicaoRepository = requisicaoRepository;
            _requisicaoItensRepository = requisicaoItensRepository;
            _localidadeRepository = localidadeRepository;
            _filialRepository = filialRepository;
            _colaboradorRepository = colaboradorRepository;
            _templateRepository = templateRepository;
            _templatetipoRepository = templatetipoRepository;
            _parametroRepository = parametroRepository;
            _contratoRepository = contratoRepository;
            _tipoaquisicaoRepository = tipoaquisicaoRepository;
            _vwLaudoRepository = vwLaudoRepository;
            _fileUploadService = fileUploadService;
            _politicaElegibilidadeRepository = politicaElegibilidadeRepository;

            _equipamentoNegocio = equipamentoNegocio;
        }

        /***************************************************************************************************/
        /************************************************* CLIENTES ****************************************/
        /***************************************************************************************************/
        #region Clientes
        public List<Cliente> ListarClientes(string pesquisa)
        {
            pesquisa = pesquisa.ToLower();
            var clis = _clienteRepository
                        .Buscar(x => ((pesquisa != "null") ?
                                x.Razaosocial.ToLower().Contains(pesquisa.ToLower()) || x.Cnpj.Contains(pesquisa) :
                                1 == 1))
                        .OrderBy(x => x.Razaosocial)
                        .ToList();
            return clis;
        }
        public string SalvarCliente(Cliente cli)
        {
            try
            {
                // Validação de CNPJ
                if (!string.IsNullOrWhiteSpace(cli.Cnpj))
                {
                    if (!CnpjValidator.IsValid(cli.Cnpj))
                    {
                        return JsonConvert.SerializeObject(new { 
                            Mensagem = "❌ CNPJ inválido! O CNPJ informado não é válido. Verifique se possui 14 dígitos e se os dígitos verificadores estão corretos.", 
                            Status = "400",
                            Tipo = "CNPJ_INVALIDO",
                            CNPJ_Informado = cli.Cnpj,
                            Sugestao = "Digite um CNPJ válido com 14 dígitos no formato XX.XXX.XXX/XXXX-XX"
                        });
                    }
                    
                    // Formata o CNPJ antes de salvar
                    cli.Cnpj = CnpjValidator.Format(cli.Cnpj);
                }
                
                var resultado = _clienteRepository.ExecuteInTransaction(() =>
                {
                    if (cli.Id == 0)
                    {
                        // Verificar se já existe cliente com mesmo CNPJ
                        var existe = _clienteRepository.Buscar(x => x.Cnpj == cli.Cnpj).Any();
                        
                        if (!existe)
                        {
                            cli.Ativo = true;
                            _clienteRepository.Adicionar(cli);
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "✅ Cliente salvo com sucesso! O cliente foi cadastrado no sistema e está disponível para uso.", 
                                Status = "200",
                                Tipo = "SUCESSO",
                                Cliente = cli.Razaosocial,
                                CNPJ = cli.Cnpj
                            });
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "⚠️ CNPJ já cadastrado! Este CNPJ já está sendo usado por outro cliente. Verifique se não é o mesmo cliente ou use um CNPJ diferente.", 
                                Status = "400.1",
                                Tipo = "CNPJ_DUPLICADO",
                                Sugestao = "Verifique se não está tentando cadastrar o mesmo cliente ou use um CNPJ diferente."
                            });
                        }
                    }
                    else
                    {
                        // Verificar se já existe cliente com mesmo CNPJ (excluindo o atual)
                        var existe = _clienteRepository.Buscar(x => x.Cnpj == cli.Cnpj && x.Id != cli.Id).Any();
                        
                        if (!existe)
                        {
                            _clienteRepository.Atualizar(cli);
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "✅ Cliente atualizado com sucesso! As informações foram modificadas e salvas no sistema.", 
                                Status = "200",
                                Tipo = "SUCESSO",
                                Cliente = cli.Razaosocial,
                                CNPJ = cli.Cnpj,
                                ID = cli.Id
                            });
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "⚠️ CNPJ já cadastrado! Este CNPJ já está sendo usado por outro cliente. Verifique se não é o mesmo cliente ou use um CNPJ diferente.", 
                                Status = "400.1",
                                Tipo = "CNPJ_DUPLICADO",
                                Sugestao = "Verifique se não está tentando cadastrar o mesmo cliente ou use um CNPJ diferente."
                            });
                        }
                    }
                });
                
                return resultado;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Mensagem = $"Erro ao salvar cliente: {ex.Message}", Status = "500" });
            }
        }
        public void ExcluirCliente(int id)
        {
            var cli = _clienteRepository.Buscar(x => x.Id == id).FirstOrDefault();
            try
            {
                _clienteRepository.Remover(cli);
            }
            catch (Exception)
            {
                cli.Ativo = false;
                _clienteRepository.Atualizar(cli);
            }
        }
        #endregion
        /***************************************************************************************************/
        /************************************************* EMPRESAS ****************************************/
        /***************************************************************************************************/
        #region Empresa
        public List<Empresa> ListarEmpresas(string pesquisa, int cliente)
        {
            try
            {
                // Buscar empresas sem Include primeiro para evitar problemas com FKs quebradas
                var query = _empresaRepository
                    .Buscar(e => e.Cliente == cliente);

                if (!string.IsNullOrEmpty(pesquisa) && pesquisa.ToLower() != "null")
                {
                    // Buscar localidades que correspondem à pesquisa
                    var localidadesPesquisa = _localidadeRepository
                        .Buscar(l => l.Cliente == cliente && 
                            (l.Descricao.ToLower().Contains(pesquisa.ToLower()) ||
                             (l.Cidade != null && l.Cidade.ToLower().Contains(pesquisa.ToLower()))))
                        .Select(l => l.Id)
                        .ToList();

                    query = query.Where(e => 
                        e.Nome.ToLower().Contains(pesquisa.ToLower()) ||
                        e.Cnpj.Contains(pesquisa) ||
                        (e.LocalidadeId.HasValue && localidadesPesquisa.Contains(e.LocalidadeId.Value))
                    );
                }

                var empresas = query.OrderBy(e => e.Nome).ToList();
                
                // Carregar relacionamentos manualmente apenas para empresas retornadas
                foreach (var empresa in empresas)
                {
                    try
                    {
                        // Carregar Localidade se existir
                        if (empresa.LocalidadeId.HasValue)
                        {
                            empresa.Localidade = _localidadeRepository
                                .Buscar(l => l.Id == empresa.LocalidadeId.Value)
                                .FirstOrDefault();
                        }
                        
                        // Calcular total de filiais
                        var totalFiliais = _filialRepository.Buscar(f => f.EmpresaId == empresa.Id && f.Ativo == true).Count();
                        empresa.TotalFiliais = totalFiliais;
                    }
                    catch (Exception ex)
                    {
                        empresa.TotalFiliais = 0;
                    }
                }
                
                return empresas;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public Empresa BuscarEmpresaPeloID(int id)
        {
            var empresa = _empresaRepository.Buscar(x => x.Id == id).FirstOrDefault();
            return empresa;
        }
        public string SalvarEmpresa(Empresa empresa)
        {
            try
            {
                // Validação de CNPJ
                if (!string.IsNullOrWhiteSpace(empresa.Cnpj))
                {
                    if (!CnpjValidator.IsValid(empresa.Cnpj))
                    {
                        return JsonConvert.SerializeObject(new { 
                            Mensagem = "❌ CNPJ inválido! O CNPJ informado não é válido. Verifique se possui 14 dígitos e se os dígitos verificadores estão corretos.", 
                            Status = "400",
                            Tipo = "CNPJ_INVALIDO",
                            CNPJ_Informado = empresa.Cnpj,
                            Sugestao = "Digite um CNPJ válido com 14 dígitos no formato XX.XXX.XXX/XXXX-XX"
                        });
                    }
                    
                    // Formata o CNPJ antes de salvar
                    empresa.Cnpj = CnpjValidator.Format(empresa.Cnpj);
                }
                
                var resultado = _empresaRepository.ExecuteInTransaction(() =>
                {
                    if (empresa.Id == 0)
                    {
                        // Forçar preenchimento dos campos de data para nova empresa
                        var agora = DateTime.Now;
                        empresa.CreatedAt = agora;
                        empresa.UpdatedAt = agora;
                        
                        // Verificar se já existe empresa com mesmo CNPJ no mesmo cliente
                        var existe = _empresaRepository.Buscar(x => x.Cnpj == empresa.Cnpj && x.Cliente == empresa.Cliente).Any();
                        
                        if (!existe)
                        {
                            _empresaRepository.Adicionar(empresa);
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "✅ Empresa salva com sucesso! A empresa foi cadastrada no sistema e está disponível para uso.", 
                                Status = "200",
                                Tipo = "SUCESSO",
                                Empresa = empresa.Nome,
                                CNPJ = empresa.Cnpj
                            });
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "⚠️ CNPJ já cadastrado! Este CNPJ já está sendo usado por outra empresa no mesmo cliente. Verifique se não é a mesma empresa ou use um CNPJ diferente.", 
                                Status = "400.1",
                                Tipo = "CNPJ_DUPLICADO",
                                Sugestao = "Verifique se não está tentando cadastrar a mesma empresa ou use um CNPJ diferente."
                            });
                        }
                    }
                    else
                    {
                        // Definir updated_at para empresa existente
                        empresa.UpdatedAt = DateTime.Now;
                        
                        // Verificar se já existe empresa com mesmo CNPJ no mesmo cliente (excluindo a atual)
                        var existe = _empresaRepository.Buscar(x => x.Cnpj == empresa.Cnpj && x.Cliente == empresa.Cliente && x.Id != empresa.Id).Any();
                        
                        if (!existe)
                        {
                            _empresaRepository.Atualizar(empresa);
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "✅ Empresa atualizada com sucesso! As informações foram modificadas e salvas no sistema.", 
                                Status = "200",
                                Tipo = "SUCESSO",
                                Empresa = empresa.Nome,
                                CNPJ = empresa.Cnpj,
                                ID = empresa.Id
                            });
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "⚠️ CNPJ já cadastrado! Este CNPJ já está sendo usado por outra empresa no mesmo cliente. Verifique se não é a mesma empresa ou use um CNPJ diferente.", 
                                Status = "400.1",
                                Tipo = "CNPJ_DUPLICADO",
                                Sugestao = "Verifique se não está tentando cadastrar a mesma empresa ou use um CNPJ diferente."
                            });
                        }
                    }
                });
                
                return resultado;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Mensagem = $"Erro ao salvar empresa: {ex.Message}", Status = "500" });
            }
        }
        public string ExcluirEmpresa(int id)
        {
            try
            {
                var empresa = _empresaRepository.Buscar(x => x.Id == id).FirstOrDefault();
                _empresaRepository.Remover(empresa);
                return JsonConvert.SerializeObject(new { Mensagem = "Empresa excluída com sucesso!", Status = "200" });
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { Mensagem = "Não foi possível excluir a empresa devido as suas associações", Status = "200.1" });
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** CENTRO CUSTOS ****************************************/
        /***************************************************************************************************/
        #region Centro Custos

        public List<Centrocusto> ListarCentrosDeCusto(string pesquisa, int cliente)
        {
            pesquisa = pesquisa.ToLower();
            var ccs = _centroCustoRepository.Include(x => x.EmpresaNavigation)
                      .Where(x => x.EmpresaNavigation.Cliente == cliente && ((pesquisa != "null") ?
                                x.EmpresaNavigation.Nome.ToLower().Contains(pesquisa.ToLower()) ||
                                x.Nome.ToLower().Contains(pesquisa) ||
                                x.Codigo.ToLower().Contains(pesquisa) :
                                1 == 1))
                      .OrderBy(x => x.Nome).ToList();
            return ccs;
        }
        public List<CentrocustoVM> ListarCentrosDeCustoVM(string pesquisa, int cliente)
        {
            try
            {
                // Buscar empresas do cliente
                var empresasDoCliente = _empresaRepository.Buscar(x => x.Cliente == cliente).Select(x => x.Id).ToList();
                
                // Buscar centros de custo das empresas do cliente
                var centrosCusto = _centroCustoRepository
                    .Include(x => x.EmpresaNavigation)
                    .Where(x => empresasDoCliente.Contains(x.Empresa))
                    .ToList();

                // Filtrar por pesquisa se não for "null"
                if (pesquisa != "null" && !string.IsNullOrEmpty(pesquisa))
                {
                    pesquisa = pesquisa.ToLower();
                    centrosCusto = centrosCusto
                        .Where(x => x.Nome.ToLower().Contains(pesquisa) ||
                                   x.Codigo.ToLower().Contains(pesquisa) ||
                                   (x.EmpresaNavigation?.Nome?.ToLower().Contains(pesquisa) ?? false))
                        .ToList();
                }

                // Mapear para CentrocustoVM
                var resultado = centrosCusto.Select(x => new CentrocustoVM
                {
                    Id = x.Id,
                    Codigo = x.Codigo,
                    Nome = x.Nome,
                    EmpresaId = x.Empresa,
                    Empresa = x.EmpresaNavigation?.Nome ?? "N/A",
                    Ativo = x.Ativo, // ✅ ADICIONADO: Campo Ativo do banco
                    CreatedAt = x.CreatedAt, // ✅ ADICIONADO: Campo CreatedAt do banco
                    UpdatedAt = x.UpdatedAt // ✅ ADICIONADO: Campo UpdatedAt do banco
                }).OrderBy(x => x.Nome).ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                // Log do erro para debug
                throw; // Re-throw para que o controller trate o erro
            }
        }
        public CentrocustoVM BuscarCentroCustoPorId(int id)
        {
            var cc = _centroCustoRepository.Include(x => x.EmpresaNavigation).Where(x => x.Id == id).FirstOrDefault();
            if (cc == null) return null;
            
            return new CentrocustoVM
            {
                Id = cc.Id,
                Codigo = cc.Codigo,
                Nome = cc.Nome,
                EmpresaId = cc.Empresa,
                Empresa = cc.EmpresaNavigation?.Nome ?? "N/A",
                Ativo = cc.Ativo,
                CreatedAt = cc.CreatedAt,
                UpdatedAt = cc.UpdatedAt
            };
        }
        public List<CentrocustoVM> BuscarPorEmpresaId(int idEMpresa)
        {
            try
            {
                var emp = _empresaRepository.Buscar(x => x.Id == idEMpresa).FirstOrDefault();
                if (idEMpresa != 0 && emp != null)
                {
                    // Buscar centros de custo da empresa específica
                    var centrosCusto = _centroCustoRepository
                        .Include(x => x.EmpresaNavigation)
                        .Where(x => x.Empresa == idEMpresa)
                        .ToList();

                    // Mapear para CentrocustoVM
                    var resultado = centrosCusto.Select(x => new CentrocustoVM
                    {
                        Id = x.Id,
                        Codigo = x.Codigo,
                        Nome = x.Nome,
                        EmpresaId = x.Empresa,
                        Empresa = x.EmpresaNavigation?.Nome ?? "N/A",
                        Ativo = x.Ativo, // ✅ ADICIONADO: Campo Ativo do banco
                        CreatedAt = x.CreatedAt, // ✅ ADICIONADO: Campo CreatedAt do banco
                        UpdatedAt = x.UpdatedAt // ✅ ADICIONADO: Campo UpdatedAt do banco
                    }).ToList();

                    return resultado;
                }
                else
                {
                    return ListarCentrosDeCustoVM("null", emp?.Cliente ?? 0);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string SalvarCentroCusto(CentrocustoVM cc)
        {
            try
            {
                
                // Verificar se a empresa existe
                var empresaExiste = _empresaRepository.Buscar(x => x.Id == cc.EmpresaId).Any();
                
                if (!empresaExiste)
                {
                    return JsonConvert.SerializeObject(new { Mensagem = "Empresa não encontrada.", Status = "400" });
                }
                
                if (cc.Id == 0)
                {
                    
                    var existe = _centroCustoRepository.Buscar(x => x.Codigo == cc.Codigo && x.Empresa == cc.EmpresaId).Any();
                    
                    if (!existe)
                    {
                        var ccNovo = new Centrocusto
                        {
                            Codigo = cc.Codigo,
                            Empresa = cc.EmpresaId,
                            Nome = cc.Nome,
                            Ativo = true,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        
                        
                        // ✅ CORREÇÃO: Usar transação para garantir persistência
                        _centroCustoRepository.ExecuteInTransaction(() => {
                            _centroCustoRepository.Adicionar(ccNovo);
                        });
                        
                        return JsonConvert.SerializeObject(new { Mensagem = "Centro de custo salvo com sucesso!", Status = "200" });
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new { Mensagem = "Já existe um centro de custo com o código informado para a empresa selecionada.", Status = "200.1" });
                    }
                }
                else
                {
                    
                    var centroCustoDb = _centroCustoRepository.ObterPorId(cc.Id);
                    if (centroCustoDb is null)
                    {
                        return JsonConvert.SerializeObject(new { Mensagem = "Centro de custo não encontrado.", Status = "200.1" });
                    }

                    centroCustoDb.Codigo = cc.Codigo;
                    centroCustoDb.Empresa = cc.EmpresaId;
                    centroCustoDb.Nome = cc.Nome;
                    centroCustoDb.UpdatedAt = DateTime.Now;

                    
                    // ✅ CORREÇÃO: Usar transação para garantir persistência
                    _centroCustoRepository.ExecuteInTransaction(() => {
                        _centroCustoRepository.Atualizar(centroCustoDb);
                    });
                    
                    return JsonConvert.SerializeObject(new { Mensagem = "Centro de custo salvo com sucesso!", Status = "200" });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string ExcluirCentroCusto(int id)
        {
            try
            {
                var cc = _centroCustoRepository.Buscar(x => x.Id == id).FirstOrDefault();
                _centroCustoRepository.Remover(cc);
                return JsonConvert.SerializeObject(new { Mensagem = "Centro de custo excluído com sucesso!", Status = "200" });
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { Mensagem = "Não foi possível excluir o centro de custo devido as suas associações", Status = "200.1" });
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** FORNECEDORES *****************************************/
        /***************************************************************************************************/
        #region Fornecedores
        public List<Fornecedore> ListarFornecedores(string pesquisa, int cliente)
        {
            try
            {
                pesquisa = pesquisa?.ToLower() ?? "";
                var fnc = _fornecedorRepository
                            .Buscar(x => x.Cliente == cliente && x.Ativo == true && ((pesquisa != "null" && !string.IsNullOrEmpty(pesquisa)) ?
                                    x.Nome.ToLower().Contains(pesquisa) ||
                                    x.Cnpj.Contains(pesquisa) :
                                    1 == 1))
                            .OrderBy(x => x.Nome).ToList();
                
                return fnc;
            }
            catch (Exception ex)
            {
                return new List<Fornecedore>();
            }
        }

        public List<Fornecedore> ListarFornecedoresDestinadores(int cliente)
        {
            try
            {
                var destinadores = _fornecedorRepository
                    .Buscar(x => x.Cliente == cliente && 
                                 x.Ativo == true && 
                                 x.DestinadorResiduos == true)
                    .OrderBy(x => x.Nome)
                    .ToList();
                
                return destinadores;
            }
            catch (Exception ex)
            {
                return new List<Fornecedore>();
            }
        }

        public string SalvarFornecedor(Fornecedore fornecedor)
        {
            try
            {
                // Validação de CNPJ
                if (!string.IsNullOrWhiteSpace(fornecedor.Cnpj))
                {
                    if (!CnpjValidator.IsValid(fornecedor.Cnpj))
                    {
                        return JsonConvert.SerializeObject(new { 
                            Mensagem = "❌ CNPJ inválido! O CNPJ informado não é válido. Verifique se possui 14 dígitos e se os dígitos verificadores estão corretos.", 
                            Status = "400",
                            Tipo = "CNPJ_INVALIDO",
                            CNPJ_Informado = fornecedor.Cnpj,
                            Sugestao = "Digite um CNPJ válido com 14 dígitos no formato XX.XXX.XXX/XXXX-XX"
                        });
                    }
                    
                    // Formata o CNPJ antes de salvar
                    fornecedor.Cnpj = CnpjValidator.Format(fornecedor.Cnpj);
                }
                
                var resultado = _fornecedorRepository.ExecuteInTransaction(() =>
                {
                    if (fornecedor.Id == 0)
                    {
                        // Verificar se já existe fornecedor com mesmo CNPJ no mesmo cliente
                        var existe = _fornecedorRepository.Buscar(x => x.Cnpj == fornecedor.Cnpj && x.Cliente == fornecedor.Cliente).Any();
                        
                        if (!existe)
                        {
                            fornecedor.Ativo = true;
                            _fornecedorRepository.Adicionar(fornecedor);
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "✅ Fornecedor salvo com sucesso! O fornecedor foi cadastrado no sistema e está disponível para uso.", 
                                Status = "200",
                                Tipo = "SUCESSO",
                                Fornecedor = fornecedor.Nome,
                                CNPJ = fornecedor.Cnpj
                            });
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "⚠️ CNPJ já cadastrado! Este CNPJ já está sendo usado por outro fornecedor no mesmo cliente. Verifique se não é o mesmo fornecedor ou use um CNPJ diferente.", 
                                Status = "400.1",
                                Tipo = "CNPJ_DUPLICADO",
                                Sugestao = "Verifique se não está tentando cadastrar o mesmo fornecedor ou use um CNPJ diferente."
                            });
                        }
                    }
                    else
                    {
                        // Verificar se já existe fornecedor com mesmo CNPJ no mesmo cliente (excluindo o atual)
                        var existe = _fornecedorRepository.Buscar(x => x.Cnpj == fornecedor.Cnpj && x.Cliente == fornecedor.Cliente && x.Id != fornecedor.Id).Any();
                        
                        if (!existe)
                        {
                            _fornecedorRepository.Atualizar(fornecedor);
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "✅ Fornecedor atualizado com sucesso! As informações foram modificadas e salvas no sistema.", 
                                Status = "200",
                                Tipo = "SUCESSO",
                                Fornecedor = fornecedor.Nome,
                                CNPJ = fornecedor.Cnpj,
                                ID = fornecedor.Id
                            });
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "⚠️ CNPJ já cadastrado! Este CNPJ já está sendo usado por outro fornecedor no mesmo cliente. Verifique se não é o mesmo fornecedor ou use um CNPJ diferente.", 
                                Status = "400.1",
                                Tipo = "CNPJ_DUPLICADO",
                                Sugestao = "Verifique se não está tentando cadastrar o mesmo fornecedor ou use um CNPJ diferente."
                            });
                        }
                    }
                });
                
                return resultado;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Mensagem = $"Erro ao salvar fornecedor: {ex.Message}", Status = "500" });
            }
        }
        public void ExcluirFornecedor(int id)
        {
            var fnc = _fornecedorRepository.Buscar(x => x.Id == id).FirstOrDefault();
            try
            {
                _fornecedorRepository.Remover(fnc);
            }
            catch
            {
                fnc.Ativo = false;
                _fornecedorRepository.Atualizar(fnc);
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** TIPOS DE RECURSOS ************************************/
        /***************************************************************************************************/
        #region Tipo Recurso
        public List<Tipoequipamento> ListarTiposDeRecursos(string pesquisa, int cliente)
        {
            pesquisa = pesquisa.ToLower();
            var tipos = (from te in _tipoequipamentoRepository.Query()
                         join tec in _tipoequipamentoClienteRepository.Query() on te.Id equals tec.Tipo
                         where te.Ativo && tec.Cliente == cliente && 
                               te.Id != 1 && // ✅ EXCLUIR especificamente ID = 1 (Linha Telefônica)
                               ((pesquisa != "null") ? te.Descricao.ToLower().Contains(pesquisa) : 1 == 1)
                         orderby te.Descricao
                         select te
                         ).Distinct().ToList(); // ✅ DISTINCT para remover duplicatas
            return tipos;
        }

        public List<Tipoaquisicao> ListarTiposAquisicao()
        {
            return _tipoaquisicaoRepository.ObterTodos().ToList();
        }

        public string SalvarTipoRecurso(Tipoequipamento te)
        {
            if (te.Id == 0)
            {
                bool existe = _tipoequipamentoRepository.Buscar(x => x.Descricao == te.Descricao).Any();
                if (!existe)
                {
                    te.Ativo = true;
                    _tipoequipamentoRepository.Adicionar(te);
                    _tipoequipamentoRepository.SalvarAlteracoes();
                    return JsonConvert.SerializeObject(new { Mensagem = "Tipo de recurso salvo com sucesso!", Status = "200" });
                }
                else
                {
                    var tipo = _tipoequipamentoRepository.Buscar(x => x.Descricao == te.Descricao).FirstOrDefault();
                    var tipoCliente = _tipoequipamentoClienteRepository.Buscar(x => x.Cliente == te.Tipoequipamentosclientes.FirstOrDefault().Cliente && x.Tipo == tipo.Id).FirstOrDefault();
                    if (tipoCliente == null)
                    {
                        var teCliente = te.Tipoequipamentosclientes.FirstOrDefault();
                        teCliente.Tipo = tipo.Id;
                        _tipoequipamentoClienteRepository.Adicionar(teCliente);
                        _tipoequipamentoClienteRepository.SalvarAlteracoes();
                        return JsonConvert.SerializeObject(new { Mensagem = "Tipo de recurso salvo com sucesso!", Status = "200" });
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new { Mensagem = "Ja existe tipo de recurso com esta descrição!", Status = "200.1" });
                    }
                }
            }
            else
            {
                _tipoequipamentoRepository.Atualizar(te);
                _tipoequipamentoRepository.SalvarAlteracoes();
                return JsonConvert.SerializeObject(new { Mensagem = "Tipo de recurso salvo com sucesso!", Status = "200" });
            }
        }
        public void ExcluirTipoRecurso(int idTipo, int idCliente)
        {
            try
            {
                var tecli = (from te in _tipoequipamentoRepository.Query()
                             join tec in _tipoequipamentoClienteRepository.Query() on te.Id equals tec.Tipo
                             where tec.Cliente == idCliente && te.Id == idTipo
                             select tec).FirstOrDefault();

                if (tecli != null)
                {
                    _tipoequipamentoClienteRepository.Remover(tecli);
                    _tipoequipamentoClienteRepository.SalvarAlteracoes();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** FABRICANTES ******************************************/
        /***************************************************************************************************/
        #region Fabricante
        public List<Fabricante> ListarFabricantes(string pesquisa, int cliente)
        {
            pesquisa = pesquisa.ToLower();
            var fabs = _fabricanteRepository
                        .Include(x => x.TipoequipamentoNavigation)
                        .Include(x => x.TipoequipamentoNavigation)
                        .Where(x => x.Cliente == cliente && x.Ativo == true && ((pesquisa != "null") ?
                                x.Descricao.ToLower().Contains(pesquisa) ||
                                x.TipoequipamentoNavigation.Descricao.ToLower().Contains(pesquisa) :
                                1 == 1))
                        .OrderBy(x => x.Descricao).ToList();
            return fabs;
        }
        public List<Fabricante> ListarFabricantesPorTipoRecurso(int tipo, int cliente)
        {
            var fabs = _fabricanteRepository.Buscar(x => x.Tipoequipamento == tipo && x.Cliente == cliente).OrderBy(x => x.Descricao).ToList();
            return fabs;
        }
        public string SalvarFabricante(Fabricante fab)
        {
            try
            {
                Console.WriteLine($"=== NEGÓCIO: SALVANDO FABRICANTE ===");
                Console.WriteLine($"ID: {fab?.Id}");
                Console.WriteLine($"Cliente: {fab?.Cliente}");
                Console.WriteLine($"Tipo Equipamento: {fab?.Tipoequipamento}");
                Console.WriteLine($"Descrição: {fab?.Descricao}");
                Console.WriteLine($"Ativo: {fab?.Ativo}");

                if (fab.Id == 0)
                {
                    Console.WriteLine("Criando novo fabricante...");
                    bool existe = _fabricanteRepository.Buscar(x => x.Cliente == fab.Cliente && x.Descricao.ToLower() == fab.Descricao.ToLower() && x.Tipoequipamento == fab.Tipoequipamento).Any();
                    Console.WriteLine($"Fabricante já existe? {existe}");
                    
                    if (!existe)
                    {
                        fab.Ativo = true;
                        Console.WriteLine("Adicionando fabricante...");
                        _fabricanteRepository.Adicionar(fab);
                        Console.WriteLine("Salvando alterações...");
                        _fabricanteRepository.SalvarAlteracoes();
                        Console.WriteLine("Fabricante salvo com sucesso!");
                        return JsonConvert.SerializeObject(new { Mensagem = "Fabricante salvo com sucesso!", Status = "200" });
                    }
                    else
                    {
                        Console.WriteLine("Fabricante já existe, lançando exceção...");
                        throw new EntidadeJaExisteEx("O fabricante informado já existe no sistema. Favor utilizá-lo!");
                    }
                }
                else
                {
                    Console.WriteLine("Atualizando fabricante existente...");
                    _fabricanteRepository.Atualizar(fab);
                    Console.WriteLine("Salvando alterações...");
                    _fabricanteRepository.SalvarAlteracoes();
                    Console.WriteLine("Fabricante atualizado com sucesso!");
                    return JsonConvert.SerializeObject(new { Mensagem = "Fabricante salvo com sucesso!", Status = "200" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no negócio: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        public void ExcluirFabricante(int id)
        {
            var fab = _fabricanteRepository.Buscar(x => x.Id == id).FirstOrDefault();
            try
            {
                if (fab != null)
                {
                    _fabricanteRepository.Remover(fab);
                    _fabricanteRepository.SalvarAlteracoes();
                }
            }
            catch
            {
                if (fab != null)
                {
                    fab.Ativo = false;
                    _fabricanteRepository.Atualizar(fab);
                    _fabricanteRepository.SalvarAlteracoes();
                }
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** MODELOS **********************************************/
        /***************************************************************************************************/
        #region Modelo
        public List<Modelo> ListarModelos(string pesquisa, int cliente)
        {
            pesquisa = pesquisa.ToLower();
            // Listar todos os modelos (ativos e inativos) para permitir reativação
            // Não filtrar por ativo para que modelos desativados possam ser visualizados e reativados
            var modelos = _modeloRepository.IncludeWithThenInclude(q => q
                                            .Include(x => x.FabricanteNavigation)
                                                .ThenInclude(x => x.TipoequipamentoNavigation))
                         .Where(x => x.Cliente == cliente && ((pesquisa != "null") ?
                                x.Descricao.ToLower().Contains(pesquisa) ||
                                x.FabricanteNavigation.Descricao.ToLower().Contains(pesquisa) ||
                                x.FabricanteNavigation.TipoequipamentoNavigation.Descricao.ToLower().Contains(pesquisa) :
                                1 == 1))
                         .OrderBy(x => x.Descricao).ToList();
            return modelos;
        }

        public List<Modelo> ListarModelosDoFabricante(int fabricante, int cliente)
        {
            var modelos = _modeloRepository.IncludeWithThenInclude(q => q
                                                .Include(x => x.FabricanteNavigation)
                                                    .ThenInclude(x => x.TipoequipamentoNavigation))
                         .Where(x => x.Cliente == cliente && x.Ativo == true && x.Fabricante == fabricante)
                         .OrderBy(x => x.Descricao).ToList();
            return modelos;
        }
        public string SalvarModelo(Modelo md)
        {
            try
            {
                Console.WriteLine($"=== NEGÓCIO: SALVANDO MODELO ===");
                Console.WriteLine($"ID: {md?.Id}");
                Console.WriteLine($"Cliente: {md?.Cliente}");
                Console.WriteLine($"Fabricante: {md?.Fabricante}");
                Console.WriteLine($"Descrição: {md?.Descricao}");
                Console.WriteLine($"Ativo: {md?.Ativo}");

                if (md.Id == 0)
                {
                    Console.WriteLine("Criando novo modelo...");
                    bool existe = _modeloRepository.Buscar(x => x.Cliente == md.Cliente && x.Descricao.ToLower() == md.Descricao.ToLower()).Any();
                    Console.WriteLine($"Modelo já existe? {existe}");
                    
                    if (!existe)
                    {
                        md.Ativo = true;
                        Console.WriteLine("Adicionando modelo...");
                        _modeloRepository.Adicionar(md);
                        Console.WriteLine("Salvando alterações...");
                        _modeloRepository.SalvarAlteracoes();
                        Console.WriteLine("Modelo salvo com sucesso!");
                        return "Modelo salvo com sucesso!";
                    }
                    else
                    {
                        Console.WriteLine("Modelo já existe, lançando exceção...");
                        throw new EntidadeJaExisteEx("O modelo informado já existe no sistema. Favor utilizá-lo!");
                    }
                }
                else
                {
                    Console.WriteLine("Atualizando modelo existente...");
                    _modeloRepository.Atualizar(md);
                    Console.WriteLine("Salvando alterações...");
                    _modeloRepository.SalvarAlteracoes();
                    Console.WriteLine("Modelo atualizado com sucesso!");
                    return "Modelo salvo com sucesso!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no negócio: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        public void ExcluirModelo(int id)
        {
            var md = _modeloRepository.Buscar(x => x.Id == id).FirstOrDefault();
            try
            {
                if (md != null)
                {
                    _modeloRepository.Remover(md);
                    _modeloRepository.SalvarAlteracoes();
                }
            }
            catch
            {
                if (md != null)
                {
                    md.Ativo = false;
                    _modeloRepository.Atualizar(md);
                    _modeloRepository.SalvarAlteracoes();
                }
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** NOTAS FISCAIS ****************************************/
        /***************************************************************************************************/
        #region NotasFiscais
        public List<NotaFiscalListagemVM> ListarNotasFiscais(string pesquisa, int cliente)
        {
            try
            {
                Console.WriteLine($"[NOTAS-FISCAIS] Listando notas fiscais - Pesquisa: '{pesquisa}', Cliente: {cliente}");
                
                pesquisa = pesquisa?.ToLower() ?? "";
                var nfs = _notafiscalRepository
                            .Include(x => x.FornecedorNavigation)
                            .Include(x => x.Notasfiscaisitens)
                            .Where(x => x.Cliente == cliente &&
                            ((pesquisa != "null" && !string.IsNullOrEmpty(pesquisa)) ?
                                x.FornecedorNavigation.Nome.ToLower().Contains(pesquisa) ||
                                x.Numero.ToString().Contains(pesquisa)
                            : 1 == 1))
                    .ToList();
                
                Console.WriteLine($"[NOTAS-FISCAIS] Encontradas {nfs.Count} notas fiscais");
                
                // Converter para DTO com propriedades de controle
                var resultado = nfs.Select(nf => {
                    var podeAdicionar = !nf.Gerouequipamento;
                    var podeExcluir = !nf.Gerouequipamento;
                    
                    Console.WriteLine($"[NOTAS-FISCAIS] Nota {nf.Id} - Gerouequipamento: {nf.Gerouequipamento}, PodeAdicionar: {podeAdicionar}, PodeExcluir: {podeExcluir}");
                    Console.WriteLine($"[NOTAS-FISCAIS] Nota {nf.Id} - Fornecedor ID: {nf.Fornecedor}, FornecedorNavigation: {nf.FornecedorNavigation?.Nome ?? "NULL"}");
                    Console.WriteLine($"[NOTAS-FISCAIS] Nota {nf.Id} - ArquivoNotaFiscal: {nf.ArquivoNotaFiscal ?? "NULL"}, NomeArquivoOriginal: {nf.NomeArquivoOriginal ?? "NULL"}");
                    
                    return new NotaFiscalListagemVM
                    {
                        Id = nf.Id,
                        Cliente = nf.Cliente,
                        Fornecedor = nf.Fornecedor,
                        Numero = nf.Numero,
                        Dtemissao = nf.Dtemissao,
                        Descricao = nf.Descricao,
                        Valor = nf.Valor,
                        Virtual = nf.Virtual,
                        Gerouequipamento = nf.Gerouequipamento,
                        Migrateid = nf.Migrateid,
                        
                        // Campos de arquivo
                        ArquivoNotaFiscal = nf.ArquivoNotaFiscal,
                        NomeArquivoOriginal = nf.NomeArquivoOriginal,
                        DataUploadArquivo = nf.DataUploadArquivo,
                        
                        // Propriedades de controle de visibilidade
                        PodeVisualizar = true, // Sempre visível
                        PodeAdicionarRecursos = podeAdicionar, // Só quando não processada
                        PodeExcluir = podeExcluir, // Só quando não processada
                        
                        // Propriedades de navegação
                        FornecedorNome = nf.FornecedorNavigation?.Nome ?? "",
                        FornecedorNavigation = nf.FornecedorNavigation != null ? new FornecedorNavigationVM
                        {
                            Id = nf.FornecedorNavigation.Id,
                            Nome = nf.FornecedorNavigation.Nome,
                            Cnpj = nf.FornecedorNavigation.Cnpj,
                            Cliente = nf.FornecedorNavigation.Cliente,
                            Ativo = nf.FornecedorNavigation.Ativo
                        } : null,
                        FornecedorCnpj = nf.FornecedorNavigation?.Cnpj ?? "",
                        QuantidadeItens = nf.Notasfiscaisitens?.Count ?? 0
                    };
                }).ToList();
                
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTAS-FISCAIS] ❌ Erro ao listar notas fiscais: {ex.Message}");
                Console.WriteLine($"[NOTAS-FISCAIS] StackTrace: {ex.StackTrace}");
                throw;
            }
        }
        public Notasfiscai BuscarNotaPorId(int id)
        {
            var nf = _notafiscalRepository
                    .Include(x => x.Notasfiscaisitens)
                    .Include(x => x.FornecedorNavigation)
                    .Include(x => x.Equipamentos)
                        .ThenInclude(x => x.FabricanteNavigation)
                    .Include(x => x.Equipamentos)
                        .ThenInclude(x => x.ModeloNavigation)
                    .Include(x => x.Equipamentos)
                        .ThenInclude(x => x.ContratoNavigation)
                    .Include(x => x.Equipamentos)
                        .ThenInclude(x => x.TipoequipamentoNavigation)
                    .Where(x => x.Id == id)
                    .FirstOrDefault();

            return nf;
        }

        public VisualizarNotaFiscalVM VisualizarNotaFiscal(int id)
        {
            Console.WriteLine($"[VISUALIZAR-NF] Iniciando visualização da nota fiscal ID: {id}");
            
            Notasfiscai nf = BuscarNotaPorId(id);
            if (nf == null)
            {
                Console.WriteLine($"[VISUALIZAR-NF] Nota fiscal ID {id} não encontrada");
                return null;
            }
            
            Console.WriteLine($"[VISUALIZAR-NF] Nota fiscal encontrada: {nf.Numero}");
            Console.WriteLine($"[VISUALIZAR-NF] Quantidade de itens: {nf.Notasfiscaisitens?.Count ?? 0}");
            
            return new VisualizarNotaFiscalVM
            {
                Dtemissao = nf.Dtemissao,
                FornecedorId = nf.Fornecedor,
                Fornecedor = nf.FornecedorNavigation?.Nome ?? "N/A",
                Numero = nf.Numero,
                Gerouequipamento = nf.Gerouequipamento ? "SIM" : "NÃO",
                Descricao = nf.Descricao,
                Valor = nf.Valor?.ToFormattedCurrency("pt-BR"),
                QuantidadeItens = nf.Notasfiscaisitens?.Count ?? 0,
                Itens = MapearItensNotafiscal(nf.Notasfiscaisitens)
            };
        }

        private List<VisualizarNotaFiscalItem> MapearItensNotafiscal(ICollection<Notasfiscaisiten> notasfiscaisitens)
        {
            Console.WriteLine($"[MAPEAR-ITENS] Iniciando mapeamento de itens");
            List<VisualizarNotaFiscalItem> itens = new List<VisualizarNotaFiscalItem>();
            
            if (notasfiscaisitens == null)
            {
                Console.WriteLine($"[MAPEAR-ITENS] Lista de itens é null");
                return itens;
            }
            
            Console.WriteLine($"[MAPEAR-ITENS] Processando {notasfiscaisitens.Count} itens");
                
            foreach (var item in notasfiscaisitens)
            {
                if (item == null)
                {
                    Console.WriteLine($"[MAPEAR-ITENS] Item null encontrado, pulando");
                    continue;
                }
                
                Console.WriteLine($"[MAPEAR-ITENS] Processando item ID: {item.Id}");
                    
                itens.Add(new VisualizarNotaFiscalItem
                {
                    Tipoequipamento = item.TipoequipamentoNavigation?.Descricao ?? "N/A",
                    TipoequipamentoId = item.Tipoequipamento,
                    Fabricante = item.FabricanteNavigation?.Descricao ?? "N/A",
                    FabricanteId = item.Fabricante,
                    Modelo = item.ModeloNavigation?.Descricao ?? "N/A",
                    ModeloId = item.Modelo,
                    Contrato = item.ContratoNavigation?.Descricao ?? "N/A",
                    Dtlimitegarantia = item.Dtlimitegarantia,
                    Id = item.Id,
                    Notafiscal = item.Notafiscal,
                    Quantidade = item.Quantidade,
                    TipoAquisicao = item.TipoAquisicao > 0 ? ((TipoAquisicaoEnum)item.TipoAquisicao).ToString() : "N/A",
                    Valorunitario = item.Valorunitario.ToFormattedCurrency("pt-BR")
                });
            }

            Console.WriteLine($"[MAPEAR-ITENS] Mapeamento concluído. {itens.Count} itens processados");
            return itens;
        }

        /// <summary>
        /// Lembrar de adicionar o array de itens na web para salvar automaticamente.
        /// </summary>
        /// <param name="nf"></param>
        public void SalvarNotaFiscal(Notasfiscai nf)
        {
            try
            {
                // Converter DateTimes UTC para Local antes de salvar no banco
                if (nf.Dtemissao.Kind == DateTimeKind.Utc)
                {
                    nf.Dtemissao = DateTime.SpecifyKind(nf.Dtemissao, DateTimeKind.Local);
                }
                
                if (nf.DataUploadArquivo.HasValue && nf.DataUploadArquivo.Value.Kind == DateTimeKind.Utc)
                {
                    nf.DataUploadArquivo = DateTime.SpecifyKind(nf.DataUploadArquivo.Value, DateTimeKind.Local);
                }
                
                if (nf.DataRemocaoArquivo.HasValue && nf.DataRemocaoArquivo.Value.Kind == DateTimeKind.Utc)
                {
                    nf.DataRemocaoArquivo = DateTime.SpecifyKind(nf.DataRemocaoArquivo.Value, DateTimeKind.Local);
                }
                
                // Converter DateTimes dos itens da nota fiscal também
                if (nf.Notasfiscaisitens != null && nf.Notasfiscaisitens.Count > 0)
                {
                    foreach (var item in nf.Notasfiscaisitens)
                    {
                        if (item.Dtlimitegarantia.HasValue && item.Dtlimitegarantia.Value.Kind == DateTimeKind.Utc)
                        {
                            item.Dtlimitegarantia = DateTime.SpecifyKind(item.Dtlimitegarantia.Value, DateTimeKind.Local);
                        }
                    }
                }
                
                if (nf.Id == 0)
                {
                    //db.Add(nf);
                    nf.Valor = nf.CalcularValorNota();
                    _notafiscalRepository.Adicionar(nf);
                }
                else
                {
                    //db.Update(nf);
                    _notafiscalRepository.Atualizar(nf);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void AdicionarItemNota(Notasfiscaisiten nfi)
        {
            try
            {
                // Converter DateTimes UTC para Local antes de salvar
                if (nfi.Dtlimitegarantia.HasValue && nfi.Dtlimitegarantia.Value.Kind == DateTimeKind.Utc)
                {
                    nfi.Dtlimitegarantia = DateTime.SpecifyKind(nfi.Dtlimitegarantia.Value, DateTimeKind.Local);
                }
                
                //db.Add(nfi);
                //db.SaveChanges();
                _itensNotafiscalRepository.Adicionar(nfi);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExcluirNotaFiscal(int id)
        {
            try
            {
                Console.WriteLine($"[NOTAS-FISCAIS] Tentando excluir nota fiscal ID: {id}");
                
                // ✅ CORREÇÃO: Usar ExecuteInTransaction para evitar conflito com estratégia de execução
                _notafiscalRepository.ExecuteInTransaction(() =>
                {
                    // ✅ CORREÇÃO: Buscar nota fiscal sem Include problemático
                    var nf = _notafiscalRepository
                        .Buscar(x => x.Id == id)
                        .FirstOrDefault();
                        
                    if (nf == null)
                    {
                        Console.WriteLine($"[NOTAS-FISCAIS] ❌ Nota fiscal ID {id} não encontrada");
                        throw new Exception($"Nota fiscal ID {id} não encontrada");
                    }
                    
                    Console.WriteLine($"[NOTAS-FISCAIS] Nota fiscal encontrada: {nf.Numero}");
                    Console.WriteLine($"[NOTAS-FISCAIS] Gerou equipamento: {nf.Gerouequipamento}");
                    
                    // ✅ VALIDAÇÃO: Verificar se há equipamentos registrados (sem Include problemático)
                    var equipamentosCount = _equipamentoRepository.Buscar(x => x.Notafiscal == nf.Id).Count();
                    Console.WriteLine($"[NOTAS-FISCAIS] Equipamentos associados: {equipamentosCount}");
                    
                    if (equipamentosCount > 0)
                    {
                        Console.WriteLine($"[NOTAS-FISCAIS] ❌ Não é possível excluir nota fiscal {nf.Numero} - possui {equipamentosCount} equipamento(s) registrado(s)");
                        throw new Exception($"Não é possível excluir a nota fiscal {nf.Numero} pois ela possui {equipamentosCount} equipamento(s) registrado(s). Remova os equipamentos primeiro.");
                    }
                    
                    // ✅ VALIDAÇÃO: Verificar se gerou equipamentos (campo gerouequipamento)
                    if (nf.Gerouequipamento)
                    {
                        Console.WriteLine($"[NOTAS-FISCAIS] ❌ Não é possível excluir nota fiscal {nf.Numero} - já gerou equipamentos");
                        throw new Exception($"Não é possível excluir a nota fiscal {nf.Numero} pois ela já gerou equipamentos no sistema.");
                    }
                    
                    Console.WriteLine($"[NOTAS-FISCAIS] ✅ Nota fiscal {nf.Numero} pode ser excluída");
                    
                    // Remover itens da nota fiscal
                    var itens = _itensNotafiscalRepository.Buscar(x => x.Notafiscal == nf.Id).ToList();
                    Console.WriteLine($"[NOTAS-FISCAIS] Removendo {itens.Count} itens da nota fiscal");
                    foreach (var nfi in itens)
                    {
                        _itensNotafiscalRepository.Remover(nfi);
                    }
                    
                    // Remover a nota fiscal
                    _notafiscalRepository.Remover(nf);
                    Console.WriteLine($"[NOTAS-FISCAIS] ✅ Nota fiscal {nf.Numero} excluída com sucesso");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTAS-FISCAIS] ❌ Erro ao excluir nota fiscal ID {id}: {ex.Message}");
                throw;
            }
        }
        public void ExcluirItemNota(int id)
        {
            try
            {
                var nfi = _itensNotafiscalRepository.Buscar(x => x.Id == id).FirstOrDefault();
                //db.Remove(nf);
                //db.SaveChanges();
                _itensNotafiscalRepository.Remover(nfi);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void LiberarParaEstoque(NotaFiscalVM nf)
        {
            Console.WriteLine($"[LIBERAR-ESTOQUE] Iniciando processo para usuário {nf.Usuario}");
            
            // Carregar a nota fiscal completa com itens do banco
            Console.WriteLine($"[LIBERAR-ESTOQUE] Buscando nota fiscal ID: {nf.Nota.Id}");
            var notaCompleta = _notafiscalRepository.Buscar(x => x.Id == nf.Nota.Id)
                .Include(x => x.Notasfiscaisitens)
                .ThenInclude(x => x.TipoequipamentoNavigation)
                .Include(x => x.Notasfiscaisitens)
                .ThenInclude(x => x.FabricanteNavigation)
                .Include(x => x.Notasfiscaisitens)
                .ThenInclude(x => x.ModeloNavigation)
                .Include(x => x.Notasfiscaisitens)
                .ThenInclude(x => x.ContratoNavigation)
                .FirstOrDefault();
            
            Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 DEBUG - Query executada. Nota encontrada: {notaCompleta != null}");
            if (notaCompleta != null)
            {
                Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 DEBUG - Nota fiscal: ID={notaCompleta.Id}, Numero={notaCompleta.Numero}");
                Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 DEBUG - Itens carregados: {notaCompleta.Notasfiscaisitens?.Count ?? 0}");
            }
            
            if (notaCompleta == null)
            {
                Console.WriteLine($"[LIBERAR-ESTOQUE] ❌ Nota fiscal ID {nf.Nota.Id} não encontrada");
                throw new ArgumentException("Nota fiscal não encontrada");
            }
            
            Console.WriteLine($"[LIBERAR-ESTOQUE] ✅ Nota fiscal encontrada: {notaCompleta.Numero}");
            
            // Validações iniciais
            if (notaCompleta.Notasfiscaisitens == null || !notaCompleta.Notasfiscaisitens.Any())
            {
                Console.WriteLine($"[LIBERAR-ESTOQUE] ⚠️ Nota fiscal {notaCompleta.Id} não possui itens. Processando mesmo assim...");
                // Não lançar exceção, apenas continuar com lista vazia
                return;
            }
            
            Console.WriteLine($"[LIBERAR-ESTOQUE] ✅ Nota fiscal {notaCompleta.Id} possui {notaCompleta.Notasfiscaisitens.Count} itens. Processando...");
            Console.WriteLine($"[LIBERAR-ESTOQUE] Gerouequipamento: {notaCompleta.Gerouequipamento}");
            
            // ✅ DEBUG: Listar todos os itens da nota fiscal
            Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 DEBUG - Itens da nota fiscal {notaCompleta.Id}:");
            Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 Total de itens encontrados: {notaCompleta.Notasfiscaisitens.Count}");
            
            if (notaCompleta.Notasfiscaisitens.Count == 0)
            {
                Console.WriteLine($"[LIBERAR-ESTOQUE] ❌ PROBLEMA: Nota fiscal {notaCompleta.Id} não possui itens para processar!");
                return;
            }
            
            for (int i = 0; i < notaCompleta.Notasfiscaisitens.Count; i++)
            {
                var item = notaCompleta.Notasfiscaisitens.ElementAt(i);
                Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 Item {i+1}: ID={item.Id}, Tipo={item.Tipoequipamento}, Fabricante={item.Fabricante}, Modelo={item.Modelo}, Qtd={item.Quantidade}, Contrato={item.Contrato}");
            }

            if (notaCompleta.Gerouequipamento)
            {
                Console.WriteLine($"[LIBERAR-ESTOQUE] ❌ Nota fiscal já foi processada anteriormente");
                throw new InvalidOperationException("Esta nota fiscal já foi processada e gerou equipamentos");
            }

            var usuario = _usuarioRepository.Buscar(x => x.Id == nf.Usuario).FirstOrDefault();
            if (usuario?.Cliente == null || usuario.Cliente == 0)
            {
                throw new ArgumentException("Usuário não encontrado ou sem cliente associado");
            }

            var clienteId = usuario.Cliente;
            Console.WriteLine($"[LIBERAR-ESTOQUE] Cliente ID: {clienteId}");

            List<dynamic> equipamentohistoricoList = new List<dynamic>();
            int totalEquipamentosCriados = 0;

            _notafiscalRepository.ExecuteInTransaction(() =>
            {
                try
                {
                    Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 Iniciando processamento de {notaCompleta.Notasfiscaisitens.Count} itens...");
                    
                    foreach (var nfi in notaCompleta.Notasfiscaisitens)
                    {
                        Console.WriteLine($"[LIBERAR-ESTOQUE] Processando item: Tipo={nfi.Tipoequipamento}, Fabricante={nfi.Fabricante}, Modelo={nfi.Modelo}, Qtd={nfi.Quantidade}, Contrato={nfi.Contrato}");
                        
                        try
                        {
                            // ✅ CORREÇÃO: Validar item usando método dedicado
                            Console.WriteLine($"[LIBERAR-ESTOQUE] Validando item: Tipo={nfi.Tipoequipamento}, Fabricante={nfi.Fabricante}, Modelo={nfi.Modelo}, Qtd={nfi.Quantidade}, Contrato={nfi.Contrato}");
                            ValidarItemNotaFiscal(nfi);
                            Console.WriteLine($"[LIBERAR-ESTOQUE] ✅ Item validado com sucesso");
                            
                            if (nfi.Quantidade <= 0)
                            {
                                Console.WriteLine($"[LIBERAR-ESTOQUE] ⚠️ Quantidade inválida: {nfi.Quantidade}");
                                continue;
                            }
                            
                            // ✅ CORREÇÃO: Contrato é opcional - nem todo lançamento exige contrato
                            if (nfi.Contrato == null || nfi.Contrato == 0)
                            {
                                Console.WriteLine($"[LIBERAR-ESTOQUE] ℹ️ Item sem contrato: Contrato={nfi.Contrato}. Processando normalmente (contrato é opcional).");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[LIBERAR-ESTOQUE] ❌ Erro ao validar item: {ex.Message}");
                            Console.WriteLine($"[LIBERAR-ESTOQUE] ❌ Item inválido será pulado: Tipo={nfi.Tipoequipamento}, Fabricante={nfi.Fabricante}, Modelo={nfi.Modelo}");
                            continue;
                        }

                        for (int i = 0; i < nfi.Quantidade; i++)
                        {
                            string serialNR = SerialNumberGenerator.GenerateSerialNumber("AUT", 10);
                            Console.WriteLine($"[LIBERAR-ESTOQUE] Criando equipamento {i+1}/{nfi.Quantidade} - Contrato do item: {nfi.Contrato}");
                            
                            var equipamento = new Equipamento
                            {
                                Tipoequipamento = nfi.Tipoequipamento,
                                Fabricante = nfi.Fabricante,
                                Modelo = nfi.Modelo,
                                Notafiscal = notaCompleta.Id, // ✅ CORREÇÃO: Usar ID da nota fiscal
                                Equipamentostatus = 6, // Novo
                                Usuario = nf.Usuario,
                                Cliente = clienteId,
                                Possuibo = false,
                                Numeroserie = serialNR,
                                Patrimonio = serialNR,
                                Dtcadastro = TimeZoneMapper.GetDateTimeNow(),
                                Ativo = true,
                                Tipoaquisicao = nfi.TipoAquisicao,
                                Dtlimitegarantia = nfi.Dtlimitegarantia,
                                Contrato = nfi.Contrato,
                                Descricaobo = $"Gerado automaticamente da nota fiscal {nf.Nota.Numero}",
                                Enviouemailreporte = false
                            };
                            
                            Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 DEBUG PERSISTÊNCIA: Equipamento criado - Contrato={equipamento.Contrato}, Serial={equipamento.Numeroserie}");
                            
                            Console.WriteLine($"[LIBERAR-ESTOQUE] Equipamento criado - Contrato={equipamento.Contrato}, Serial={serialNR}");

                            Console.WriteLine($"[LIBERAR-ESTOQUE] Adicionando equipamento ao contexto...");
                            // ✅ CORREÇÃO: Usar método AdicionarSemSalvar (será salvo pela transação)
                            _equipamentoRepository.AdicionarSemSalvar(equipamento);
                            totalEquipamentosCriados++;
                            Console.WriteLine($"[LIBERAR-ESTOQUE] Equipamento adicionado ao contexto: Serial={serialNR}");

                            // ✅ CORREÇÃO CRÍTICA: Armazenar dados do histórico para criação posterior com ID correto
                            var historicoData = new
                            {
                                Serial = serialNR,
                                Status = 6, // Novo
                                Usuario = nf.Usuario,
                                Dtregistro = TimeZoneMapper.GetDateTimeNow(),
                                NotaFiscal = notaCompleta.Id,
                                Cliente = clienteId
                            };
                            equipamentohistoricoList.Add(historicoData);
                            Console.WriteLine($"[LIBERAR-ESTOQUE] Dados do histórico armazenados para equipamento Serial={serialNR}");
                        }
                    }

                    // Atualizar nota fiscal - usar Update diretamente para evitar problemas de transação
                    try
                    {
                        Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 Buscando nota fiscal ID {notaCompleta.Id} para atualização...");
                        var notaFiscalParaAtualizar = _notafiscalRepository.Buscar(x => x.Id == notaCompleta.Id).FirstOrDefault();
                        if (notaFiscalParaAtualizar != null)
                        {
                            Console.WriteLine($"[LIBERAR-ESTOQUE] ✅ Nota fiscal encontrada: ID={notaFiscalParaAtualizar.Id}, Numero={notaFiscalParaAtualizar.Numero}, Gerouequipamento={notaFiscalParaAtualizar.Gerouequipamento}");
                            notaFiscalParaAtualizar.Gerouequipamento = true;
                            Console.WriteLine($"[LIBERAR-ESTOQUE] 🔄 Marcando nota fiscal para atualização...");
                            
                            // Usar Update diretamente sem chamar SaveChanges (será chamado pela transação)
                            _notafiscalRepository.Atualizar(notaFiscalParaAtualizar);
                            Console.WriteLine($"[LIBERAR-ESTOQUE] ✅ Nota fiscal {notaCompleta.Id} marcada para atualização");
                        }
                        else
                        {
                            Console.WriteLine($"[LIBERAR-ESTOQUE] ⚠️ Nota fiscal {notaCompleta.Id} não encontrada para atualização");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LIBERAR-ESTOQUE] ⚠️ Erro ao atualizar nota fiscal: {ex.Message}");
                        Console.WriteLine($"[LIBERAR-ESTOQUE] ⚠️ Stack trace: {ex.StackTrace}");
                        // Continuar mesmo com erro na atualização da nota fiscal
                    }

                    // ✅ CORREÇÃO CRÍTICA: Salvar equipamentos primeiro para obter IDs
                    Console.WriteLine($"[LIBERAR-ESTOQUE] Salvando equipamentos para obter IDs...");
                    _equipamentoRepository.SalvarAlteracoes();
                    Console.WriteLine($"[LIBERAR-ESTOQUE] Equipamentos salvos com sucesso");

                    // ✅ CORREÇÃO CRÍTICA: Obter os IDs reais dos equipamentos salvos usando critério mais específico
                    Console.WriteLine($"[LIBERAR-ESTOQUE] Obtendo IDs reais dos equipamentos salvos...");
                    
                    // Buscar equipamentos criados nesta transação usando a nota fiscal e timestamp
                    var equipamentosSalvos = _equipamentoRepository.Buscar(x => 
                        x.Cliente == clienteId && 
                        x.Notafiscal == notaCompleta.Id &&
                        x.Dtcadastro >= DateTime.Now.AddMinutes(-5) // Últimos 5 minutos
                    ).OrderByDescending(x => x.Id).Take(totalEquipamentosCriados).ToList();
                    
                    Console.WriteLine($"[LIBERAR-ESTOQUE] Encontrados {equipamentosSalvos.Count} equipamentos salvos para nota fiscal {notaCompleta.Id}");

                    // ✅ CORREÇÃO CRÍTICA: Criar históricos com IDs corretos dos equipamentos
                    Console.WriteLine($"[LIBERAR-ESTOQUE] Criando históricos com IDs corretos...");
                    var historicosComIdsCorretos = new List<Equipamentohistorico>();
                    
                    // Ordenar equipamentos por ID para garantir correspondência
                    equipamentosSalvos = equipamentosSalvos.OrderBy(x => x.Id).ToList();
                    
                    // ✅ CORREÇÃO CRÍTICA: Criar históricos usando dados armazenados e IDs corretos
                    for (int i = 0; i < equipamentohistoricoList.Count && i < equipamentosSalvos.Count; i++)
                    {
                        var historicoData = equipamentohistoricoList[i];
                        var equipamentoSalvo = equipamentosSalvos[i];
                        
                        // Criar novo histórico com ID correto
                        var historico = new Equipamentohistorico
                        {
                            Equipamento = equipamentoSalvo.Id,
                            Equipamentostatus = historicoData.Status,
                            Usuario = historicoData.Usuario,
                            Dtregistro = historicoData.Dtregistro
                        };
                        
                        historicosComIdsCorretos.Add(historico);
                        Console.WriteLine($"[LIBERAR-ESTOQUE] Histórico {i+1}: Equipamento ID = {equipamentoSalvo.Id}, Serial = {equipamentoSalvo.Numeroserie}, Status = {historicoData.Status}");
                    }

                    // ✅ CORREÇÃO CRÍTICA: Verificar se todos os históricos foram criados corretamente
                    if (historicosComIdsCorretos.Count != totalEquipamentosCriados)
                    {
                        Console.WriteLine($"[LIBERAR-ESTOQUE] ⚠️ ATENÇÃO: Número de históricos ({historicosComIdsCorretos.Count}) não confere com equipamentos criados ({totalEquipamentosCriados})");
                    }

                    // Adicionar históricos com IDs corretos
                    Console.WriteLine($"[LIBERAR-ESTOQUE] Adicionando {historicosComIdsCorretos.Count} históricos ao contexto...");
                    foreach (var item in historicosComIdsCorretos)
                    {
                        _equipamentohistoricoRepository.AdicionarSemSalvar(item);
                    }
                    Console.WriteLine($"[LIBERAR-ESTOQUE] Históricos adicionados ao contexto com sucesso");

                    Console.WriteLine($"[LIBERAR-ESTOQUE] ✅ Processo concluído com sucesso! {totalEquipamentosCriados} equipamentos criados");
                    Console.WriteLine($"[LIBERAR-ESTOQUE] 🔍 RESUMO: {notaCompleta.Notasfiscaisitens.Count} itens processados, {totalEquipamentosCriados} equipamentos criados");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LIBERAR-ESTOQUE] ❌ Erro durante o processo: {ex.Message}");
                    Console.WriteLine($"[LIBERAR-ESTOQUE] ❌ Stack trace: {ex.StackTrace}");
                    throw;
                }
            });
            
            Console.WriteLine($"[LIBERAR-ESTOQUE] ✅ Transação executada com sucesso! Total de equipamentos criados: {totalEquipamentosCriados}");
        }

        private void ValidarItemNotaFiscal(Notasfiscaisiten nfi)
        {
            Console.WriteLine($"[VALIDAR-ITEM] 🔍 Validando item: Tipo={nfi.Tipoequipamento}, Fabricante={nfi.Fabricante}, Modelo={nfi.Modelo}, Contrato={nfi.Contrato}");
            
            // Validar Tipo de Equipamento
            var tipoExiste = _tipoequipamentoRepository.Buscar(x => x.Id == nfi.Tipoequipamento).Any();
            Console.WriteLine($"[VALIDAR-ITEM] 🔍 Tipo de equipamento {nfi.Tipoequipamento} existe: {tipoExiste}");
            if (!tipoExiste)
            {
                throw new ArgumentException($"Tipo de equipamento ID {nfi.Tipoequipamento} não encontrado");
            }

            // Validar Fabricante
            var fabricanteExiste = _fabricanteRepository.Buscar(x => x.Id == nfi.Fabricante).Any();
            Console.WriteLine($"[VALIDAR-ITEM] 🔍 Fabricante {nfi.Fabricante} existe: {fabricanteExiste}");
            if (!fabricanteExiste)
            {
                throw new ArgumentException($"Fabricante ID {nfi.Fabricante} não encontrado");
            }

            // Validar Modelo
            var modeloExiste = _modeloRepository.Buscar(x => x.Id == nfi.Modelo).Any();
            Console.WriteLine($"[VALIDAR-ITEM] 🔍 Modelo {nfi.Modelo} existe: {modeloExiste}");
            if (!modeloExiste)
            {
                throw new ArgumentException($"Modelo ID {nfi.Modelo} não encontrado");
            }

            // Validar Contrato (se informado)
            if (nfi.Contrato.HasValue)
            {
                var contratoExiste = _contratoRepository.Buscar(x => x.Id == nfi.Contrato.Value).Any();
                Console.WriteLine($"[VALIDAR-ITEM] 🔍 Contrato {nfi.Contrato} existe: {contratoExiste}");
                if (!contratoExiste)
                {
                    throw new ArgumentException($"Contrato ID {nfi.Contrato} não encontrado");
                }
            }
            else
            {
                Console.WriteLine($"[VALIDAR-ITEM] 🔍 Contrato não informado (opcional)");
            }
            
            Console.WriteLine($"[VALIDAR-ITEM] ✅ Item validado com sucesso");
        }
        #endregion


        /***************************************************************************************************/
        /******************************************** LAUDOS ***********************************************/
        /***************************************************************************************************/
        #region Laudo
        //public List<Laudo> ListarLaudos(string pesquisa, int cliente)
        //{
        //    pesquisa = pesquisa.ToLower();
        //    var laudos = db.Laudos
        //        .Include(x => x.EquipamentoNavigation)
        //            .ThenInclude(x => x.ModeloNavigation)
        //        .Include(x => x.EquipamentoNavigation)
        //            .ThenInclude(x => x.FabricanteNavigation)
        //        .Include(x => x.EquipamentoNavigation)
        //            .ThenInclude(x => x.TipoequipamentoNavigation)
        //        .Include(x => x.TecnicoNavigation)
        //        .Include(x => x.UsuarioNavigation)
        //        .Buscar(x => x.Ativo && x.Cliente == cliente &&
        //            ((pesquisa != "null") ? x.TecnicoNavigation.Nome.ToLower().Contains(pesquisa) ||
        //                                    x.UsuarioNavigation.Nome.ToLower().Contains(pesquisa) ||
        //                                    x.EquipamentoNavigation.Numeroserie.ToLower().Contains(pesquisa) ||
        //                                    x.EquipamentoNavigation.Patrimonio.ToLower().Contains(pesquisa)
        //           : 1 == 1))
        //        .OrderByDescending(x => x.Dtentrada).ToList();


        //    return laudos;
        //}
        public List<Vwlaudo> ListarLaudos(string pesquisa, int cliente)
        {
            try
            {
                var query = _vwLaudoRepository.Query().Where(x => x.Cliente == cliente);
                
                if (!string.IsNullOrEmpty(pesquisa) && pesquisa != "null")
                {
                    var pesquisaLower = pesquisa.ToLower();
                    
                    // Busca simplificada que o Entity Framework consegue traduzir
                    query = query.Where(x => 
                        (x.Tecniconome != null && x.Tecniconome.ToLower().Contains(pesquisaLower)) ||
                        (x.Usuarionome != null && x.Usuarionome.ToLower().Contains(pesquisaLower)) ||
                        (x.Numeroserie != null && x.Numeroserie.ToLower().Contains(pesquisaLower)) ||
                        (x.Patrimonio != null && x.Patrimonio.ToLower().Contains(pesquisaLower)) ||
                        (x.Equipamento != null && x.Equipamento.ToLower().Contains(pesquisaLower)) ||
                        (x.Descricao != null && x.Descricao.ToLower().Contains(pesquisaLower))
                    );
                }
                
                var laudos = query.OrderByDescending(x => x.Dtentrada).ToList();
                return laudos;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Laudo BuscarLaudoPorID(int id)
        {
            var laudos = _laudoRepository
                //.Include(x => x.Equipamento)
                //.ThenInclude(x => x.ModeloNavigation)
                .IncludeWithThenInclude(q => q.Include(x => x.EquipamentoNavigation).ThenInclude(x => x.ModeloNavigation))
                .Include(x => x.EquipamentoNavigation).ThenInclude(x => x.FabricanteNavigation)
                .Include(x => x.EquipamentoNavigation).ThenInclude(x => x.TipoequipamentoNavigation)
                .Include(x => x.TecnicoNavigation)
                .Include(x => x.UsuarioNavigation)
                .Where(x => x.Id == id).AsNoTracking().FirstOrDefault();

            return laudos;
        }

        public void SalvarLaudo(Laudo laudo)
        {
            if (laudo.Id == 0)
            {
                // ✅ CORREÇÃO: Usar ExecuteInTransaction para evitar conflito com estratégia de execução
                _laudoRepository.ExecuteInTransaction(() =>
                {
                    //Salvo o laudo
                    laudo.Dtentrada = TimeZoneMapper.GetDateTimeNow();
                    laudo.Ativo = true;
                    _laudoRepository.Adicionar(laudo);

                    //Atualizo o status do equipamento para danificado
                    var eqp = _equipamentoRepository.Buscar(x => x.Id == laudo.Equipamento).FirstOrDefault();
                    if (eqp != null)
                    {
                        eqp.Equipamentostatus = 1; //Danificado
                        _equipamentoRepository.Atualizar(eqp);

                        //Cancelo requisições ativas ou finalizo a entrega corrente
                        var req = _requisicaoRepository.Buscar(x => 
                            x.Requisicoesitens.Any(ri => ri.Equipamento == eqp.Id)
                        ).OrderByDescending(x => x.Id).FirstOrDefault();

                        if (req != null)
                        {
                            req.Requisicaostatus = 2;
                            _requisicaoRepository.Atualizar(req);
                        }

                        //Insiro o registro no histórico do equipamento
                        var hst = new Equipamentohistorico();
                        hst.Equipamento = laudo.Equipamento;
                        hst.Equipamentostatus = 1; //Danificado
                        hst.Usuario = laudo.Usuario;
                        hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }
                });
            }
            else
            {
                // ✅ CORREÇÃO: Usar ExecuteInTransaction para evitar conflito com estratégia de execução
                _laudoRepository.ExecuteInTransaction(() =>
                {
                    var atual = _laudoRepository.Buscar(x => x.Id == laudo.Id).FirstOrDefault();

                    if (laudo.Ativo != atual.Ativo)
                    {
                        //Busco o ultimo status antes de coloca-lo em manutenção
                        var ultimoStatus = _equipamentohistoricoRepository.Buscar(x => x.Equipamento == laudo.Equipamento && x.Equipamentostatus != 1).OrderByDescending(x => x.Dtregistro).Take(1).FirstOrDefault();

                        if (ultimoStatus != null)
                        {
                            //Atualizo o status do equipamento para o status de antes da manutenção
                            var eqp = _equipamentoRepository.Buscar(x => x.Id == laudo.Equipamento).FirstOrDefault();
                            if (eqp != null)
                            {
                                eqp.Equipamentostatus = ultimoStatus.Equipamentostatus;
                                _equipamentoRepository.Atualizar(eqp);

                                //Insiro novo histórico do equipamento
                                var hst = new Equipamentohistorico();
                                hst.Equipamento = eqp.Id;
                                hst.Equipamentostatus = ultimoStatus.Equipamentostatus;
                                hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                                hst.Usuario = laudo.Usuario;
                                _equipamentohistoricoRepository.Adicionar(hst);
                            }
                        }
                    }
                });
            }
        }
        public void EncerrarLaudo(Laudo laudo)
        {
            // ✅ CORREÇÃO: Usar ExecuteInTransaction para evitar conflito com estratégia de execução
            _laudoRepository.ExecuteInTransaction(() =>
            {
                bool encerrou = false;

                    //Salvo o laudo do equipamento
                    if (!String.IsNullOrEmpty(laudo.Laudo1))
                    {
                        laudo.Dtlaudo = TimeZoneMapper.GetDateTimeNow();
                        encerrou = true;
                        _laudoRepository.Atualizar(laudo);
                    }
                    //db.Entry(laudo).State = EntityState.Modified;
                    //db.SaveChanges();

                    //Busco o ultimo status antes de coloca-lo em manutenção
                    //var ultimoStatus = db.EquipamentoHistorico.Buscar(x => x.Equipamento == laudo.Equipamento && x.EquipamentoStatus != 1).OrderByDescending(x => x.DtRegistro).Take(1).FirstOrDefault();

                    //Atualizo o status do equipamento. Se tiver conserto, volta para 'Em estoque', do contrário, fica no status 'Sem conserto'
                    var eqp = _equipamentoRepository.Buscar(x => x.Id == laudo.Equipamento).FirstOrDefault();
                    eqp.Equipamentostatus = (laudo.Temconserto) ? 3 : 9;
                    //db.Entry(eqp).State = EntityState.Modified;
                    //db.SaveChanges();
                    _equipamentoRepository.Atualizar(eqp);

                    //Se equipamento não teve conserto, é necessário cancelar a requisição que o gerou (se tiver)
                    if (!laudo.Temconserto)
                    {
                        var req = _requisicaoItensRepository.Include(x => x.RequisicaoNavigation).Where(x => x.Equipamento == eqp.Id && x.RequisicaoNavigation.Requisicaostatus == 1).FirstOrDefault();
                        if (req != null)
                        {
                            req.RequisicaoNavigation.Requisicaostatus = 2; //Cancelada
                            //db.Update(req.RequisicaoNavigation);
                            //db.SaveChanges();
                            _requisicaoRepository.Atualizar(req.RequisicaoNavigation);
                        }
                    }


                    //Insiro novo histórico do equipamento
                    var hst = new Equipamentohistorico();
                    if (encerrou)
                    {
                        hst.Equipamento = eqp.Id;
                        hst.Equipamentostatus = (laudo.Temconserto) ? 3 : 9;
                        hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        hst.Usuario = laudo.Usuario;

                        //db.Equipamentohistoricos.Add(hst);
                        //db.SaveChanges();
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }

                    if (laudo.Mauuso)
                    {
                        _equipamentoNegocio.NotificarRH(eqp, false);
                    }
                });
        }
        public byte[] GerarLaudoEmPDF(int idLaudo, int? templateId = null)
        {
            try
            {
                Console.WriteLine($"[LAUDO] Gerando PDF para laudo ID: {idLaudo}, Template ID: {templateId}");
                
                var laudo = _laudoRepository.Include(x => x.TecnicoNavigation).Where(x => x.Id == idLaudo).FirstOrDefault();
                if (laudo == null)
                {
                    Console.WriteLine("[LAUDO] Laudo não encontrado");
                    throw new Exception($"Laudo com ID {idLaudo} não encontrado");
                }
                
                var eqpto = _equipamentoRepository
                                .Include(x => x.TipoequipamentoNavigation)
                                .Include(x => x.FabricanteNavigation)
                                .Include(x => x.ModeloNavigation)
                            .Where(x => x.Id == laudo.Equipamento).FirstOrDefault();
                
                if (eqpto == null)
                {
                    Console.WriteLine("[LAUDO] Equipamento não encontrado");
                    throw new Exception($"Equipamento com ID {laudo.Equipamento} não encontrado");
                }

                // Carregar evidências do laudo
                Console.WriteLine("[LAUDO] Carregando evidências do laudo");
                var evidencias = _laudoEvidenciaRepository.Buscar(x => x.laudo == idLaudo).ToList();
                Console.WriteLine($"[LAUDO] {evidencias.Count} evidências encontradas");

                string template;
                
                // Se templateId foi fornecido, usar template personalizado
                if (templateId.HasValue && templateId.Value > 0)
                {
                    Console.WriteLine($"[LAUDO] Usando template personalizado ID: {templateId.Value}");
                    var templatePersonalizado = _templateRepository.ObterPorId(templateId.Value);
                    if (templatePersonalizado != null && templatePersonalizado.Ativo == true)
                    {
                        template = templatePersonalizado.Conteudo;
                        Console.WriteLine($"[LAUDO] Template carregado: {templatePersonalizado.Titulo}");
                        Console.WriteLine($"[LAUDO] Conteúdo do template: {template}");
                        
                        // Adicionar CSS como no "Nada Consta"
                        var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "ckeditor.css");
                        if (File.Exists(file))
                        {
                            string css = File.ReadAllText(file);
                            template = template + "<style>" + css + "table{width:100%}</style>";
                            Console.WriteLine("[LAUDO] CSS adicionado ao template");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[LAUDO] Template personalizado não encontrado ou inativo, usando padrão");
                        // Fallback para template padrão
                        var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "template-laudo-tecnico.html");
                        template = File.ReadAllText(file);
                    }
                }
                else
                {
                    Console.WriteLine("[LAUDO] Usando template padrão");
                    // Usar template padrão
                    var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "template-laudo-tecnico.html");
                    template = File.ReadAllText(file);
                }

                Console.WriteLine("[LAUDO] Substituindo variáveis no template");
                
                // Substituir variáveis no template com verificação de null
                template = template.Replace("@fabricante", eqpto.FabricanteNavigation?.Descricao ?? "N/A")
                                    .Replace("@modelo", eqpto.ModeloNavigation?.Descricao ?? "N/A")
                                    .Replace("@tipo", eqpto.TipoequipamentoNavigation?.Descricao ?? "N/A")
                                    .Replace("@numeroSerie", eqpto.Numeroserie ?? "N/A")
                                    .Replace("@patrimonio", eqpto.Patrimonio ?? "N/A")
                                    .Replace("@laudoInicial", laudo.Descricao ?? "N/A")
                                    .Replace("@laudoFinal", laudo.Laudo1 ?? "N/A")
                                    .Replace("@mauUso", ((laudo.Mauuso == true) ? "Sim" : "Não"))
                                    .Replace("@tecnico", laudo.TecnicoNavigation?.Nome ?? "N/A")
                                    .Replace("@dataAtual", TimeZoneMapper.GetDateTimeNow().ToString("dddd, dd MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")));

                // Adicionar seção de evidências se houver
                if (evidencias.Any())
                {
                    Console.WriteLine("[LAUDO] Adicionando seção de evidências ao template");
                    string evidenciasHtml = "<div style='page-break-before: always; margin-top: 20px;'>";
                    evidenciasHtml += "<h3 style='text-align: center;'>EVIDÊNCIAS FOTOGRÁFICAS</h3>";
                    evidenciasHtml += "<div style='display: flex; flex-wrap: wrap; gap: 10px; justify-content: center;'>";
                    
                    foreach (var evidencia in evidencias)
                    {
                        try
                        {
                            // Ler o arquivo da evidência
                            string caminhoEvidencia = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos", "laudos", evidencia.nomearquivo);
                            if (File.Exists(caminhoEvidencia))
                            {
                                // Converter para base64
                                byte[] bytes = File.ReadAllBytes(caminhoEvidencia);
                                string base64 = Convert.ToBase64String(bytes);
                                string extensao = Path.GetExtension(evidencia.nomearquivo).ToLower();
                                string mimeType = extensao switch
                                {
                                    ".jpg" => "image/jpeg",
                                    ".jpeg" => "image/jpeg",
                                    ".png" => "image/png",
                                    ".gif" => "image/gif",
                                    _ => "image/jpeg"
                                };
                                
                                evidenciasHtml += $"<div style='text-align: center; margin: 10px;'>";
                                evidenciasHtml += $"<div style='font-weight: bold; font-size: 14px; margin-bottom: 5px;'>Evidência {evidencia.ordem}</div>";
                                evidenciasHtml += $"<img src='data:{mimeType};base64,{base64}' style='width: 250px; height: 250px; object-fit: cover; border: 1px solid #ccc;' />";
                                evidenciasHtml += $"<br><small style='font-size: 10px;'>{evidencia.nomearquivo}</small>";
                                evidenciasHtml += $"</div>";
                                
                                Console.WriteLine($"[LAUDO] Evidência adicionada: {evidencia.nomearquivo}");
                            }
                            else
                            {
                                Console.WriteLine($"[LAUDO] Arquivo de evidência não encontrado: {caminhoEvidencia}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[LAUDO] Erro ao processar evidência {evidencia.nomearquivo}: {ex.Message}");
                        }
                    }
                    
                    evidenciasHtml += "</div></div>";
                    template += evidenciasHtml;
                    Console.WriteLine("[LAUDO] Seção de evidências adicionada com sucesso");
                }
                else
                {
                    Console.WriteLine("[LAUDO] Nenhuma evidência encontrada para este laudo");
                }

                Console.WriteLine("[LAUDO] Convertendo HTML para PDF");
                Console.WriteLine($"[LAUDO] Template final para conversão: {template}");
                
                // Teste simples primeiro
                try {
                    var pdf = HtmlToPdfConverter.ConvertHtmlToPdf(template);
                    Console.WriteLine($"[LAUDO] PDF gerado com sucesso, tamanho: {pdf.Length} bytes");
                    
                    // Verificar se o PDF é válido
                    if (pdf != null && pdf.Length > 0) {
                        Console.WriteLine("[LAUDO] PDF parece válido");
                        // Verificar os primeiros bytes para confirmar que é PDF
                        if (pdf.Length >= 4) {
                            string header = System.Text.Encoding.ASCII.GetString(pdf, 0, 4);
                            Console.WriteLine($"[LAUDO] Primeiros bytes do PDF: {header}");
                            if (header == "%PDF") {
                                Console.WriteLine("[LAUDO] ✅ Header PDF válido detectado");
                            } else {
                                Console.WriteLine("[LAUDO] ❌ Header PDF inválido - não é um PDF real");
                                Console.WriteLine($"[LAUDO] Primeiros 20 bytes: {BitConverter.ToString(pdf.Take(20).ToArray())}");
                            }
                        }
                    } else {
                        Console.WriteLine("[LAUDO] ❌ PDF gerado é null ou vazio");
                    }
                    
                    return pdf;
                } catch (Exception ex) {
                    Console.WriteLine($"[LAUDO] ❌ Erro na conversão HTML para PDF: {ex.Message}");
                    Console.WriteLine($"[LAUDO] Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LAUDO] Erro ao gerar PDF: {ex.Message}");
                Console.WriteLine($"[LAUDO] Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** LOCALIZAÇÃO ******************************************/
        /***************************************************************************************************/
        #region Localidade
        public List<Localidade> ListarLocalidade(int cliente)
        {
            // ✅ DEBUG: Log para verificar o que está sendo retornado
            Console.WriteLine($"[LOCALIDADES-NEGOCIO] ==========================================");
            Console.WriteLine($"[LOCALIDADES-NEGOCIO] ListarLocalidade chamado - Cliente: {cliente}");
            Console.WriteLine($"[LOCALIDADES-NEGOCIO] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"[LOCALIDADES-NEGOCIO] ==========================================");
            
            try
            {
                // ✅ CORREÇÃO: Buscar todas as localidades EXCETO a "Padrão" (ID 1)
                // Primeiro agrupar por ID para remover duplicatas de ID
                var localidadesPorId = _localidadeRepository.Buscar(x => x.Cliente == cliente && x.Id != 1)
                    .GroupBy(x => x.Id)
                    .Select(g => g.First())
                    .ToList();
                
                // Depois agrupar por descrição (normalizada) para remover duplicatas de descrição
                var todasLocalidades = localidadesPorId
                    .GroupBy(x => (x.Descricao ?? "").Trim().ToLower())
                    .Select(g => g.First())
                    .OrderBy(x => x.Descricao)
                    .ToList();
                
                Console.WriteLine($"[LOCALIDADES-NEGOCIO] ✅ Total de localidades encontradas (excluindo Padrão e duplicatas): {todasLocalidades.Count}");
                
                // Log detalhado de cada localidade
                foreach (var localidade in todasLocalidades)
                {
                    Console.WriteLine($"[LOCALIDADES-NEGOCIO] ID: {localidade.Id}, Descrição: {localidade.Descricao}, Ativo: {localidade.Ativo}, Cliente: {localidade.Cliente}");
                }
                
                Console.WriteLine($"[LOCALIDADES-NEGOCIO] ==========================================");
                
                return todasLocalidades;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOCALIDADES-NEGOCIO] ❌ ERRO: {ex.Message}");
                Console.WriteLine($"[LOCALIDADES-NEGOCIO] StackTrace: {ex.StackTrace}");
                Console.WriteLine($"[LOCALIDADES-NEGOCIO] ==========================================");
                throw;
            }
        }
        public void SalvarLocalidade(Localidade local)
        {
            try
            {
                if (local.Id == 0)
                {
                    local.Ativo = true;
                    _localidadeRepository.Adicionar(local);
                }
                else
                {
                    _localidadeRepository.Atualizar(local);
                }
                
                // ✅ CORREÇÃO: Salvar alterações para persistir no banco
                _localidadeRepository.SalvarAlteracoes();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExcluirLocalidade(int id)
        {
            var local = _localidadeRepository.Buscar(x => x.Id == id).FirstOrDefault();
            try
            {
                //db.Remove(local);
                //db.SaveChanges();
                _localidadeRepository.Remover(local);
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        /// <summary>
        /// ✅ NOVO: Retorna apenas as localidades de uma empresa específica
        /// Garante consistência: colaborador só pode ter localidade da sua empresa
        /// </summary>
        public List<Localidade> ListarLocalidadesDaEmpresa(int empresaId)
        {
            Console.WriteLine($"[LOCALIDADES-CASCATA] ==========================================");
            Console.WriteLine($"[LOCALIDADES-CASCATA] ListarLocalidadesDaEmpresa - EmpresaId: {empresaId}");
            Console.WriteLine($"[LOCALIDADES-CASCATA] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            try
            {
                // Buscar a empresa para pegar sua localidade
                var empresa = _empresaRepository.Buscar(x => x.Id == empresaId)
                    .Include(e => e.Localidade)
                    .FirstOrDefault();
                
                if (empresa == null)
                {
                    Console.WriteLine($"[LOCALIDADES-CASCATA] ❌ Empresa não encontrada: {empresaId}");
                    return new List<Localidade>();
                }
                
                Console.WriteLine($"[LOCALIDADES-CASCATA] ✅ Empresa encontrada: {empresa.Nome}");
                Console.WriteLine($"[LOCALIDADES-CASCATA] ✅ LocalidadeId da empresa: {empresa.LocalidadeId}");
                
                // Buscar todas as filiais desta empresa para pegar TODAS as localidades relacionadas
                var filiaisEmpresa = _filialRepository
                    .Buscar(f => f.EmpresaId == empresaId && f.Ativo == true)
                    .Include(f => f.Localidade)
                    .ToList();
                
                Console.WriteLine($"[LOCALIDADES-CASCATA] ✅ Filiais da empresa: {filiaisEmpresa.Count}");
                
                // Pegar IDs de localidades únicas (da empresa + das filiais)
                var localidadeIds = filiaisEmpresa
                    .Where(f => f.LocalidadeId > 0)
                    .Select(f => f.LocalidadeId)
                    .Distinct()
                    .ToList();
                
                // Adicionar a localidade da empresa se tiver
                if (empresa.LocalidadeId.HasValue && !localidadeIds.Contains(empresa.LocalidadeId.Value))
                {
                    localidadeIds.Add(empresa.LocalidadeId.Value);
                }
                
                Console.WriteLine($"[LOCALIDADES-CASCATA] ✅ IDs de localidades únicas: {string.Join(", ", localidadeIds)}");
                
                // Buscar as localidades
                var localidades = _localidadeRepository
                    .Buscar(l => localidadeIds.Contains(l.Id) && l.Ativo == true)
                    .OrderBy(l => l.Descricao)
                    .ToList();
                
                Console.WriteLine($"[LOCALIDADES-CASCATA] ✅ Total de localidades retornadas: {localidades.Count}");
                
                foreach (var loc in localidades)
                {
                    Console.WriteLine($"[LOCALIDADES-CASCATA]   → ID: {loc.Id}, Descrição: {loc.Descricao}");
                }
                
                Console.WriteLine($"[LOCALIDADES-CASCATA] ==========================================");
                
                return localidades;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOCALIDADES-CASCATA] ❌ ERRO: {ex.Message}");
                Console.WriteLine($"[LOCALIDADES-CASCATA] StackTrace: {ex.StackTrace}");
                Console.WriteLine($"[LOCALIDADES-CASCATA] ==========================================");
                throw;
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** FILIAIS **********************************************/
        /***************************************************************************************************/
        #region Filial
        public List<Filial> ListarFiliais(string pesquisa, int cliente)
        {
            Console.WriteLine($"[FILIAIS-NEGOCIO] ==========================================");
            Console.WriteLine($"[FILIAIS-NEGOCIO] ListarFiliais - Pesquisa: '{pesquisa}', Cliente: {cliente}");
            Console.WriteLine($"[FILIAIS-NEGOCIO] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"[FILIAIS-NEGOCIO] ==========================================");
            
            try
            {
                Console.WriteLine($"[FILIAIS-NEGOCIO] Iniciando busca no repositório...");
                
                // Validar parâmetros
                if (cliente <= 0)
                {
                    Console.WriteLine($"[FILIAIS-NEGOCIO] ❌ Cliente inválido: {cliente}");
                    return new List<Filial>();
                }
                
                // Primeiro, buscar empresas do cliente
                Console.WriteLine($"[FILIAIS-NEGOCIO] 🔍 Buscando empresas para cliente {cliente}...");
                var empresasDoCliente = _empresaRepository.Buscar(x => x.Cliente == cliente).ToList();
                Console.WriteLine($"[FILIAIS-NEGOCIO] ✅ Empresas encontradas para cliente {cliente}: {empresasDoCliente.Count}");
                
                if (!empresasDoCliente.Any())
                {
                    Console.WriteLine($"[FILIAIS-NEGOCIO] ⚠️ Nenhuma empresa encontrada para o cliente {cliente}");
                    return new List<Filial>();
                }
                
                var idsEmpresas = empresasDoCliente.Select(e => e.Id).ToList();
                Console.WriteLine($"[FILIAIS-NEGOCIO] 🔍 IDs das empresas: {string.Join(", ", idsEmpresas)}");
                
                // Agora buscar filiais dessas empresas
                Console.WriteLine($"[FILIAIS-NEGOCIO] 🔍 Buscando filiais ativas para empresas do cliente {cliente}...");
                var query = _filialRepository.Buscar(x => x.Ativo == true && idsEmpresas.Contains(x.EmpresaId));
                Console.WriteLine($"[FILIAIS-NEGOCIO] ✅ Query inicial: {query.Count()} filiais ativas para empresas do cliente {cliente}");
                
                // Aplicar filtro de pesquisa se fornecido
                if (!string.IsNullOrEmpty(pesquisa) && pesquisa != "null" && pesquisa != "empty")
                {
                    Console.WriteLine($"[FILIAIS-NEGOCIO] Aplicando filtro de pesquisa: '{pesquisa}'");
                    try
                    {
                        // Buscar filiais que contêm a pesquisa no nome ou CNPJ
                        var pesquisaUpper = pesquisa.ToUpper();
                        query = query.Where(x => 
                            (x.Nome != null && x.Nome.ToUpper().Contains(pesquisaUpper)) || 
                            (x.Cnpj != null && x.Cnpj.Contains(pesquisa)));
                        
                        Console.WriteLine($"[FILIAIS-NEGOCIO] Após filtro de pesquisa: {query.Count()} filiais");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[FILIAIS-NEGOCIO] ❌ Erro ao aplicar filtro de pesquisa: {ex.Message}");
                        // Continuar sem filtro se houver erro
                    }
                }
                
                Console.WriteLine($"[FILIAIS-NEGOCIO] 🔍 Aplicando Includes e ordenação...");
                List<Filial> resultado;
                try
                {
                    Console.WriteLine($"[FILIAIS-NEGOCIO] 🔍 Aplicando Include para Empresa...");
                    var queryComEmpresa = query.Include(x => x.Empresa);
                    Console.WriteLine($"[FILIAIS-NEGOCIO] ✅ Include Empresa aplicado");
                    
                    Console.WriteLine($"[FILIAIS-NEGOCIO] 🔍 Aplicando Include para Localidade...");
                    var queryComLocalidade = queryComEmpresa.Include(x => x.Localidade);
                    Console.WriteLine($"[FILIAIS-NEGOCIO] ✅ Include Localidade aplicado");
                    
                    Console.WriteLine($"[FILIAIS-NEGOCIO] 🔍 Aplicando ordenação...");
                    var queryOrdenada = queryComLocalidade.OrderBy(x => x.Nome);
                    Console.WriteLine($"[FILIAIS-NEGOCIO] ✅ Ordenação aplicada");
                    
                    Console.WriteLine($"[FILIAIS-NEGOCIO] 🔍 Executando ToList()...");
                    resultado = queryOrdenada.ToList();
                    Console.WriteLine($"[FILIAIS-NEGOCIO] ✅ ToList() executado com sucesso");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FILIAIS-NEGOCIO] ❌ Erro ao aplicar Includes: {ex.Message}");
                    Console.WriteLine($"[FILIAIS-NEGOCIO] ❌ StackTrace: {ex.StackTrace}");
                    
                    // Tentar sem Includes se houver erro
                    Console.WriteLine($"[FILIAIS-NEGOCIO] 🔄 Tentando sem Includes...");
                    try
                    {
                        resultado = query.OrderBy(x => x.Nome).ToList();
                        Console.WriteLine($"[FILIAIS-NEGOCIO] ✅ Lista obtida sem Includes: {resultado.Count} filiais");
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine($"[FILIAIS-NEGOCIO] ❌❌ Erro também sem Includes: {ex2.Message}");
                        resultado = new List<Filial>();
                    }
                }
                
                Console.WriteLine($"[FILIAIS-NEGOCIO] Resultado final: {resultado.Count} filiais");
                
                if (resultado.Count > 0)
                {
                    Console.WriteLine($"[FILIAIS-NEGOCIO] Primeira filial: ID={resultado[0].Id}, Nome={resultado[0].Nome}, Empresa={resultado[0].Empresa?.Nome}, Localidade={resultado[0].Localidade?.Descricao}");
                }
                
                Console.WriteLine($"[FILIAIS-NEGOCIO] ==========================================");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILIAIS-NEGOCIO] ❌ ERRO: {ex.Message}");
                Console.WriteLine($"[FILIAIS-NEGOCIO] StackTrace: {ex.StackTrace}");
                Console.WriteLine($"[FILIAIS-NEGOCIO] ==========================================");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVO: Retorna apenas as filiais de uma empresa específica
        /// Garante consistência: colaborador só pode ter filial da sua empresa
        /// </summary>
        public List<Filial> ListarFiliaisDaEmpresa(int empresaId)
        {
            Console.WriteLine($"[FILIAIS-CASCATA] ==========================================");
            Console.WriteLine($"[FILIAIS-CASCATA] ListarFiliaisDaEmpresa - EmpresaId: {empresaId}");
            Console.WriteLine($"[FILIAIS-CASCATA] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            try
            {
                var filiais = _filialRepository
                    .Buscar(f => f.EmpresaId == empresaId && f.Ativo == true)
                    .Include(f => f.Localidade)
                    .Include(f => f.Empresa)
                    .OrderBy(f => f.Nome)
                    .ToList();
                
                Console.WriteLine($"[FILIAIS-CASCATA] ✅ Total de filiais retornadas: {filiais.Count}");
                
                foreach (var fil in filiais)
                {
                    Console.WriteLine($"[FILIAIS-CASCATA]   → ID: {fil.Id}, Nome: {fil.Nome}, LocalidadeId: {fil.LocalidadeId}, Localidade: {fil.Localidade?.Descricao}");
                }
                
                Console.WriteLine($"[FILIAIS-CASCATA] ==========================================");
                
                return filiais;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILIAIS-CASCATA] ❌ ERRO: {ex.Message}");
                Console.WriteLine($"[FILIAIS-CASCATA] StackTrace: {ex.StackTrace}");
                Console.WriteLine($"[FILIAIS-CASCATA] ==========================================");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVO: Retorna apenas as filiais de uma empresa E localidade específicas
        /// Garante consistência: colaborador só pode ter filial da sua empresa E localidade
        /// </summary>
        public List<Filial> ListarFiliaisPorLocalidade(int empresaId, int localidadeId)
        {
            Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA] ==========================================");
            Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA] ListarFiliaisPorLocalidade - EmpresaId: {empresaId}, LocalidadeId: {localidadeId}");
            Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            try
            {
                var filiais = _filialRepository
                    .Buscar(f => f.EmpresaId == empresaId && f.LocalidadeId == localidadeId && f.Ativo == true)
                    .Include(f => f.Localidade)
                    .Include(f => f.Empresa)
                    .OrderBy(f => f.Nome)
                    .ToList();
                
                Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA] ✅ Total de filiais retornadas: {filiais.Count}");
                
                foreach (var fil in filiais)
                {
                    Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA]   → ID: {fil.Id}, Nome: {fil.Nome}, LocalidadeId: {fil.LocalidadeId}, Localidade: {fil.Localidade?.Descricao}");
                }
                
                Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA] ==========================================");
                
                return filiais;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA] ❌ ERRO: {ex.Message}");
                Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA] StackTrace: {ex.StackTrace}");
                Console.WriteLine($"[FILIAIS-LOCALIDADE-CASCATA] ==========================================");
                throw;
            }
        }

        public Filial BuscarFilialPeloID(int id)
        {
            return _filialRepository.Buscar(x => x.Id == id)
                                  .Include(x => x.Empresa)
                                  .Include(x => x.Localidade)
                                  .FirstOrDefault();
        }

        public string SalvarFilial(Filial filial)
        {
            try
            {
                            Console.WriteLine($"[SALVAR FILIAL] 🔍 Iniciando salvamento da filial: {filial.Nome}");
            Console.WriteLine($"[SALVAR FILIAL] 🔍 ID: {filial.Id}, Empresa: {filial.EmpresaId}, CNPJ: {filial.Cnpj}");
            Console.WriteLine($"[SALVAR FILIAL] 🔍 Status Ativo: {filial.Ativo}");
                
                // Validação de CNPJ
                if (!string.IsNullOrWhiteSpace(filial.Cnpj))
                {
                    if (!CnpjValidator.IsValid(filial.Cnpj))
                    {
                        Console.WriteLine($"[SALVAR FILIAL] ❌ CNPJ inválido: {filial.Cnpj}");
                        return JsonConvert.SerializeObject(new { 
                            Mensagem = "❌ CNPJ inválido! O CNPJ informado não é válido. Verifique se possui 14 dígitos e se os dígitos verificadores estão corretos.", 
                            Status = "400",
                            Tipo = "CNPJ_INVALIDO",
                            CNPJ_Informado = filial.Cnpj,
                            Sugestao = "Digite um CNPJ válido com 14 dígitos no formato XX.XXX.XXX/XXXX-XX"
                        });
                    }
                    
                    // Formata o CNPJ antes de salvar
                    filial.Cnpj = CnpjValidator.Format(filial.Cnpj);
                    Console.WriteLine($"[SALVAR FILIAL] 🔍 CNPJ formatado: {filial.Cnpj}");
                }
                
                // 🔧 CORREÇÃO: Garantir que o campo Ativo seja sempre preservado
                if (!filial.Ativo.HasValue)
                {
                    filial.Ativo = true;
                    Console.WriteLine("[SALVAR FILIAL] ✅ Campo Ativo definido como true (valor padrão)");
                }
                
                var resultado = _filialRepository.ExecuteInTransaction(() =>
                {
                    Console.WriteLine($"[SALVAR FILIAL] 🔍 Dentro da transação - ID: {filial.Id}");
                    
                    if (filial.Id == 0)
                    {
                        Console.WriteLine("[SALVAR FILIAL] 🔍 Criando nova filial...");
                        
                        // Verificar se já existe filial com mesmo CNPJ na mesma empresa
                        var existe = _filialRepository.Buscar(x => x.Cnpj == filial.Cnpj && x.EmpresaId == filial.EmpresaId).Any();
                        Console.WriteLine($"[SALVAR FILIAL] 🔍 Verificação de duplicata - Existe: {existe}");
                        
                        if (!existe)
                        {
                            Console.WriteLine("[SALVAR FILIAL] 🔍 Adicionando filial ao repositório...");
                            filial.Ativo = true;
                            filial.CreatedAt = DateTime.Now;
                            filial.UpdatedAt = DateTime.Now;
                            _filialRepository.Adicionar(filial);
                            Console.WriteLine($"[SALVAR FILIAL] ✅ Filial adicionada com sucesso! ID após adição: {filial.Id}");
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "✅ Filial salva com sucesso! A filial foi cadastrada no sistema e está disponível para uso.", 
                                Status = "200",
                                Tipo = "SUCESSO",
                                Filial = filial.Nome,
                                CNPJ = filial.Cnpj
                            });
                        }
                        else
                        {
                            Console.WriteLine("[SALVAR FILIAL] ⚠️ Filial já existe com este CNPJ");
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "⚠️ CNPJ já cadastrado! Este CNPJ já está sendo usado por outra filial na mesma empresa. Verifique se não é a mesma filial ou use um CNPJ diferente.", 
                                Status = "400.1",
                                Tipo = "CNPJ_DUPLICADO",
                                Sugestao = "Verifique se não está tentando cadastrar a mesma filial ou use um CNPJ diferente."
                            });
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[SALVAR FILIAL] 🔍 Atualizando filial existente com ID: {filial.Id}");
                        
                        // Verificar se já existe filial com mesmo CNPJ na mesma empresa (excluindo a atual)
                        var existe = _filialRepository.Buscar(x => x.Cnpj == filial.Cnpj && x.EmpresaId == filial.EmpresaId && x.Id != filial.Id).Any();
                        Console.WriteLine($"[SALVAR FILIAL] 🔍 Verificação de duplicata - Existe: {existe}");
                        
                        if (!existe)
                        {
                            Console.WriteLine("[SALVAR FILIAL] 🔍 Atualizando filial no repositório...");
                            filial.UpdatedAt = DateTime.Now;
                            _filialRepository.Atualizar(filial);
                            Console.WriteLine("[SALVAR FILIAL] ✅ Filial atualizada com sucesso!");
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "✅ Filial atualizada com sucesso! As informações foram modificadas e salvas no sistema.", 
                                Status = "200",
                                Tipo = "SUCESSO",
                                Filial = filial.Nome,
                                CNPJ = filial.Cnpj,
                                ID = filial.Id
                            });
                        }
                        else
                        {
                            Console.WriteLine("[SALVAR FILIAL] ⚠️ Filial já existe com este CNPJ");
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "⚠️ CNPJ já cadastrado! Este CNPJ já está sendo usado por outra filial na mesma empresa. Verifique se não é a mesma filial ou use um CNPJ diferente.", 
                                Status = "400.1",
                                Tipo = "CNPJ_DUPLICADO",
                                Sugestao = "Verifique se não está tentando cadastrar a mesma filial ou use um CNPJ diferente."
                            });
                        }
                    }
                });
                
                Console.WriteLine($"[SALVAR FILIAL] ✅ Transação executada com sucesso. Resultado: {resultado}");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SALVAR FILIAL] ❌ ERRO: {ex.Message}");
                Console.WriteLine($"[SALVAR FILIAL] ❌ StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SALVAR FILIAL] ❌ InnerException: {ex.InnerException.Message}");
                }
                return JsonConvert.SerializeObject(new { Mensagem = $"Erro ao salvar filial: {ex.Message}", Status = "500" });
            }
        }

        public string ExcluirFilial(int id)
        {
            try
            {
                var filial = _filialRepository.Buscar(x => x.Id == id).FirstOrDefault();
                if (filial == null)
                    return "Filial não encontrada!";

                // Verificar se há relacionamentos ativos
                var temColaboradores = _colaboradorRepository.Buscar(x => x.FilialId == id && x.Situacao == "A").Any();
                var temEquipamentos = _equipamentoRepository.Buscar(x => x.FilialId == id && x.Ativo == true).Any();
                var temCentrosCusto = _centroCustoRepository.Buscar(x => x.FilialId == id).Any();

                if (temColaboradores || temEquipamentos || temCentrosCusto)
                    return "Não é possível excluir a filial. Existem colaboradores, equipamentos ou centros de custo vinculados.";

                filial.Ativo = false;
                filial.UpdatedAt = DateTime.Now;
                _filialRepository.Atualizar(filial);
                return "Filial excluída com sucesso!";
            }
            catch (Exception ex)
            {
                return $"Erro ao excluir filial: {ex.Message}";
            }
        }
        #endregion

        /***************************************************************************************************/
        /******************************************** TEMPLATES ********************************************/
        /***************************************************************************************************/
        #region Templates
        public List<Templatetipo> ListarTiposDeTemplate()
        {
            var tipos = _templatetipoRepository.ObterTodos().OrderBy(x => x.Id).ToList();
            return tipos;
        }
        public List<Template> ListarTemplates(int cliente)
        {
            var templates = _templateRepository.Include(x => x.TipoNavigation).Where(x => x.Cliente == cliente && x.Ativo == true).OrderBy(x => x.Titulo).ToList();
            return templates;
        }
        
        public List<Template> ListarTemplatesPorTipo(int cliente, int tipo)
        {
            var templates = _templateRepository.Include(x => x.TipoNavigation)
                .Where(x => x.Cliente == cliente && x.Tipo == tipo && x.Ativo == true)
                .OrderBy(x => x.Titulo)
                .ToList();
            return templates;
        }
        public Template ObterTemplatePorId(int id)
        {
            var tmp = _templateRepository.ObterPorId(id);
            return tmp;
        }
        public void SalvarTemplate(Template t)
        {
            // ✅ CORREÇÃO: Usar ExecuteInTransaction para garantir SaveChanges
            _templateRepository.ExecuteInTransaction(() =>
            {
                if (t.Id == 0)
                {
                    t.Ativo = true;
                    t.Versao = 1;
                    t.DataCriacao = TimeZoneMapper.GetDateTimeNow();
                    _templateRepository.Adicionar(t);
                }
                else
                {
                    t.Versao++;
                    t.DataAlteracao = TimeZoneMapper.GetDateTimeNow();
                    _templateRepository.Atualizar(t);
                }
            });
        }
        public void ExcluirTemplate(int id)
        {
            var tmp = _templateRepository.ObterPorId(id);
            try
            {
                //db.Remove(tmp);
                //db.SaveChanges();
                _templateRepository.Remover(tmp);
            }
            catch
            {
                tmp.Ativo = false;
                //db.Update(tmp);
                //db.SaveChanges();
                _templateRepository.Atualizar(tmp);
            }
        }
        public byte[] VisualizarTemplate(TemplateVM template)
        {
            try
            {
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] === INICIANDO VISUALIZAÇÃO ===");
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] UsuarioLogado: {template?.UsuarioLogado ?? 0}");
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] Conteudo length: {template?.Conteudo?.Length ?? 0}");
                
                // ✅ CORREÇÃO: Validar template antes de processar
                if (template == null)
                {
                    Console.WriteLine("[VISUALIZAR-TEMPLATE] ❌ Template é nulo");
                    throw new Exception("Template não pode ser nulo");
                }

                // ✅ CORREÇÃO: Validar usuário antes de usar
                if (template.UsuarioLogado <= 0)
                {
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] ❌ UsuarioLogado inválido: {template.UsuarioLogado}");
                    throw new Exception($"ID de usuário inválido: {template.UsuarioLogado}");
                }

                Console.WriteLine($"[VISUALIZAR-TEMPLATE] Buscando usuário ID: {template.UsuarioLogado}");
                var usu = _usuarioRepository.ObterPorId(template.UsuarioLogado);
                if (usu == null)
                {
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] ❌ Usuário não encontrado: {template.UsuarioLogado}");
                    throw new Exception($"Usuário com ID {template.UsuarioLogado} não encontrado");
                }
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] ✅ Usuário encontrado: {usu.Nome}");

                // ✅ CORREÇÃO: Validar conteúdo antes de processar
                if (string.IsNullOrEmpty(template.Conteudo))
                {
                    Console.WriteLine("[VISUALIZAR-TEMPLATE] ❌ Conteúdo vazio");
                    throw new Exception("Conteúdo do template não pode ser vazio");
                }
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] ✅ Conteúdo válido: {template.Conteudo.Length} caracteres");

                var nomeUsuario = usu.Nome ?? "Usuário";
                template.Conteudo = template.Conteudo
                                .Replace("@nomeEmpresa", "SingleOne Tech")
                                .Replace("@cnpjEmpresa", "00.000.000/0001-00")
                                .Replace("@centroCusto", "Diretoria")
                                .Replace("@nomeColaborador", "Roberto Carlos")
                                .Replace("@cargo", "Presidente")
                                .Replace("@matricula", "M01")
                                .Replace("@equipamentos", "")
                                .Replace("@usuarioLogado", nomeUsuario)
                                .Replace("@dataAtual", TimeZoneMapper.GetDateTimeNow().ToString("dddd, dd MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")))
                                .Replace("@dataUltimaAtual", TimeZoneMapper.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm"))
                                .Replace("@versao", $"Versão: 2")
                                .Replace("@tipoColaborador", "Funcionário");

                // ✅ CORREÇÃO: Verificar se o arquivo CSS existe antes de ler
                var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "ckeditor.css");
                string css = "";
                if (File.Exists(file))
                {
                    css = File.ReadAllText(file);
                }
                else
                {
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] ⚠️ Arquivo CSS não encontrado: {file}");
                    // Usar CSS padrão mínimo se o arquivo não existir
                    css = "body { font-family: Arial, sans-serif; } table { width: 100%; border-collapse: collapse; }";
                }
                template.Conteudo = template.Conteudo + "<style>" + css + "table{width:100%}</style>";

                // ✅ CORREÇÃO: Validar HTML antes de converter
                if (string.IsNullOrWhiteSpace(template.Conteudo))
                {
                    throw new Exception("Conteúdo HTML vazio após processamento");
                }

                Console.WriteLine($"[VISUALIZAR-TEMPLATE] Iniciando conversão HTML para PDF...");
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] HTML length: {template.Conteudo.Length} caracteres");
                
                byte[] pdf = null;
                try
                {
                    //var pdf = _generatePdf.GetPDF(template.Conteudo);
                    pdf = HtmlToPdfConverter.ConvertHtmlToPdf(template.Conteudo);
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] ✅ Conversão concluída - PDF size: {pdf?.Length ?? 0} bytes");
                }
                catch (Exception pdfEx)
                {
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] ❌ Erro ao converter HTML para PDF: {pdfEx.Message}");
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] Tipo de exceção: {pdfEx.GetType().Name}");
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] StackTrace: {pdfEx.StackTrace}");
                    if (pdfEx.InnerException != null)
                    {
                        Console.WriteLine($"[VISUALIZAR-TEMPLATE] InnerException: {pdfEx.InnerException.Message}");
                    }
                    throw new Exception($"Erro ao converter HTML para PDF: {pdfEx.Message}", pdfEx);
                }
                
                if (pdf == null)
                {
                    Console.WriteLine("[VISUALIZAR-TEMPLATE] ❌ PDF é nulo após conversão");
                    throw new Exception("Erro ao gerar PDF: resultado nulo após conversão");
                }
                
                if (pdf.Length == 0)
                {
                    Console.WriteLine("[VISUALIZAR-TEMPLATE] ❌ PDF vazio após conversão");
                    throw new Exception("Erro ao gerar PDF: resultado vazio após conversão");
                }

                Console.WriteLine($"[VISUALIZAR-TEMPLATE] ✅ PDF gerado com sucesso - Tamanho: {pdf.Length} bytes");
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] === VISUALIZAÇÃO CONCLUÍDA COM SUCESSO ===");
                return pdf;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] ❌ ERRO GERAL: {ex.Message}");
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] Tipo de exceção: {ex.GetType().Name}");
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] InnerException: {ex.InnerException.Message}");
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] InnerException StackTrace: {ex.InnerException.StackTrace}");
                }
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] === ERRO NA VISUALIZAÇÃO ===");
                throw; // Re-lançar para o controller tratar
            }
        }
        #endregion


        /***************************************************************************************************/
        /******************************************** PARAMÊTROS *******************************************/
        /***************************************************************************************************/
        #region Parametro
        public Parametro ObterParametros(int cliente)
        {
            var param = _parametroRepository.Buscar(x => x.Cliente == cliente).FirstOrDefault();
            
            Console.WriteLine($"[PARAMETROS] Buscando parâmetros para cliente: {cliente}");
            if (param != null)
            {
                Console.WriteLine($"[PARAMETROS] Parâmetros encontrados:");
                Console.WriteLine($"[PARAMETROS] - ID: {param.Id}");
                Console.WriteLine($"[PARAMETROS] - Cliente: {param.Cliente}");
                Console.WriteLine($"[PARAMETROS] - EmailReporte: {param.Emailreporte}");
                Console.WriteLine($"[PARAMETROS] - EmailDescontosEnabled: {param.EmailDescontosEnabled}");
                Console.WriteLine($"[PARAMETROS] - SmtpEnabled: {param.SmtpEnabled}");
            }
            else
            {
                Console.WriteLine($"[PARAMETROS] Nenhum parâmetro encontrado para cliente: {cliente}");
            }
            
            return param;
        }
        public void SalvarParametro(Parametro p)
        {
            if (p.Id == 0)
            {
                //db.Add(p);
                _parametroRepository.Adicionar(p);
            }
            else
            {
                //db.Update(p);
                _parametroRepository.Atualizar(p);
            }
        }
        #endregion


        /***************************************************************************************************/
        /**************************************** LAUDO EVIDÊNCIAS *****************************************/
        /***************************************************************************************************/
        #region LaudoEvidencias
        public List<LaudoEvidencia> ListarEvidenciasLaudo(int laudoId)
        {
            try
            {
                Console.WriteLine($"[LAUDO] Listando evidências para laudo ID: {laudoId}");
                
                var evidencias = _laudoEvidenciaRepository.Query()
                    .Where(x => x.laudo == laudoId)
                    .OrderBy(x => x.ordem)
                    .ToList();
                
                Console.WriteLine($"[LAUDO] Evidências encontradas: {evidencias.Count}");
                
                return evidencias;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LAUDO] Erro ao listar evidências: {ex.Message}");
                Console.WriteLine($"[LAUDO] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void SalvarEvidenciaLaudo(LaudoEvidencia evidencia)
        {
            try
            {
                Console.WriteLine($"[NEGOCIO] Salvando evidência: ID={evidencia.Id}, laudo={evidencia.laudo}, nomearquivo={evidencia.nomearquivo}, ordem={evidencia.ordem}");
                
                if (evidencia.Id == 0)
                {
                    Console.WriteLine($"[NEGOCIO] Adicionando nova evidência...");
                    _laudoEvidenciaRepository.Adicionar(evidencia);
                    // ✅ IMPORTANTE: Salvar alterações para obter o ID gerado
                    _laudoEvidenciaRepository.SalvarAlteracoes();
                    Console.WriteLine($"[NEGOCIO] Evidência adicionada com sucesso - Novo ID: {evidencia.Id}");
                }
                else
                {
                    Console.WriteLine($"[NEGOCIO] Atualizando evidência existente...");
                    _laudoEvidenciaRepository.Atualizar(evidencia);
                    // ✅ IMPORTANTE: Salvar alterações
                    _laudoEvidenciaRepository.SalvarAlteracoes();
                    Console.WriteLine($"[NEGOCIO] Evidência atualizada com sucesso");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NEGOCIO] Erro ao salvar evidência: {ex.Message}");
                Console.WriteLine($"[NEGOCIO] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void ExcluirEvidenciaLaudo(int evidenciaId)
        {
            var evidencia = _laudoEvidenciaRepository.ObterPorId(evidenciaId);
            if (evidencia != null)
            {
                // Como não há coluna Ativo, vamos remover fisicamente a evidência
                _laudoEvidenciaRepository.Remover(evidenciaId);
            }
        }

        public dynamic ObterEvidenciaPorId(int evidenciaId)
        {
            var evidencia = _laudoEvidenciaRepository.ObterPorId(evidenciaId);
            if (evidencia == null) return null;

            return new
            {
                Id = evidencia.Id,
                nomearquivo = evidencia.nomearquivo,
                ordem = evidencia.ordem,
                laudo = evidencia.laudo
            };
        }

        public void ReordenarEvidenciasLaudo(int laudoId, List<int> ordemEvidencias)
        {
            try
            {
                Console.WriteLine($"[LAUDO] Reordenando evidências para laudo ID: {laudoId}");
                
                // Como não há coluna Ativo, buscar todas as evidências do laudo
                var evidencias = _laudoEvidenciaRepository.Query()
                    .Where(x => x.laudo == laudoId)
                    .ToList();
                
                Console.WriteLine($"[LAUDO] Evidências encontradas para reordenação: {evidencias.Count}");
                
                for (int i = 0; i < ordemEvidencias.Count; i++)
                {
                    var evidencia = evidencias.FirstOrDefault(x => x.Id == ordemEvidencias[i]);
                    if (evidencia != null)
                    {
                        evidencia.ordem = i + 1;
                        _laudoEvidenciaRepository.Atualizar(evidencia);
                        Console.WriteLine($"[LAUDO] Evidência ID {evidencia.Id} reordenada para posição {i + 1}");
                    }
                }
                
                // ✅ IMPORTANTE: Salvar todas as alterações de uma vez
                _laudoEvidenciaRepository.SalvarAlteracoes();
                Console.WriteLine($"[LAUDO] Todas as evidências foram reordenadas e salvas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LAUDO] Erro ao reordenar evidências: {ex.Message}");
                throw;
            }
        }

        public int ObterProximaOrdemEvidencia(int laudoId)
        {
            try
            {
                var ultimaOrdem = _laudoEvidenciaRepository.Query()
                    .Where(x => x.laudo == laudoId)
                    .Max(x => (int?)x.ordem) ?? 0;
                return ultimaOrdem + 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LAUDO] Erro ao obter próxima ordem: {ex.Message}");
                return 1; // Retornar 1 como fallback
            }
        }
        #endregion

        #region POLÍTICAS DE ELEGIBILIDADE
        public List<SingleOneAPI.Models.ViewModels.PoliticaElegibilidadeVM> ListarPoliticasElegibilidade(int cliente, string tipoColaborador = null, int? tipoEquipamentoId = null)
        {
            try
            {
                var query = _politicaElegibilidadeRepository.Query()
                    .Where(x => x.Cliente == cliente && x.Ativo);

                // Filtro opcional por tipo de colaborador
                if (!string.IsNullOrEmpty(tipoColaborador))
                {
                    query = query.Where(x => x.TipoColaborador == tipoColaborador);
                }

                // Filtro opcional por tipo de equipamento
                if (tipoEquipamentoId.HasValue)
                {
                    query = query.Where(x => x.TipoEquipamentoId == tipoEquipamentoId.Value);
                }

                var politicas = query
                    .Include(x => x.ClienteNavigation)
                    .Include(x => x.TipoEquipamentoNavigation)
                    .Include(x => x.UsuarioCadastroNavigation)
                    .OrderBy(x => x.TipoColaborador)
                    .ThenBy(x => x.TipoEquipamentoNavigation.Descricao)
                    .ToList();

                // Mapear para ViewModel
                var resultado = politicas.Select(p => new SingleOneAPI.Models.ViewModels.PoliticaElegibilidadeVM
                {
                    Id = p.Id,
                    Cliente = p.Cliente,
                    ClienteNome = p.ClienteNavigation?.Razaosocial ?? "",
                    TipoColaborador = p.TipoColaborador,
                    TipoColaboradorDescricao = ObterDescricaoTipoColaborador(p.TipoColaborador),
                    Cargo = p.Cargo,
                    UsarPadrao = p.UsarPadrao,
                    TipoEquipamentoId = p.TipoEquipamentoId,
                    TipoEquipamentoDescricao = p.TipoEquipamentoNavigation?.Descricao ?? "",
                    PermiteAcesso = p.PermiteAcesso,
                    QuantidadeMaxima = p.QuantidadeMaxima,
                    Observacoes = p.Observacoes,
                    Ativo = p.Ativo,
                    DtCadastro = p.DtCadastro,
                    DtAtualizacao = p.DtAtualizacao,
                    UsuarioCadastro = p.UsuarioCadastro,
                    UsuarioCadastroNome = p.UsuarioCadastroNavigation?.Nome ?? ""
                }).ToList();

                Console.WriteLine($"[ELEGIBILIDADE] Listadas {resultado.Count} políticas para cliente {cliente}");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE] Erro ao listar políticas: {ex.Message}");
                throw;
            }
        }

        public SingleOneAPI.Models.ViewModels.PoliticaElegibilidadeVM BuscarPoliticaElegibilidadePorId(int id)
        {
            try
            {
                var politica = _politicaElegibilidadeRepository.Query()
                    .Include(x => x.ClienteNavigation)
                    .Include(x => x.TipoEquipamentoNavigation)
                    .Include(x => x.UsuarioCadastroNavigation)
                    .FirstOrDefault(x => x.Id == id);

                if (politica == null)
                {
                    Console.WriteLine($"[ELEGIBILIDADE] Política ID {id} não encontrada");
                    return null;
                }

                return new SingleOneAPI.Models.ViewModels.PoliticaElegibilidadeVM
                {
                    Id = politica.Id,
                    Cliente = politica.Cliente,
                    ClienteNome = politica.ClienteNavigation?.Razaosocial ?? "",
                    TipoColaborador = politica.TipoColaborador,
                    TipoColaboradorDescricao = ObterDescricaoTipoColaborador(politica.TipoColaborador),
                    Cargo = politica.Cargo,
                    UsarPadrao = politica.UsarPadrao,
                    TipoEquipamentoId = politica.TipoEquipamentoId,
                    TipoEquipamentoDescricao = politica.TipoEquipamentoNavigation?.Descricao ?? "",
                    PermiteAcesso = politica.PermiteAcesso,
                    QuantidadeMaxima = politica.QuantidadeMaxima,
                    Observacoes = politica.Observacoes,
                    Ativo = politica.Ativo,
                    DtCadastro = politica.DtCadastro,
                    DtAtualizacao = politica.DtAtualizacao,
                    UsuarioCadastro = politica.UsuarioCadastro,
                    UsuarioCadastroNome = politica.UsuarioCadastroNavigation?.Nome ?? ""
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE] Erro ao buscar política ID {id}: {ex.Message}");
                throw;
            }
        }

        public string SalvarPoliticaElegibilidade(SingleOneAPI.Models.PoliticaElegibilidade politica)
        {
            try
            {
                Console.WriteLine($"[ELEGIBILIDADE] Salvando política - Cliente: {politica.Cliente}, Tipo: {politica.TipoColaborador}, Equipamento: {politica.TipoEquipamentoId}");

                // Validações
                if (politica.Cliente <= 0)
                {
                    return JsonConvert.SerializeObject(new { Mensagem = "Cliente inválido", Status = "400" });
                }

                if (string.IsNullOrEmpty(politica.TipoColaborador))
                {
                    return JsonConvert.SerializeObject(new { Mensagem = "Tipo de colaborador é obrigatório", Status = "400" });
                }

                if (politica.TipoEquipamentoId <= 0)
                {
                    return JsonConvert.SerializeObject(new { Mensagem = "Tipo de equipamento é obrigatório", Status = "400" });
                }

                // Validação: Se não permite acesso, não deve ter quantidade máxima
                if (!politica.PermiteAcesso && politica.QuantidadeMaxima.HasValue)
                {
                    return JsonConvert.SerializeObject(new { Mensagem = "Quando não permite acesso, não deve especificar quantidade máxima", Status = "400" });
                }

                // Verificar duplicidade (mesma combinação cliente + tipo colaborador + cargo + tipo equipamento)
                // CARGO agora funciona como filtro parcial (LIKE), então verificamos se já existe uma política igual
                var duplicada = _politicaElegibilidadeRepository.Query()
                    .Where(x => x.Cliente == politica.Cliente
                             && x.TipoColaborador == politica.TipoColaborador
                             && x.TipoEquipamentoId == politica.TipoEquipamentoId
                             && x.Id != politica.Id
                             && x.Ativo)
                    .Where(x => x.Cargo == politica.Cargo) // Comparação exata para evitar duplicação
                    .Any();

                if (duplicada)
                {
                    return JsonConvert.SerializeObject(new { Mensagem = "Já existe uma política ativa para esta combinação de tipo de colaborador e tipo de equipamento", Status = "400.1" });
                }

                if (politica.Id == 0)
                {
                    // Inserir nova política
                    politica.DtCadastro = DateTime.Now;
                    politica.DtAtualizacao = DateTime.Now;
                    politica.Ativo = true;

                    _politicaElegibilidadeRepository.Adicionar(politica);

                    Console.WriteLine($"[ELEGIBILIDADE] ✅ Política criada com sucesso - ID: {politica.Id}");
                    return JsonConvert.SerializeObject(new { Mensagem = "Política de elegibilidade criada com sucesso!", Status = "200", Id = politica.Id });
                }
                else
                {
                    // Atualizar política existente
                    var politicaExistente = _politicaElegibilidadeRepository.ObterPorId(politica.Id);
                    if (politicaExistente == null)
                    {
                        return JsonConvert.SerializeObject(new { Mensagem = "Política não encontrada", Status = "404" });
                    }

                    politicaExistente.TipoColaborador = politica.TipoColaborador;
                    politicaExistente.Cargo = politica.Cargo;
                    politicaExistente.TipoEquipamentoId = politica.TipoEquipamentoId;
                    politicaExistente.PermiteAcesso = politica.PermiteAcesso;
                    politicaExistente.QuantidadeMaxima = politica.QuantidadeMaxima;
                    politicaExistente.Observacoes = politica.Observacoes;
                    politicaExistente.DtAtualizacao = DateTime.Now;

                    _politicaElegibilidadeRepository.Atualizar(politicaExistente);

                    Console.WriteLine($"[ELEGIBILIDADE] ✅ Política atualizada com sucesso - ID: {politica.Id}");
                    return JsonConvert.SerializeObject(new { Mensagem = "Política de elegibilidade atualizada com sucesso!", Status = "200", Id = politica.Id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE] ❌ Erro ao salvar política: {ex.Message}");
                Console.WriteLine($"[ELEGIBILIDADE] StackTrace: {ex.StackTrace}");
                return JsonConvert.SerializeObject(new { Mensagem = "Erro ao salvar política: " + ex.Message, Status = "500" });
            }
        }

        public string ExcluirPoliticaElegibilidade(int id)
        {
            try
            {
                Console.WriteLine($"[ELEGIBILIDADE] Excluindo política ID: {id}");

                var politica = _politicaElegibilidadeRepository.ObterPorId(id);
                if (politica == null)
                {
                    return JsonConvert.SerializeObject(new { Mensagem = "Política não encontrada", Status = "404" });
                }

                // Soft delete - apenas marcar como inativa
                politica.Ativo = false;
                politica.DtAtualizacao = DateTime.Now;

                _politicaElegibilidadeRepository.Atualizar(politica);

                Console.WriteLine($"[ELEGIBILIDADE] ✅ Política ID {id} marcada como inativa com sucesso");
                return JsonConvert.SerializeObject(new { Mensagem = "Política excluída com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE] ❌ Erro ao excluir política ID {id}: {ex.Message}");
                return JsonConvert.SerializeObject(new { Mensagem = "Erro ao excluir política: " + ex.Message, Status = "500" });
            }
        }

        public bool VerificarElegibilidade(int colaboradorId, int tipoEquipamentoId)
        {
            try
            {
                var colaborador = _colaboradorRepository.ObterPorId(colaboradorId);
                if (colaborador == null)
                {
                    Console.WriteLine($"[ELEGIBILIDADE] Colaborador ID {colaboradorId} não encontrado");
                    return true; // Por segurança, permite se não encontrar colaborador
                }

                // Buscar política aplicável
                // CARGO funciona com dois modos:
                // 1. UsarPadrao = true: se a política tem "Analista", aplica para cargos que CONTENHAM "Analista" (LIKE '%Analista%')
                // 2. UsarPadrao = false: match exato do cargo
                var politicas = _politicaElegibilidadeRepository.Query()
                    .Where(x => x.Cliente == colaborador.Cliente
                             && x.TipoColaborador == colaborador.Tipocolaborador.ToString()
                             && x.TipoEquipamentoId == tipoEquipamentoId
                             && x.Ativo)
                    .ToList();

                // Filtrar por cargo considerando o campo UsarPadrao
                var politica = politicas.FirstOrDefault(x => 
                {
                    // Se não há filtro de cargo na política, aplica a todos
                    if (string.IsNullOrEmpty(x.Cargo))
                        return true;
                    
                    // Se colaborador não tem cargo, não aplica política específica de cargo
                    if (string.IsNullOrEmpty(colaborador.Cargo))
                        return false;
                    
                    // Se UsarPadrao = true, usa LIKE (contém)
                    if (x.UsarPadrao)
                    {
                        return colaborador.Cargo.ToLower().Contains(x.Cargo.ToLower());
                    }
                    // Se UsarPadrao = false, usa match exato
                    else
                    {
                        return colaborador.Cargo.Equals(x.Cargo, StringComparison.OrdinalIgnoreCase);
                    }
                });

                if (politica == null)
                {
                    Console.WriteLine($"[ELEGIBILIDADE] Nenhuma política encontrada - permitindo acesso");
                    return true; // Se não há política, permite
                }

                // Verificar se permite acesso
                if (!politica.PermiteAcesso)
                {
                    Console.WriteLine($"[ELEGIBILIDADE] Política proíbe acesso - colaborador: {colaboradorId}, equipamento: {tipoEquipamentoId}");
                    return false;
                }

                // Verificar quantidade máxima
                if (politica.QuantidadeMaxima.HasValue)
                {
                    var quantidadeAtual = _equipamentohistoricoRepository.Query()
                        .Include(x => x.EquipamentoNavigation)
                        .Where(x => x.Colaborador == colaboradorId
                                 && x.EquipamentoNavigation.Tipoequipamento == tipoEquipamentoId
                                 && x.EquipamentoNavigation.Ativo
                                 && x.Equipamentostatus == 4) // Status 4 = Entregue
                        .Count();

                    if (quantidadeAtual >= politica.QuantidadeMaxima.Value)
                    {
                        Console.WriteLine($"[ELEGIBILIDADE] Quantidade máxima atingida - colaborador: {colaboradorId}, atual: {quantidadeAtual}, máximo: {politica.QuantidadeMaxima.Value}");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE] Erro ao verificar elegibilidade: {ex.Message}");
                return true; // Em caso de erro, permite por segurança
            }
        }

        private string ObterDescricaoTipoColaborador(string tipo)
        {
            return tipo switch
            {
                "F" => "Funcionário",
                "T" => "Terceirizado",
                "C" => "Consultor",
                _ => tipo
            };
        }

        public List<dynamic> ListarTiposColaboradorDistintos()
        {
            // Retornar os 3 tipos padrao do sistema
            // F = Funcionario, T = Terceirizado, C = Consultor
            var tiposPadrao = new List<dynamic>
            {
                new { Codigo = "F", Descricao = "Funcion\u00E1rio" },
                new { Codigo = "T", Descricao = "Terceirizado" },
                new { Codigo = "C", Descricao = "Consultor" }
            };

            Console.WriteLine($"[ELEGIBILIDADE] Retornando {tiposPadrao.Count} tipos padrao de colaboradores");
            foreach (var tipo in tiposPadrao)
            {
                Console.WriteLine($"  - Codigo: {tipo.Codigo}, Descricao: {tipo.Descricao}");
            }

            return tiposPadrao;
        }
        #endregion
    }
}
