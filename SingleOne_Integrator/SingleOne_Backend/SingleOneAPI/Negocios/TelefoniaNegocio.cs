using Microsoft.EntityFrameworkCore;
using SingleOne.Models;
using SingleOne.Util;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleOne.Negocios
{
    public class TelefoniaNegocio : ITelefoniaNegocio
    {
        private readonly IRepository<Telefoniacontrato> _telefoniacontratoRepository;
        private readonly IRepository<Telefonialinha> _telefonialinhaRepository;
        private readonly IRepository<Telefoniaoperadora> _telefoniaoperadoraRepository;
        private readonly IRepository<Telefoniaplano> _telefoniaplanoRepository;
        private readonly IReadOnlyRepository<Vwtelefonium> _readOnlyRepository;
        private readonly IReadOnlyRepository<PlanosVM> _planosVMRepository;
        private readonly IRepository<Requisicoesiten> _requisicaoItensRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        public TelefoniaNegocio(
                                IRepository<Telefoniacontrato> telefoniacontratoRepository,
                                IRepository<Telefonialinha> telefonialinhaRepository,
                                IRepository<Telefoniaoperadora> telefoniaoperadoraRepository,
                                IRepository<Telefoniaplano> telefoniaplanoRepository,
                                IReadOnlyRepository<Vwtelefonium> readOnlyRepository,
                                IReadOnlyRepository<PlanosVM> planosVMRepository,
                                IRepository<Requisicoesiten> requisicaoItensRepository,
                                IRepository<Colaboradore> colaboradorRepository)
        {
            _telefoniacontratoRepository = telefoniacontratoRepository;
            _telefonialinhaRepository = telefonialinhaRepository;
            _telefoniaoperadoraRepository = telefoniaoperadoraRepository;
            _telefoniaplanoRepository = telefoniaplanoRepository;
            _readOnlyRepository = readOnlyRepository;
            _planosVMRepository = planosVMRepository;
            _requisicaoItensRepository = requisicaoItensRepository;
            _colaboradorRepository = colaboradorRepository;
        }

        /***************************************************************************************************/
        /******************************************** OPERADORAS *******************************************/
        /***************************************************************************************************/
        public List<Telefoniaoperadora> ListarOperadoras()
        {
            // Simplificar para debug - listar TODAS as operadoras
            var operadoras = _telefoniaoperadoraRepository
                            .IncludeWithThenInclude(q => q.Include(x => x.Telefoniacontratos)
                                .ThenInclude(x => x.Telefoniaplanos)
                                    .ThenInclude(x => x.Telefonialinhas))
                            .ToList(); // ❌ REMOVER FILTRO TEMPORARIAMENTE
            return operadoras;
        }
        public Telefoniaoperadora SalvarOperadora(Telefoniaoperadora to)
        {
            try
            {
                if(to.Id == 0)
                {
                    to.Ativo = true;
                    _telefoniaoperadoraRepository.Adicionar(to);
                    // ✅ IMPORTANTE: Salvar alterações no banco
                    _telefoniaoperadoraRepository.SalvarAlteracoes();
                    return to; // Retorna a operadora com ID gerado
                }
                else
                {
                    // ✅ IMPORTANTE: Preservar o status Ativo da operadora existente
                    var operadoraExistente = _telefoniaoperadoraRepository.ObterPorId(to.Id);
                    if (operadoraExistente != null)
                    {
                        to.Ativo = operadoraExistente.Ativo; // Preservar status Ativo
                    }
                    
                    _telefoniaoperadoraRepository.Atualizar(to);
                    // ✅ IMPORTANTE: Salvar alterações no banco
                    _telefoniaoperadoraRepository.SalvarAlteracoes();
                    return to; // Retorna a operadora atualizada
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExcluirOperadora(int id)
        {
            var op = _telefoniaoperadoraRepository.ObterPorId(id);
            try
            {
                //db.Remove(op);
                //db.SaveChanges();
                _telefoniaoperadoraRepository.Remover(op);
            }
            catch
            {
                op.Ativo = false;
                //db.Update(op);
                //db.SaveChanges();
                _telefoniaoperadoraRepository.Atualizar(op);
            }
        }

        /***************************************************************************************************/
        /******************************************** CONTRATOS ********************************************/
        /***************************************************************************************************/
        public List<Telefoniacontrato> ListarContratos(string pesquisa, int operadora, int cliente)
        {
            // Se não há pesquisa específica, retornar apenas contadores para o dashboard
            if (string.IsNullOrEmpty(pesquisa) && operadora == 0 && cliente == 0)
            {
                var listaContratos = _telefoniacontratoRepository
                    .Buscar(x => x.Ativo)
                    .ToList();
                return listaContratos;
            }
            
            // Pesquisa específica com includes
            pesquisa = pesquisa.ToLower();
            var listaContratosCompleta = _telefoniacontratoRepository
                .IncludeWithThenInclude(q => q.Include(x => x.Telefoniaplanos)
                    .ThenInclude(x => x.Telefonialinhas))
                .Include(x => x.OperadoraNavigation)
                .Where(x => x.Ativo && 
                    ((cliente != 0) ? x.Cliente == cliente : 1 == 1) &&
                    ((pesquisa != "null") ? 
                        x.OperadoraNavigation.Nome.ToLower().Contains(pesquisa) ||
                        x.Nome.ToLower().Contains(pesquisa) : 1 == 1) &&
                    ((operadora != 0) ? x.Operadora == operadora : 1 == 1)
                 ).ToList();
            return listaContratosCompleta;
        }
        public void SalvarContrato(Telefoniacontrato tc)
        {
            try
            {
                if(tc.Id == 0)
                {
                    tc.Ativo = true;
                    //db.Add(tc);
                    _telefoniacontratoRepository.Adicionar(tc);
                }
                else
                {
                    //db.Update(tc);
                    _telefoniacontratoRepository.Atualizar(tc);
                }
                // ✅ IMPORTANTE: Salvar alterações no banco
                _telefoniacontratoRepository.SalvarAlteracoes();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExcluirContrato(int id)
        {
            var tc = _telefoniacontratoRepository.ObterPorId(id);
            try
            {
                //db.Remove(tc);
                //db.SaveChanges();
                _telefoniacontratoRepository.Remover(tc);
            }
            catch
            {
                tc.Ativo = false;
                //db.Update(tc);
                //db.SaveChanges();
                _telefoniacontratoRepository.Atualizar(tc);
            }
        }


        /***************************************************************************************************/
        /******************************************** PLANOS ***********************************************/
        /***************************************************************************************************/
        public List<PlanosVM> ListarPlanos(string pesquisa, int contrato, int cliente)
        {
            Console.WriteLine($"[DEBUG] ListarPlanos chamado - pesquisa: '{pesquisa}', contrato: {contrato}, cliente: {cliente}");
            
            try
            {
                pesquisa = pesquisa.ToLower();
                var planos = _planosVMRepository.Buscar(x => x.Ativo &&
                ((pesquisa != "null") ? 
                    x.Contrato.ToLower().Contains(pesquisa) ||
                    x.Operadora.ToLower().Contains(pesquisa) ||
                    x.Plano.ToLower().Contains(pesquisa) ||
                    x.Valor.ToString().Contains(pesquisa)
                : 1 == 1) &&
                ((contrato != 0) ? x.ContratoId == contrato : 1 == 1)).ToList();
                
                Console.WriteLine($"[DEBUG] Planos encontrados: {planos.Count}");
                if (planos.Count > 0)
                {
                    Console.WriteLine($"[DEBUG] Primeiro plano: {planos[0].Plano} - {planos[0].Operadora}");
                }
                
                return planos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao buscar planos: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return new List<PlanosVM>();
            }
        }
        
        // 🆕 MÉTODO SIMPLES PARA LISTAR TODOS OS PLANOS
        public List<PlanosVM> ListarTodosPlanos()
        {
            Console.WriteLine($"[DEBUG] ListarTodosPlanos chamado");
            
            try
            {
                var planos = _planosVMRepository.Buscar(x => x.Ativo).ToList();
                
                Console.WriteLine($"[DEBUG] Total de planos encontrados: {planos.Count}");
                if (planos.Count > 0)
                {
                    Console.WriteLine($"[DEBUG] Primeiro plano: {planos[0].Plano} - {planos[0].Operadora}");
                }
                
                return planos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao buscar todos os planos: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return new List<PlanosVM>();
            }
        }
        public void SalvarPlano(PlanosVM tp)
        {
            try
            {
                if(tp.Id == 0)
                {
                    var plano = new Telefoniaplano
                    {
                        Ativo = true,
                        Contrato = tp.ContratoId,
                        Nome = tp.Plano,
                        Valor = tp.Valor
                    };
                    _telefoniaplanoRepository.Adicionar(plano);
                }
                else
                {
                    var planoDb = _telefoniaplanoRepository.ObterPorId(tp.Id);
                    if (planoDb == null) 
                        throw new EntidadeNaoEncontradaEx("Plano não encontrado.");

                    planoDb.Ativo = tp.Ativo;
                    planoDb.Valor = tp.Valor;
                    planoDb.Contrato = tp.ContratoId;
                    planoDb.Nome = tp.Plano;

                    _telefoniaplanoRepository.Atualizar(planoDb);
                }
                // ✅ IMPORTANTE: Salvar alterações no banco
                _telefoniaplanoRepository.SalvarAlteracoes();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExcluirPlano(int id)
        {
            var tp = _telefoniaplanoRepository.ObterPorId(id);
            try
            {
                //db.Remove(tp);
                //db.SaveChanges();
                _telefoniaplanoRepository.Remover(tp);
            }
            catch 
            {
                tp.Ativo = false;
                //db.Update(tp);
                //db.SaveChanges();
                _telefoniaplanoRepository.Atualizar(tp);
            }
        }


        /***************************************************************************************************/
        /******************************************** LINHAS ***********************************************/
        /***************************************************************************************************/
        public List<Telefonialinha> ListarLinhas(string pesquisa, int cliente, int pagina)
        {
            pesquisa = pesquisa.ToLower();
            var linhas = _telefonialinhaRepository
                            .IncludeWithThenInclude(q => q.Include(x => x.PlanoNavigation)
                                .ThenInclude(x => x.ContratoNavigation)
                                    .ThenInclude(x => x.OperadoraNavigation))
                            .Where(x => x.Ativo && x.PlanoNavigation.ContratoNavigation.Cliente == cliente
                               && ((pesquisa != "null") ?
                                    x.Numero.ToString().Contains(pesquisa) ||
                                    x.PlanoNavigation.Nome.ToLower().Contains(pesquisa) ||
                                    x.PlanoNavigation.ContratoNavigation.Nome.ToLower().Contains(pesquisa) ||
                                    x.PlanoNavigation.ContratoNavigation.OperadoraNavigation.Nome.ToLower().Contains(pesquisa)
                              : 1 == 1))
                            .ToList(); // ✅ RETORNAR TODAS AS LINHAS (paginação no frontend)

            return linhas;
        }

        // 🆕 NOVOS MÉTODOS PARA FILTROS ESPECÍFICOS
        public PagedResult<Telefonialinha> ListarLinhasPorConta(int contaId, int cliente, int pagina)
        {
            var linhas = _telefonialinhaRepository
                            .IncludeWithThenInclude(q => q.Include(x => x.PlanoNavigation)
                                .ThenInclude(x => x.ContratoNavigation)
                                    .ThenInclude(x => x.OperadoraNavigation))
                            .Where(x => x.Ativo && 
                                       x.PlanoNavigation.ContratoNavigation.Id == contaId &&
                                       x.PlanoNavigation.ContratoNavigation.Cliente == cliente)
                            .GetPaged(pagina, 10);

            return linhas;
        }

        public PagedResult<Telefonialinha> ListarLinhasPorPlano(int planoId, int cliente, int pagina)
        {
            var linhas = _telefonialinhaRepository
                            .IncludeWithThenInclude(q => q.Include(x => x.PlanoNavigation)
                                .ThenInclude(x => x.ContratoNavigation)
                                    .ThenInclude(x => x.OperadoraNavigation))
                            .Where(x => x.Ativo && 
                                       x.PlanoNavigation.Id == planoId &&
                                       x.PlanoNavigation.ContratoNavigation.Cliente == cliente)
                            .GetPaged(pagina, 10);

            return linhas;
        }

        public PagedResult<Telefonialinha> ListarLinhasPorTipo(int contaId, string tipo, int cliente, int pagina)
        {
            var query = _telefonialinhaRepository
                            .IncludeWithThenInclude(q => q.Include(x => x.PlanoNavigation)
                                .ThenInclude(x => x.ContratoNavigation)
                                    .ThenInclude(x => x.OperadoraNavigation))
                            .Where(x => x.Ativo && 
                                       x.PlanoNavigation.ContratoNavigation.Id == contaId &&
                                       x.PlanoNavigation.ContratoNavigation.Cliente == cliente);

            // Aplicar filtro por tipo
            switch (tipo.ToLower())
            {
                case "em-uso":
                    query = query.Where(x => x.Emuso == true);
                    break;
                case "livres":
                    query = query.Where(x => x.Emuso == false);
                    break;
                case "todas":
                default:
                    // Não aplicar filtro adicional
                    break;
            }

            return query.GetPaged(pagina, 10);
        }

        public PagedResult<Telefonialinha> ListarLinhasPorPlanoETipo(int planoId, string tipo, int cliente, int pagina)
        {
            var query = _telefonialinhaRepository
                            .IncludeWithThenInclude(q => q.Include(x => x.PlanoNavigation)
                                .ThenInclude(x => x.ContratoNavigation)
                                    .ThenInclude(x => x.OperadoraNavigation))
                            .Where(x => x.Ativo && 
                                       x.PlanoNavigation.Id == planoId &&
                                       x.PlanoNavigation.ContratoNavigation.Cliente == cliente);

            // Aplicar filtro por tipo
            switch (tipo.ToLower())
            {
                case "em-uso":
                    query = query.Where(x => x.Emuso == true);
                    break;
                case "livres":
                    query = query.Where(x => x.Emuso == false);
                    break;
                case "todas":
                default:
                    // Não aplicar filtro adicional
                    break;
            }

            return query.GetPaged(pagina, 10);
        }
        
        public List<Telefonialinha> LinhasDisponiveisParaRequisicao(string pesquisa, int cliente)
        {
            var linha = _telefonialinhaRepository
                        .IncludeWithThenInclude(q => q.Include(x => x.PlanoNavigation)
                            .ThenInclude(x => x.ContratoNavigation)
                                .ThenInclude(x => x.OperadoraNavigation))
                        .Where(x => x.Emuso == false && x.PlanoNavigation.ContratoNavigation.Cliente == cliente &&
                        ((pesquisa) != "null" ? x.Numero.ToString().Contains(pesquisa) : 1 == 1))
                        .ToList();
            return linha;
        }

        // 🆕 NOVO: Método para exportação com todos os dados de relacionamento
        // PROCESSO REVERSO: Parte das REQUISIÇÕES → USUÁRIOS → COLABORADORES → LINHAS
        public List<dynamic> ListarLinhasParaExportacao(string pesquisa, int cliente)
        {
            pesquisa = pesquisa?.ToLower() ?? "null";
            
            Console.WriteLine($"[TELEFONIA] === INÍCIO ListarLinhasParaExportacao (PROCESSO REVERSO) ===");
            Console.WriteLine($"[TELEFONIA] Pesquisa: {pesquisa}, Cliente: {cliente}");
            
            // PASSO 1: Buscar TODAS as requisições ativas (entregues e não devolvidas) que têm LINHA
            var requisicaoItens = _requisicaoItensRepository
                                    .Buscar(x => x.Linhatelefonica.HasValue &&
                                                x.Dtentrega != null &&
                                                x.Dtdevolucao == null)
                                    .Include(x => x.LinhatelefonicaNavigation)
                                        .ThenInclude(x => x.PlanoNavigation)
                                            .ThenInclude(x => x.ContratoNavigation)
                                                .ThenInclude(x => x.OperadoraNavigation)
                                    .Include(x => x.EquipamentoNavigation)
                                        .ThenInclude(x => x.TipoequipamentoNavigation)
                                    .Include(x => x.EquipamentoNavigation)
                                        .ThenInclude(x => x.FabricanteNavigation)
                                    .Include(x => x.EquipamentoNavigation)
                                        .ThenInclude(x => x.ModeloNavigation)
                                    .ToList();
            
            // Filtrar apenas requisições do cliente especificado
            requisicaoItens = requisicaoItens
                                .Where(ri => ri.LinhatelefonicaNavigation != null && 
                                            ri.LinhatelefonicaNavigation.PlanoNavigation?.ContratoNavigation?.Cliente == cliente)
                                .ToList();
            
            Console.WriteLine($"[TELEFONIA] Total de requisições ativas com linha do cliente {cliente}: {requisicaoItens.Count}");
            
            // PASSO 2: Buscar todos os IDs de usuários das requisições
            var usuariosIds = requisicaoItens
                                .Where(ri => ri.Usuarioentrega.HasValue)
                                .Select(ri => ri.Usuarioentrega.Value)
                                .Distinct()
                                .ToList();
            
            Console.WriteLine($"[TELEFONIA] Total de IDs de usuários encontrados: {usuariosIds.Count}");
            if (usuariosIds.Any())
            {
                Console.WriteLine($"[TELEFONIA] IDs de usuários: {string.Join(", ", usuariosIds)}");
            }
            
            // PASSO 3: Buscar TODOS os colaboradores com seus relacionamentos
            var colaboradores = new List<Colaboradore>();
            if (usuariosIds.Any())
            {
                colaboradores = _colaboradorRepository
                                        .Buscar(x => usuariosIds.Contains(x.Usuario))
                                        .Include(x => x.EmpresaNavigation)
                                        .Include(x => x.CentrocustoNavigation)
                                        .Include(x => x.LocalidadeNavigation)
                                        .ToList();
                
                Console.WriteLine($"[TELEFONIA] Total de colaboradores encontrados: {colaboradores.Count}");
                foreach (var col in colaboradores)
                {
                    Console.WriteLine($"[TELEFONIA]   -> Colaborador: {col.Nome} (Usuario ID: {col.Usuario}, CPF: {col.Cpf}, Empresa: {col.EmpresaNavigation?.Nome})");
                }
            }
            
            // PASSO 4: Montar o resultado combinando Requisição + Linha + Colaborador
            var resultado = requisicaoItens.Select(item =>
            {
                var linha = item.LinhatelefonicaNavigation;
                var equipamento = item.EquipamentoNavigation;
                
                // Buscar colaborador pelo usuário que recebeu a entrega
                Colaboradore colaborador = null;
                if (item.Usuarioentrega.HasValue)
                {
                    colaborador = colaboradores.FirstOrDefault(c => c.Usuario == item.Usuarioentrega.Value);
                }

                // Criar objeto de colaborador se disponível (descriptografar CPF e Email)
                var dadosColaborador = colaborador != null ? new
                {
                    nome = colaborador.Nome,
                    cpf = Cripto.CriptografarDescriptografar(colaborador.Cpf, false), // Descriptografar CPF
                    matricula = colaborador.Matricula,
                    email = Cripto.CriptografarDescriptografar(colaborador.Email, false), // Descriptografar Email
                    telefone = (string)null,
                    cargo = colaborador.Cargo,
                    departamento = colaborador.Setor,
                    ativo = colaborador.Situacao == "A",
                    empresa = colaborador.EmpresaNavigation != null ? new { nome = colaborador.EmpresaNavigation.Nome } : null,
                    centrocusto = colaborador.CentrocustoNavigation != null ? new { nome = colaborador.CentrocustoNavigation.Nome } : null,
                    localidade = colaborador.LocalidadeNavigation != null ? new { descricao = colaborador.LocalidadeNavigation.Descricao } : null
                } : null;
                
                // Criar objeto de recurso (sempre inclui colaborador quando disponível)
                var dadosRecurso = equipamento != null || colaborador != null ? new
                {
                    id = equipamento?.Id,
                    numeroserie = equipamento?.Numeroserie,
                    patrimonio = equipamento?.Patrimonio,
                    tipoequipamento = equipamento?.TipoequipamentoNavigation?.Descricao,
                    fabricante = equipamento?.FabricanteNavigation?.Descricao,
                    modelo = equipamento?.ModeloNavigation?.Descricao,
                    usuario = dadosColaborador
                } : null;

                return new
                {
                    // Dados da linha
                    id = linha.Id,
                    numero = linha.Numero,
                    iccid = linha.Iccid,
                    emuso = linha.Emuso,
                    createdAt = (DateTime?)null,
                    
                    // Relacionamentos da linha
                    planoNavigation = linha.PlanoNavigation != null ? new
                    {
                        nome = linha.PlanoNavigation.Nome,
                        plano = linha.PlanoNavigation.Nome,
                        contratoNavigation = linha.PlanoNavigation.ContratoNavigation != null ? new
                        {
                            nome = linha.PlanoNavigation.ContratoNavigation.Nome,
                            operadoraNavigation = linha.PlanoNavigation.ContratoNavigation.OperadoraNavigation != null ? new
                            {
                                nome = linha.PlanoNavigation.ContratoNavigation.OperadoraNavigation.Nome
                            } : null
                        } : null
                    } : null,
                    
                    // Dados do recurso (equipamento) + colaborador
                    recurso = dadosRecurso
                };
            }).ToList<dynamic>();

            Console.WriteLine($"[TELEFONIA] Total de resultados retornados: {resultado.Count}");
            var comColaborador = resultado.Count(r => ((dynamic)r).recurso != null && ((dynamic)r).recurso.usuario != null);
            Console.WriteLine($"[TELEFONIA] Resultados com colaborador: {comColaborador}");
            Console.WriteLine($"[TELEFONIA] === FIM ListarLinhasParaExportacao ===");

            return resultado;
        }
        public void SalvarLinha(Telefonialinha tl)
        {
            try
            {
                if(tl.Id == 0)
                {
                    bool existe = _telefonialinhaRepository.Buscar(x => x.Numero == tl.Numero).Any();
                    if (existe) 
                        throw new EntidadeJaExisteEx("O Número informado já existe em outra linha. Favor utilizá-lo.");
                    tl.Ativo = true;
                    //db.Add(tl);
                    _telefonialinhaRepository.Adicionar(tl);
                }
                else
                {
                    //db.Update(tl);
                    _telefonialinhaRepository.Atualizar(tl);
                }
                // ✅ IMPORTANTE: Salvar alterações no banco
                _telefonialinhaRepository.SalvarAlteracoes();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Telefonialinha BuscarLinhaPorId(int id)
        {
            return _telefonialinhaRepository
                .IncludeWithThenInclude(q => q.Include(x => x.PlanoNavigation)
                    .ThenInclude(x => x.ContratoNavigation)
                        .ThenInclude(x => x.OperadoraNavigation))
                .Where(x => x.Id == id)
                .FirstOrDefault();
        }
        
        public void ExcluirLinha(int id)
        {
            var tl = _telefonialinhaRepository.ObterPorId(id);
            try
            {
                //db.Remove(tl);
                //db.SaveChanges();
                _telefonialinhaRepository.Remover(tl);
            }
            catch
            {
                tl.Ativo = false;
                //db.Update(tl);
                //db.SaveChanges();
                _telefonialinhaRepository.Atualizar(tl);
            }
        }


        public List<Vwtelefonium> ExportarParaExcel(int cliente)
        {
            var dados = _readOnlyRepository.Buscar(x => x.Cliente == cliente).ToList();
            return dados;
        }

        /***************************************************************************************************/
        /****************************************** CONTADORES *********************************************/
        /***************************************************************************************************/
        public int ContarOperadoras()
        {
            return _telefoniaoperadoraRepository.Buscar(x => x.Ativo).Count();
        }

        public int ContarContratos()
        {
            return _telefoniacontratoRepository.Buscar(x => x.Ativo).Count();
        }

        public int ContarPlanos()
        {
            return _telefoniaplanoRepository.Buscar(x => x.Ativo).Count();
        }

        public int ContarLinhas()
        {
            return _telefonialinhaRepository.Buscar(x => x.Ativo).Count();
        }
    }
}
