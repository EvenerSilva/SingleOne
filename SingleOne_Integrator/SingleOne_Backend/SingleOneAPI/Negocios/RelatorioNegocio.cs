using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOne.Util;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Models;
using SingleOneAPI.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SingleOne.Negocios
{
    public class RelatorioNegocio : IRelatorioNegocio
    {
        private readonly IRepository<Laudo> _laudoRepository;
        private readonly IRepository<Requisico> _requisicaoRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IReadOnlyRepository<Vwlaudo> _vwLaudoRepository;
        private readonly IReadOnlyRepository<Equipamentohistoricovm> _equipamentohistoricovmRepository;
        private readonly IRepository<Equipamentohistorico> _equipamentoHistoricoRepository;
        private readonly IReadOnlyRepository<Requisicoesvm> _requisicoesvmRepository;
        private readonly IReadOnlyRepository<Colaboradorhistoricovm> _colaboradorhistoricovmsRepository;
        private readonly IReadOnlyRepository<Vwequipamentosdetalhe> _vwequipamentosdetalhesRepository;
        private readonly IReadOnlyRepository<Vwdevolucaoprogramadum> _vwdevolucaoprogramadumRepository;
        private readonly IReadOnlyRepository<Vwequipamentoscomcolaboradoresdesligado> _equipamentosComColaboradorDesligadoRepository;
        private readonly IReadOnlyRepository<Vwequipamentosstatus> _equipamentosPorStatusRepository;
        private readonly IReadOnlyRepository<Requisicaoequipamentosvm> _requisicaoequipamentosvmsRepository;
        private readonly IRepository<Requisicoesiten> _requisicaoItensRepository;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<PatrimonioLogAcesso> _patrimonioLogAcessoRepository;
        private readonly IRepository<PatrimonioContestacao> _contestacaoRepository;
        private readonly IRepository<SinalizacaoSuspeita> _sinalizacaoRepository;
        private readonly IRepository<CampanhaAssinatura> _campanhaRepository;
        private readonly IRepository<Contrato> _contratoRepository;
        private readonly SingleOneDbContext _context;

        public RelatorioNegocio(IRepository<Laudo> laudoRepository,
            IRepository<Requisico> requisicaoRepository,
            IRepository<Equipamento> equipamentoRepository,
            IReadOnlyRepository<Vwlaudo> vwLaudoRepository,
            IReadOnlyRepository<Equipamentohistoricovm> equipamentohistoricovmRepository,
            IRepository<Equipamentohistorico> equipamentoHistoricoRepository,
            IReadOnlyRepository<Requisicoesvm> requisicoesvmRepository,
            IReadOnlyRepository<Colaboradorhistoricovm> colaboradorhistoricovmsRepository,
            IReadOnlyRepository<Vwequipamentosdetalhe> vwequipamentosdetalhesRepository,
            IReadOnlyRepository<Vwdevolucaoprogramadum> vwdevolucaoprogramadumRepository,
            IReadOnlyRepository<Vwequipamentoscomcolaboradoresdesligado> equipamentosComColaboradorDesligadoRepository,
            IReadOnlyRepository<Vwequipamentosstatus> equipamentosPorStatusRepository,
            IReadOnlyRepository<Requisicaoequipamentosvm> requisicaoequipamentosvmsRepository,
            IRepository<Requisicoesiten> requisicaoItensRepository,
            IRepository<Usuario> usuarioRepository,
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<PatrimonioLogAcesso> patrimonioLogAcessoRepository,
            IRepository<PatrimonioContestacao> contestacaoRepository,
            IRepository<SinalizacaoSuspeita> sinalizacaoRepository,
            IRepository<CampanhaAssinatura> campanhaRepository,
            IRepository<Contrato> contratoRepository,
            SingleOneDbContext context
            )
        {
            _requisicoesvmRepository = requisicoesvmRepository;
            _laudoRepository = laudoRepository;
            _requisicaoRepository = requisicaoRepository;
            _equipamentoRepository = equipamentoRepository;
            _vwLaudoRepository = vwLaudoRepository;
            _equipamentohistoricovmRepository = equipamentohistoricovmRepository;
            _equipamentoHistoricoRepository = equipamentoHistoricoRepository;
            _requisicoesvmRepository = requisicoesvmRepository;
            _colaboradorhistoricovmsRepository = colaboradorhistoricovmsRepository;
            _vwequipamentosdetalhesRepository = vwequipamentosdetalhesRepository;
            _vwdevolucaoprogramadumRepository = vwdevolucaoprogramadumRepository;
            _equipamentosComColaboradorDesligadoRepository = equipamentosComColaboradorDesligadoRepository;
            _equipamentosPorStatusRepository = equipamentosPorStatusRepository;
            _requisicaoequipamentosvmsRepository = requisicaoequipamentosvmsRepository;
            _requisicaoItensRepository = requisicaoItensRepository;
            _usuarioRepository = usuarioRepository;
            _colaboradorRepository = colaboradorRepository;
            _patrimonioLogAcessoRepository = patrimonioLogAcessoRepository;
            _contestacaoRepository = contestacaoRepository;
            _sinalizacaoRepository = sinalizacaoRepository;
            _campanhaRepository = campanhaRepository;
            _contratoRepository = contratoRepository;
            _context = context;

        }

        public List<Equipamentohistoricovm> HistoricoEquipamento(int id)
        {
            // ✅ CORREÇÃO: Primeiro verificar se é uma linha telefônica
            // Buscar se existe algum Requisicoesiten com este ID como Linhatelefonica
            var linhaExiste = _requisicaoItensRepository.Buscar(x => x.Linhatelefonica == id).Any();
            
            if (linhaExiste)
            {
                // ✅ É uma linha telefônica - buscar histórico via Requisicoesiten
                var linhasHist = _requisicaoItensRepository.Buscar(x => x.Linhatelefonica == id)
                    .Include(x => x.LinhatelefonicaNavigation)
                    .ThenInclude(x => x.PlanoNavigation)
                    .ThenInclude(x => x.ContratoNavigation)
                    .ThenInclude(x => x.OperadoraNavigation)
                    .Include(x => x.RequisicaoNavigation)
                    .OrderByDescending(x => x.Dtentrega)
                    .ToList();

                // ✅ Buscar nomes dos usuários via tabela Usuario
                // Incluir usuários da entrega e da requisição principal
                var usuarioIds = linhasHist.Where(x => x.Usuarioentrega.HasValue).Select(x => x.Usuarioentrega.Value)
                    .Concat(linhasHist.Where(x => x.RequisicaoNavigation != null).Select(x => x.RequisicaoNavigation.Usuariorequisicao))
                    .Concat(linhasHist.Where(x => x.RequisicaoNavigation != null).Select(x => x.RequisicaoNavigation.Tecnicoresponsavel))
                    .Distinct().ToList();
                var usuarios = new Dictionary<int, string>();
                if (usuarioIds.Any())
                {
                    var usuariosEncontrados = _usuarioRepository.Buscar(x => usuarioIds.Contains(x.Id))
                        .Select(x => new { x.Id, x.Nome })
                        .ToList();
                    usuarios = usuariosEncontrados.ToDictionary(x => x.Id, x => x.Nome);
                }

                // ✅ Buscar nomes dos colaboradores via tabela Colaboradore
                // Para linhas telefônicas, buscar o colaborador na requisição principal
                var colaboradorIds = linhasHist.Where(x => x.RequisicaoNavigation?.Colaboradorfinal.HasValue == true)
                    .Select(x => x.RequisicaoNavigation.Colaboradorfinal.Value).Distinct().ToList();
                var colaboradores = new Dictionary<int, string>();
                if (colaboradorIds.Any())
                {
                    var colaboradoresEncontrados = _colaboradorRepository.Buscar(x => colaboradorIds.Contains(x.Id))
                        .Select(x => new { x.Id, x.Nome })
                        .ToList();
                    colaboradores = colaboradoresEncontrados.ToDictionary(x => x.Id, x => x.Nome);
                }

                // ✅ Converter linhas telefônicas para Equipamentohistoricovm
                var linhasVM = linhasHist.Select(lt => {
                    // Tentar primeiro o usuário da entrega, depois da requisição
                    var nomeUsuario = "Sistema";
                    if (lt.Usuarioentrega.HasValue && usuarios.ContainsKey(lt.Usuarioentrega.Value))
                    {
                        nomeUsuario = usuarios[lt.Usuarioentrega.Value];
                    }
                    else if (lt.RequisicaoNavigation != null && usuarios.ContainsKey(lt.RequisicaoNavigation.Usuariorequisicao))
                    {
                        nomeUsuario = usuarios[lt.RequisicaoNavigation.Usuariorequisicao];
                    }
                    else if (lt.RequisicaoNavigation != null && usuarios.ContainsKey(lt.RequisicaoNavigation.Tecnicoresponsavel))
                    {
                        nomeUsuario = usuarios[lt.RequisicaoNavigation.Tecnicoresponsavel];
                    }
                    
                    // Para linhas telefônicas, usar o colaborador da requisição principal
                    // Se Colaboradorfinal estiver NULL, usar Tecnicoresponsavel como fallback
                    var nomeColaborador = "Sistema";
                    if (lt.RequisicaoNavigation?.Colaboradorfinal.HasValue == true && colaboradores.ContainsKey(lt.RequisicaoNavigation.Colaboradorfinal.Value))
                    {
                        nomeColaborador = colaboradores[lt.RequisicaoNavigation.Colaboradorfinal.Value];
                    }
                    else if (lt.RequisicaoNavigation != null && usuarios.ContainsKey(lt.RequisicaoNavigation.Tecnicoresponsavel))
                    {
                        // Usar o nome do técnico responsável como colaborador
                        nomeColaborador = usuarios[lt.RequisicaoNavigation.Tecnicoresponsavel];
                    }
                    
                    return new Equipamentohistoricovm
                    {
                        Id = lt.Id,
                        Equipamentoid = lt.Linhatelefonica,
                        Tipoequipamento = "Linha Telefônica",
                        Fabricante = lt.LinhatelefonicaNavigation?.PlanoNavigation?.ContratoNavigation?.OperadoraNavigation?.Nome ?? "N/A",
                        Modelo = lt.LinhatelefonicaNavigation?.Numero.ToString() ?? "N/A",
                        Numeroserie = lt.LinhatelefonicaNavigation?.Numero.ToString(),
                        Patrimonio = lt.LinhatelefonicaNavigation?.PlanoNavigation?.ContratoNavigation?.OperadoraNavigation?.Nome ?? "N/A",
                        Equipamentostatusid = lt.Dtdevolucao.HasValue ? 5 : 4, // 4 = Entregue, 5 = Devolvido
                        Equipamentostatus = lt.Dtdevolucao.HasValue ? "Devolvido" : "Entregue",
                        Colaboradorid = lt.Usuarioentrega,
                        Colaborador = nomeColaborador,
                        Dtregistro = lt.Dtentrega ?? DateTime.Now,
                        Usuarioid = lt.Usuarioentrega,
                        Usuario = nomeUsuario,
                        Tecnicoresponsavelid = lt.Usuariodevolucao,
                        Tecnicoresponsavel = nomeUsuario // Usar o mesmo usuário para responsável provisório
                    };
                }).ToList();
                
                return linhasVM;
            }
            else
            {
                // ✅ É um equipamento físico - buscar histórico via equipamentohistorico
                var equipamentosHist = _equipamentohistoricovmRepository.Buscar(x => x.Equipamentoid == id).OrderByDescending(x => x.Dtregistro).ToList();
                return equipamentosHist;
            }
        }

        // ✅ NOVO: Buscar histórico por número de série (resolve conflito de IDs entre equipamentos e linhas)
        public List<Equipamentohistoricovm> HistoricoEquipamentoPorNumeroSerie(string numeroSerie)
        {
            var linhaExiste = _requisicaoItensRepository.Buscar(x => x.Linhatelefonica.HasValue)
                .Include(x => x.LinhatelefonicaNavigation)
                .Where(x => x.LinhatelefonicaNavigation.Numero.ToString() == numeroSerie)
                .FirstOrDefault();
            
            if (linhaExiste != null)
            {
                return BuscarHistoricoLinhaTelefonica(linhaExiste.Linhatelefonica.Value);
            }
            
            var equipamentoExiste = _equipamentoRepository
                .Buscar(x => x.Numeroserie == numeroSerie)
                .FirstOrDefault();
            
            if (equipamentoExiste != null)
            {
                return BuscarHistoricoEquipamentoFisico(equipamentoExiste.Id);
            }
            
            return new List<Equipamentohistoricovm>();
        }

        // ✅ NOVO: Método privado para buscar histórico de linha telefônica
        private List<Equipamentohistoricovm> BuscarHistoricoLinhaTelefonica(int linhaId)
        {
            var linhasHist = _requisicaoItensRepository.Buscar(x => x.Linhatelefonica == linhaId)
                .Include(x => x.LinhatelefonicaNavigation)
                .ThenInclude(x => x.PlanoNavigation)
                .ThenInclude(x => x.ContratoNavigation)
                .ThenInclude(x => x.OperadoraNavigation)
                .Include(x => x.RequisicaoNavigation)
                .OrderByDescending(x => x.Dtentrega)
                .ToList();

            // Buscar nomes dos usuários
            var usuarioIds = linhasHist.Where(x => x.Usuarioentrega.HasValue).Select(x => x.Usuarioentrega.Value)
                .Concat(linhasHist.Where(x => x.RequisicaoNavigation != null).Select(x => x.RequisicaoNavigation.Usuariorequisicao))
                .Concat(linhasHist.Where(x => x.RequisicaoNavigation != null).Select(x => x.RequisicaoNavigation.Tecnicoresponsavel))
                .Distinct().ToList();
            var usuarios = new Dictionary<int, string>();
            if (usuarioIds.Any())
            {
                var usuariosEncontrados = _usuarioRepository.Buscar(x => usuarioIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.Nome })
                    .ToList();
                usuarios = usuariosEncontrados.ToDictionary(x => x.Id, x => x.Nome);
            }

            // Buscar nomes dos colaboradores
            var colaboradorIds = linhasHist.Where(x => x.RequisicaoNavigation?.Colaboradorfinal.HasValue == true)
                .Select(x => x.RequisicaoNavigation.Colaboradorfinal.Value).Distinct().ToList();
            var colaboradores = new Dictionary<int, string>();
            if (colaboradorIds.Any())
            {
                var colaboradoresEncontrados = _colaboradorRepository.Buscar(x => colaboradorIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.Nome })
                    .ToList();
                colaboradores = colaboradoresEncontrados.ToDictionary(x => x.Id, x => x.Nome);
            }

            // Converter linhas telefônicas para Equipamentohistoricovm
            var linhasVM = linhasHist.Select(lt => {
                var nomeUsuario = "Sistema";
                if (lt.Usuarioentrega.HasValue && usuarios.ContainsKey(lt.Usuarioentrega.Value))
                {
                    nomeUsuario = usuarios[lt.Usuarioentrega.Value];
                }
                else if (lt.RequisicaoNavigation != null && usuarios.ContainsKey(lt.RequisicaoNavigation.Usuariorequisicao))
                {
                    nomeUsuario = usuarios[lt.RequisicaoNavigation.Usuariorequisicao];
                }
                else if (lt.RequisicaoNavigation != null && usuarios.ContainsKey(lt.RequisicaoNavigation.Tecnicoresponsavel))
                {
                    nomeUsuario = usuarios[lt.RequisicaoNavigation.Tecnicoresponsavel];
                }
                
                var nomeColaborador = "Sistema";
                if (lt.RequisicaoNavigation?.Colaboradorfinal.HasValue == true && colaboradores.ContainsKey(lt.RequisicaoNavigation.Colaboradorfinal.Value))
                {
                    nomeColaborador = colaboradores[lt.RequisicaoNavigation.Colaboradorfinal.Value];
                }
                else if (lt.RequisicaoNavigation != null && usuarios.ContainsKey(lt.RequisicaoNavigation.Tecnicoresponsavel))
                {
                    nomeColaborador = usuarios[lt.RequisicaoNavigation.Tecnicoresponsavel];
                }
                
                return new Equipamentohistoricovm
                {
                    Id = lt.Id,
                    Equipamentoid = lt.Linhatelefonica,
                    Tipoequipamento = "Linha Telefônica",
                    Fabricante = lt.LinhatelefonicaNavigation?.PlanoNavigation?.ContratoNavigation?.OperadoraNavigation?.Nome ?? "N/A",
                    Modelo = lt.LinhatelefonicaNavigation?.Numero.ToString() ?? "N/A",
                    Numeroserie = lt.LinhatelefonicaNavigation?.Numero.ToString(),
                    Patrimonio = lt.LinhatelefonicaNavigation?.PlanoNavigation?.ContratoNavigation?.OperadoraNavigation?.Nome ?? "N/A",
                    Equipamentostatusid = lt.Dtdevolucao.HasValue ? 5 : 4,
                    Equipamentostatus = lt.Dtdevolucao.HasValue ? "Devolvido" : "Entregue",
                    Colaboradorid = lt.Usuarioentrega,
                    Colaborador = nomeColaborador,
                    Dtregistro = lt.Dtentrega ?? DateTime.Now,
                    Usuarioid = lt.Usuarioentrega,
                    Usuario = nomeUsuario,
                    Tecnicoresponsavelid = lt.Usuariodevolucao,
                    Tecnicoresponsavel = nomeUsuario
                };
            }).ToList();
            
            return linhasVM;
        }

        // ✅ NOVO: Método privado para buscar histórico de equipamento físico
        private List<Equipamentohistoricovm> BuscarHistoricoEquipamentoFisico(int equipamentoId)
        {
            var equipamentosHist = _equipamentohistoricovmRepository.Buscar(x => x.Equipamentoid == equipamentoId).OrderByDescending(x => x.Dtregistro).ToList();
            return equipamentosHist;
        }
        
        public List<RequisicaoVM> EquipamentosComColaboradores(int id)
        {
            var vms = new List<RequisicaoVM>();
            var reqs = _requisicoesvmRepository.Buscar(x => x.Colaboradorfinalid == id).OrderByDescending(x => x.Id).ToList();
            foreach(var r in reqs)
            {
                var vm = new RequisicaoVM();
                vm.Requisicao = r;
                
                // ✅ CORREÇÃO: Buscar equipamentos físicos
                var equipamentos = _requisicaoequipamentosvmsRepository.Buscar(x => x.Requisicao == r.Id).OrderByDescending(x => x.Dtdevolucao).ToList();
                
                
                // ✅ NOVO: Buscar linhas telefônicas da requisição
                var linhasTelefonicas = _requisicaoItensRepository.Buscar(x => x.Requisicao == r.Id && x.Linhatelefonica.HasValue && x.Linhatelefonica > 0)
                    .Include(x => x.LinhatelefonicaNavigation)
                    .ThenInclude(x => x.PlanoNavigation)
                    .ThenInclude(x => x.ContratoNavigation)
                    .ThenInclude(x => x.OperadoraNavigation)
                    .ToList();

                // ✅ NOVO: Converter linhas telefônicas para o mesmo formato dos equipamentos
                var linhasVM = linhasTelefonicas.Select(lt => {
                    var status = lt.Dtdevolucao.HasValue ? 5 : 4;
                    return new Requisicaoequipamentosvm
                    {
                        Id = lt.Id,
                        Requisicao = lt.Requisicao,
                        Equipamento = $"Linha Telefônica - {lt.LinhatelefonicaNavigation?.Numero}",
                        Numeroserie = lt.LinhatelefonicaNavigation?.Numero.ToString(),
                        Patrimonio = lt.LinhatelefonicaNavigation?.PlanoNavigation?.ContratoNavigation?.OperadoraNavigation?.Nome ?? "N/A",
                        Dtentrega = lt.Dtentrega,
                        Dtdevolucao = lt.Dtdevolucao,
                        Usuarioentregaid = lt.Usuarioentrega,
                        Usuariodevolucaoid = lt.Usuariodevolucao,
                        Observacaoentrega = lt.Observacaoentrega,
                        Dtprogramadaretorno = lt.Dtprogramadaretorno,
                        Equipamentostatus = status, // 4 = Entregue (Ativo), 5 = Devolvido
                        Numero = lt.LinhatelefonicaNavigation?.Numero ?? 0,
                        Linhaid = lt.Linhatelefonica,
                        TipoAquisicao = 1 // 1 = Corporativo (não-BYOD)
                    };
                }).ToList();

                // ✅ NOVO: Combinar equipamentos e linhas telefônicas
                vm.EquipamentosRequisicao = equipamentos.Concat(linhasVM).OrderByDescending(x => x.Dtdevolucao).ToList();
                
                vms.Add(vm);
            }
            return vms;
        }

        public MovimentacoesVM MovimentacoesColaboradores(int cliente, int pagina, string relatorio, string pesquisa)
        {
            pesquisa = pesquisa.ToLower();
            var vm = new MovimentacoesVM();
            switch(relatorio.ToUpper())
            {
                case "T":
                    {
                        vm.StatusColaborador = _colaboradorhistoricovmsRepository.Buscar(x => x.Cliente == cliente && x.Dtatualizacao != null && ((pesquisa != "null") ? x.Nome.Contains(pesquisa.ToUpper()) : 1 == 1)).OrderByDescending(x => x.Dtatualizacao).GetPaged(pagina, 10);
                        vm.EmpresaColaborador = _colaboradorhistoricovmsRepository.Buscar(x => x.Cliente == cliente && x.Dtatualizacaoempresa != null && ((pesquisa != "null") ? x.Nome.Contains(pesquisa.ToUpper()) : 1 == 1)).OrderByDescending(x => x.Dtatualizacaoempresa).GetPaged(pagina, 10);
                        vm.LocalidadeColaborador = _colaboradorhistoricovmsRepository.Buscar(x => x.Cliente == cliente && x.Dtatualizacaolocalidade != null && ((pesquisa != "null") ? x.Nome.Contains(pesquisa.ToUpper()) : 1 == 1)).OrderByDescending(x => x.Dtatualizacaolocalidade).GetPaged(pagina, 10);
                        vm.CentroCustoColaborador = _colaboradorhistoricovmsRepository.Buscar(x => x.Cliente == cliente && x.Dtatualizacaocentrocusto != null && ((pesquisa != "null") ? x.Nome.Contains(pesquisa.ToUpper()) : 1 == 1)).OrderByDescending(x => x.Dtatualizacaocentrocusto).GetPaged(pagina, 10);
                        break;
                    }
                case "S":
                    {
                        vm.StatusColaborador = _colaboradorhistoricovmsRepository.Buscar(x => x.Cliente == cliente && x.Dtatualizacao != null).OrderByDescending(x => x.Dtatualizacao).GetPaged(pagina, 10);
                        break;
                    }
                case "E":
                    {
                        vm.EmpresaColaborador = _colaboradorhistoricovmsRepository.Buscar(x => x.Cliente == cliente && x.Dtatualizacaoempresa != null).OrderByDescending(x => x.Dtatualizacaoempresa).GetPaged(pagina, 10);
                        break;
                    }
                case "L":
                    {
                        vm.LocalidadeColaborador = _colaboradorhistoricovmsRepository.Buscar(x => x.Cliente == cliente && x.Dtatualizacaolocalidade != null).OrderByDescending(x => x.Dtatualizacaolocalidade).GetPaged(pagina, 10);
                        break;
                    }
                case "C":
                    {
                        vm.CentroCustoColaborador = _colaboradorhistoricovmsRepository.Buscar(x => x.Cliente == cliente && x.Dtatualizacaocentrocusto != null).OrderByDescending(x => x.Dtatualizacaocentrocusto).GetPaged(pagina, 10);
                        break;
                    }
            }

            return vm;
        }
        public List<Vwequipamentosdetalhe> ConsultarDetalhesEquipamentos(Vwequipamentosdetalhe vw)
        {
            // Montar consulta incremental garantindo interseção dos filtros
            var query = _vwequipamentosdetalhesRepository.Query()
                .Where(x => x.Cliente == vw.Cliente);

            if (vw.Equipamentostatusid != null)
                query = query.Where(x => x.Equipamentostatusid == vw.Equipamentostatusid);

            if (vw.Localidadeid != null)
                query = query.Where(x => x.Localidadeid == vw.Localidadeid);

            if (vw.Empresaid != null)
                query = query.Where(x => x.Empresaid == vw.Empresaid);

            if (vw.Tipoequipamentoid != null)
                query = query.Where(x => x.Tipoequipamentoid == vw.Tipoequipamentoid);

            if (vw.Fabricanteid != null)
                query = query.Where(x => x.Fabricanteid == vw.Fabricanteid);

            if (vw.Modeloid != null)
                query = query.Where(x => x.Modeloid == vw.Modeloid);

            if (vw.Centrocustoid != null)
                query = query.Where(x => x.Centrocustoid == vw.Centrocustoid);

            if (vw.Tipoaquisicao != null)
            {
                // EXISTS em Equipamento por cliente, tipo e chave de vínculo (número de série ou patrimônio)
                var eq = _equipamentoRepository.Query();
                query = query.Where(v => eq.Any(e => e.Cliente == vw.Cliente && e.Tipoaquisicao == vw.Tipoaquisicao &&
                    ((e.Numeroserie != null && v.Numeroserie != null && e.Numeroserie == v.Numeroserie) ||
                     (e.Patrimonio != null && v.Patrimonio != null && e.Patrimonio == v.Patrimonio))));
            }

            if (vw.Categoriaid != null)
            {
                var eq = _equipamentoRepository.Query();
                query = query.Where(v => eq.Any(e => e.Cliente == vw.Cliente && e.TipoequipamentoNavigation != null && e.TipoequipamentoNavigation.CategoriaId == vw.Categoriaid &&
                    ((e.Numeroserie != null && v.Numeroserie != null && e.Numeroserie == v.Numeroserie) ||
                     (e.Patrimonio != null && v.Patrimonio != null && e.Patrimonio == v.Patrimonio))));
            }

            return query.ToList();
        }

        public DashboardMobileVM DashboardMobile(int cliente)
        {
            var tipos = (from eqp in _equipamentoRepository.Include(x => x.TipoequipamentoNavigation)
                         where eqp.Cliente == cliente && eqp.TipoequipamentoNavigation.Ativo
                         group eqp by eqp.TipoequipamentoNavigation.Descricao into g
                         select new TotalEquipamentosVM()
                         {
                             Equipamento = g.Key,
                             Total = g.Count()
                         }).ToList();
            //var tipos = _equipamentoRepository.Include(x => x.TipoequipamentoNavigation).Buscar(x => x.Cliente == cliente).GroupBy(x => x.TipoequipamentoNavigation.Descricao

            var laudos = new TotalLaudosVM();
            laudos.Abertos = _laudoRepository.Buscar(x => x.Dtlaudo == null && x.Cliente == cliente).Count();
            laudos.Finalizados = _laudoRepository.Buscar(x => x.Dtlaudo != null && x.Cliente == cliente).Count();

            var vm = new DashboardMobileVM();
            vm.TotalEquipamentos = tipos;
            vm.TotalLaudos = laudos;
            
            return vm;
            //var tipos = _equipamentoRepository.Include(x => x.Tipoequipamento).Buscar(x => x.Cliente == cliente)
            //            .GroupBy(x => x.)
        }
        public DashboardWebVM DashboardWeb(int cliente)
        {
            try
            {
                DateTime dtNow = TimeZoneMapper.GetDateTimeNow();
                DateTime hoje = DateTime.Today;
                DateTime ontem = hoje.AddDays(-1);
                
            var vm = new DashboardWebVM();
                
                // ========== TIMESTAMP ==========
                vm.UltimaAtualizacao = DateTime.Now;
                
                // Buscar devoluções programadas enriquecidas com dados do equipamento
                var devolucoesProgramadasDetalhadas = (from evm in _requisicaoequipamentosvmsRepository.Query()
                                                       join r in _requisicoesvmRepository.Query() on evm.Requisicao equals r.Id
                                                       join col in _colaboradorRepository.Query() on r.Colaboradorfinalid equals col.Id
                                                       join eq in _vwequipamentosdetalhesRepository.Query() on evm.Equipamentoid equals eq.Id
                                                       where r.Cliente == cliente
                                                             && evm.Dtprogramadaretorno.HasValue
                                                             && evm.Equipamentostatus == 4 // Entregue
                                                             && !evm.Dtdevolucao.HasValue // Ainda não devolvido
                                                       select new Vwdevolucaoprogramadum
                                                       {
                                                           Cliente = cliente,
                                                           Nomecolaborador = col.Nome,
                                                           Dtprogramadaretorno = evm.Dtprogramadaretorno,
                                                           // 🆕 Campos adicionais para enriquecer os dados
                                                           Matricula = col.Matricula,
                                                           ColaboradorId = col.Id,
                                                           Equipamento = (eq.Tipoequipamento ?? "") + " " + (eq.Fabricante ?? "") + " " + (eq.Modelo ?? ""),
                                                           Serial = eq.Numeroserie,
                                                           Patrimonio = eq.Patrimonio,
                                                           EquipamentoId = eq.Id,
                                                           RequisicaoId = r.Id,
                                                           RequisicoesItemId = evm.Id
                                                       })
                                                       .OrderByDescending(x => x.Dtprogramadaretorno)
                                                       .ToList();
                
                vm.DevolucoesProgramadas = devolucoesProgramadasDetalhadas;
                    
                var hojeDash = TimeZoneMapper.GetDateTimeNow().Date;

                // Colaboradores com demissão hoje ou anterior
                var colaboradoresDesligados = _colaboradorRepository
                    .Buscar(x => x.Cliente == cliente && x.Dtdemissao.HasValue && x.Dtdemissao.Value.Date <= hojeDash)
                    .ToList();
                var idsDesligados = colaboradoresDesligados.Select(c => c.Id).ToHashSet();
                vm.TotalColaboradoresDesligados = idsDesligados.Count;

                // Buscar equipamentos individuais de colaboradores desligados
                // Status 4 = Entregue, e que não tenham data de devolução
                var equipamentosAtivos = (from r in _requisicoesvmRepository.Query()
                                         join evm in _requisicaoequipamentosvmsRepository.Query() on r.Id equals evm.Requisicao
                                         join eq in _vwequipamentosdetalhesRepository.Query() on evm.Equipamentoid equals eq.Id
                                         where r.Cliente == cliente
                                               && r.Colaboradorfinalid.HasValue
                                               && evm.Equipamentostatus.HasValue && evm.Equipamentostatus.Value == 4
                                               && (!evm.Dtdevolucao.HasValue)
                                         select new 
                                         { 
                                             ColabId = r.Colaboradorfinalid.Value,
                                             EquipamentoId = eq.Id.Value,
                                             TipoEquipamento = eq.Tipoequipamento ?? "Equipamento",
                                             Fabricante = eq.Fabricante ?? "",
                                             Modelo = eq.Modelo ?? "",
                                             NumeroSerie = eq.Numeroserie ?? "",
                                             Patrimonio = eq.Patrimonio ?? ""
                                         })
                                         .ToList();

                var equipamentosComDesligados = new List<Vwequipamentoscomcolaboradoresdesligado>();
                
                // Agrupar por colaborador para contar total de equipamentos
                var equipamentosPorColaborador = equipamentosAtivos
                    .Where(x => idsDesligados.Contains(x.ColabId))
                    .GroupBy(x => x.ColabId)
                    .ToList();

                foreach (var grupo in equipamentosPorColaborador)
                {
                    var colaborador = colaboradoresDesligados.First(c => c.Id == grupo.Key);
                    var totalEquipamentos = grupo.Count();
                    
                    // Criar uma entrada para cada equipamento individual
                    foreach (var equip in grupo)
                    {
                        // Montar descrição completa do equipamento
                        var partes = new List<string>();
                        
                        if (!string.IsNullOrEmpty(equip.TipoEquipamento))
                            partes.Add(equip.TipoEquipamento);
                        
                        if (!string.IsNullOrEmpty(equip.Fabricante))
                            partes.Add(equip.Fabricante);
                        
                        if (!string.IsNullOrEmpty(equip.Modelo))
                            partes.Add(equip.Modelo);
                        
                        // Se tiver patrimônio ou série, adicionar entre parênteses
                        var identificadores = new List<string>();
                        if (!string.IsNullOrEmpty(equip.Patrimonio))
                            identificadores.Add($"Patr: {equip.Patrimonio}");
                        if (!string.IsNullOrEmpty(equip.NumeroSerie))
                            identificadores.Add($"S/N: {equip.NumeroSerie}");
                        
                        var descricaoEquipamento = partes.Count > 0 
                            ? string.Join(" ", partes) 
                            : "Equipamento";
                        
                        if (identificadores.Count > 0)
                            descricaoEquipamento += $" ({string.Join(", ", identificadores)})";
                        
                        equipamentosComDesligados.Add(new Vwequipamentoscomcolaboradoresdesligado
                        {
                            Cliente = cliente,
                            Nome = colaborador.Nome,
                            ColaboradorId = colaborador.Id,
                            Matricula = colaborador.Matricula,  // 🆕 Adicionar matrícula
                            Dtdemissao = colaborador.Dtdemissao,
                            Equipamento = descricaoEquipamento,
                            EquipamentoId = equip.EquipamentoId,
                            // Cada linha representa 1 recurso; somatório dará o total de recursos
                            Qtde = 1
                        });
                    }
                }

                vm.EquipamentosComColaboradorDesligado = equipamentosComDesligados
                    .OrderBy(x => x.Dtdemissao)
                    .ToList();
                // Distintos por colaborador
                vm.TotalDesligadosComRecursos = vm.EquipamentosComColaboradorDesligado
                    .Select(x => x.ColaboradorId)
                    .Distinct()
                    .Count();
                // Soma correta de recursos (cada item vale 1)
                vm.TotalRecursosDesligados = (int)(vm.EquipamentosComColaboradorDesligado.Sum(x => x.Qtde ?? 0));
                Console.WriteLine($"[DASHBOARD] Total desligados: {vm.TotalColaboradoresDesligados}; com recursos: {vm.TotalDesligadosComRecursos}; recursos somados: {vm.TotalRecursosDesligados}");
                    
                // Buscar equipamentos por status, EXCLUINDO Linhas Telefônicas (gestão exclusiva no Telecom)
                vm.EquipamentosPorStatus = _equipamentosPorStatusRepository
                    .Buscar(x => x.Cliente == cliente && 
                                 x.Tipoequipamento != "Linha Telefônica" && 
                                 x.Tipoequipamento != "Linha Telefonica" &&
                                 !x.Tipoequipamento.ToLower().Contains("telefon"))
                    .ToList();
                    
                // 🔥 FILTRAR APENAS REQUISIÇÕES DE COLABORADORES COM RECURSOS ATIVOS
                // Buscar colaboradores que possuem recursos ativos (não devolvidos)
                var colaboradoresComRecursosAtivos = (from eh in _equipamentoHistoricoRepository.Query()
                                                       where eh.Equipamentostatus == 4 // Status 4 = Entregue (ATIVO)
                                                       select eh.Colaborador)
                                                      .Distinct()
                                                      .ToList();

                Console.WriteLine($"[DASHBOARD] 📊 Colaboradores com recursos ativos: {colaboradoresComRecursosAtivos.Count}");

                // Contar apenas requisições de colaboradores COM recursos ativos
                vm.AdesaoTermoResponsabilidade.Assinados = _requisicaoRepository
                    .Buscar(x => x.Cliente == cliente && 
                                 x.Assinaturaeletronica && 
                                 x.Colaboradorfinal.HasValue &&
                                 colaboradoresComRecursosAtivos.Contains(x.Colaboradorfinal.Value))
                    .Count();
                    
                vm.AdesaoTermoResponsabilidade.NaoAssinados = _requisicaoRepository
                    .Buscar(x => x.Cliente == cliente && 
                                 !x.Assinaturaeletronica &&
                                 x.Colaboradorfinal.HasValue &&
                                 colaboradoresComRecursosAtivos.Contains(x.Colaboradorfinal.Value))
                    .Count();

                Console.WriteLine($"[DASHBOARD] ✅ Assinados (com recursos ativos): {vm.AdesaoTermoResponsabilidade.Assinados}");
                Console.WriteLine($"[DASHBOARD] ❌ Não assinados (com recursos ativos): {vm.AdesaoTermoResponsabilidade.NaoAssinados}");
                    
                // REMOVIDO: QtdeAtivosDescartados e QtdeAtivosRoubados
                // Agora são KPIs completos: vm.RecursosDescartados e vm.RecursosPerdidos
                    
                vm.QtdeContestacoesPendentes = _contestacaoRepository
                    .Buscar(x => x.Status == "pendente")
                    .Count();

                // Lista resumida das contestações pendentes (exibir no cartão)
                try
                {
                    var pendenciasContestacoes = _contestacaoRepository
                        .Buscar(x => x.Status == "pendente")
                        .Select(x => new ContestacaoPendenteResumo
                        {
                            Id = x.Id,
                            TipoContestacao = "contestacao",
                            Status = "pendente",
                            Motivo = string.Empty,
                            Descricao = string.Empty,
                            DataContestacao = DateTime.Now
                        })
                        .OrderByDescending(x => x.DataContestacao)
                        .Take(10)
                        .ToList();

                    vm.ContestacoesPendentesLista = pendenciasContestacoes;
                }
                catch { vm.ContestacoesPendentesLista = new List<ContestacaoPendenteResumo>(); }

                // ➕ Lista resumida de contestações pendentes (inclui auto inventário)
                // Mostra as 5 mais recentes para o card de Ações Pendentes
                vm.ContestacoesPendentesLista = _contestacaoRepository
                    .Buscar(x => x.Status == "pendente")
                    .OrderByDescending(x => x.DataContestacao)
                    .Take(5)
                    .Select(x => new ContestacaoPendenteResumo
                    {
                        Id = x.Id,
                        TipoContestacao = (x.TipoContestacao ?? "contestacao").Trim().ToLower().Replace("-", "_").Replace(" ", "_") == "auto_inventario" ? "auto_inventario" : "contestacao",
                        Status = x.Status,
                        Motivo = x.Motivo,
                        Descricao = x.Descricao,
                        DataContestacao = x.DataContestacao
                    })
                    .ToList();
                    
                // Recursos movimentados HOJE: entregas + devoluções (contar operações separadamente)
                var entregasHoje = (from evm in _requisicaoequipamentosvmsRepository.Query() 
                                   join r in _requisicaoRepository.Query() on evm.Requisicao equals r.Id 
                                   where r.Cliente == cliente && 
                                         evm.Dtentrega.HasValue && evm.Dtentrega.Value.Date == hoje
                                   select evm).Count();
                
                var devolucoesHoje = (from evm in _requisicaoequipamentosvmsRepository.Query() 
                                     join r in _requisicaoRepository.Query() on evm.Requisicao equals r.Id 
                                     where r.Cliente == cliente && 
                                           evm.Dtdevolucao.HasValue && evm.Dtdevolucao.Value.Date == hoje
                                     select evm).Count();
                
                // Total de movimentações = entregas + devoluções
                vm.QtdeAtivosMovimentadoDia = entregasHoje + devolucoesHoje;
                                               
                // ✅ CORREÇÃO: Contar ENTREGAS e DEVOLUÇÕES nos últimos 5 dias
                var dataLimite = dtNow.AddDays(-5);
                
                // Buscar todas as movimentações (entregas + devoluções) dos últimos 5 dias
                var movimentacoes = (from evm in _requisicaoequipamentosvmsRepository.Query() 
                                     join r in _requisicaoRepository.Query() on evm.Requisicao equals r.Id 
                                     where r.Cliente == cliente && 
                                           ((evm.Dtentrega.HasValue && evm.Dtentrega.Value >= dataLimite) ||
                                            (evm.Dtdevolucao.HasValue && evm.Dtdevolucao.Value >= dataLimite))
                                     select new
                                     {
                                         UsuarioEntrega = evm.Usuarioentrega,
                                         UsuarioDevolucao = evm.Usuariodevolucao,
                                         TemEntrega = evm.Dtentrega.HasValue && evm.Dtentrega.Value >= dataLimite,
                                         TemDevolucao = evm.Dtdevolucao.HasValue && evm.Dtdevolucao.Value >= dataLimite
                                     })
                                     .ToList();
                
                // Consolidar contagem por usuário (entregas + devoluções)
                var contagemPorUsuario = new Dictionary<string, int>();
                
                foreach (var mov in movimentacoes)
                {
                    // Contar entrega
                    if (mov.TemEntrega && !string.IsNullOrEmpty(mov.UsuarioEntrega))
                    {
                        if (!contagemPorUsuario.ContainsKey(mov.UsuarioEntrega))
                            contagemPorUsuario[mov.UsuarioEntrega] = 0;
                        contagemPorUsuario[mov.UsuarioEntrega]++;
                    }
                    
                    // Contar devolução
                    if (mov.TemDevolucao && !string.IsNullOrEmpty(mov.UsuarioDevolucao))
                    {
                        if (!contagemPorUsuario.ContainsKey(mov.UsuarioDevolucao))
                            contagemPorUsuario[mov.UsuarioDevolucao] = 0;
                        contagemPorUsuario[mov.UsuarioDevolucao]++;
                    }
                }
                
                vm.UltimosUsuariosQueMovimentaram = contagemPorUsuario;
                
                Console.WriteLine($"[DASHBOARD] Movimentações últimos 5 dias: {movimentacoes.Count} registros, {contagemPorUsuario.Count} usuários únicos");
                foreach (var kvp in contagemPorUsuario.OrderByDescending(x => x.Value).Take(5))
                {
                    Console.WriteLine($"[DASHBOARD]   - {kvp.Key}: {kvp.Value} movimentações");
                }
                
                // ========== NOVO: KPI - TOTAL DE RECURSOS ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Total de Recursos...");
                
                var totalRecursosHoje = _equipamentoRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo)
                    .Count();
                    
                var totalRecursosOntem = _equipamentoRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo && x.Dtcadastro < hoje)
                    .Count();
                    
                vm.TotalRecursos = CalcularKPI(
                    totalRecursosHoje, 
                    totalRecursosOntem, 
                    "percentual",
                    ObterSparklineRecursos(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Total Recursos: {totalRecursosHoje} (Ontem: {totalRecursosOntem})");
                
                // ========== NOVO: KPI - SINALIZAÇÕES PENDENTES ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Sinalizações Pendentes...");
                
                var sinalizacoesPendentesHoje = _sinalizacaoRepository
                    .Buscar(x => x.Status == "pendente")
                    .Include(x => x.Colaborador)
                    .Where(x => x.Colaborador.Cliente == cliente)
                    .Count();
                    
                var sinalizacoesPendentesOntem = _sinalizacaoRepository
                    .Buscar(x => x.Status == "pendente" && x.DataSinalizacao < hoje)
                    .Include(x => x.Colaborador)
                    .Where(x => x.Colaborador.Cliente == cliente)
                    .Count();
                    
                vm.SinalizacoesPendentes = CalcularKPI(
                    sinalizacoesPendentesHoje,
                    sinalizacoesPendentesOntem,
                    "absoluto",
                    ObterSparklineSinalizacoes(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Sinalizações Pendentes: {sinalizacoesPendentesHoje} (Ontem: {sinalizacoesPendentesOntem})");
                
                // ========== NOVO: KPI - DEVOLUÇÕES PENDENTES ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Devoluções Pendentes...");
                
                var devolucoesPendentes = vm.DevolucoesProgramadas?.Count ?? 0;
                var devolucoesPendentesOntem = _vwdevolucaoprogramadumRepository
                    .Buscar(x => x.Cliente == cliente && x.Dtprogramadaretorno < hoje)
                    .Count();
                    
                vm.DevolucoesPendentes = CalcularKPI(
                    devolucoesPendentes,
                    devolucoesPendentesOntem,
                    "absoluto",
                    ObterSparklineDevolucoes(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Devoluções Pendentes: {devolucoesPendentes} (Ontem: {devolucoesPendentesOntem})");
                
                // ========== NOVO: KPI - TAXA DE ADESÃO AO TERMO ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Taxa de Adesão...");
                
                var totalRequisicoes = vm.AdesaoTermoResponsabilidade.Assinados + vm.AdesaoTermoResponsabilidade.NaoAssinados;
                var taxaAdesaoHoje = totalRequisicoes > 0 ? 
                    (int)Math.Round((vm.AdesaoTermoResponsabilidade.Assinados / (decimal)totalRequisicoes) * 100) : 0;
                    
                var requisicoeOntem = _requisicaoRepository.Buscar(x => x.Cliente == cliente && x.Dtprocessamento.HasValue && x.Dtprocessamento.Value < hoje).Count();
                var assinadosOntem = _requisicaoRepository.Buscar(x => x.Cliente == cliente && x.Dtprocessamento.HasValue && x.Dtprocessamento.Value < hoje && x.Assinaturaeletronica).Count();
                var taxaAdesaoOntem = requisicoeOntem > 0 ? 
                    (int)Math.Round((assinadosOntem / (decimal)requisicoeOntem) * 100) : 0;
                    
                vm.TaxaAdesaoTermo = CalcularKPI(
                    taxaAdesaoHoje,
                    taxaAdesaoOntem,
                    "percentual",
                    ObterSparklineAdesao(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Taxa de Adesão: {taxaAdesaoHoje}% (Ontem: {taxaAdesaoOntem}%)");
                
                // ========== NOVO: KPI - CONTESTAÇÕES E AUTO INVENTÁRIO PENDENTES ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Contestações Pendentes...");
                
                var contestacoesPendentesHoje = _contestacaoRepository
                    .Buscar(x => x.Status == "pendente")
                    .Include(x => x.Colaborador)
                    .Where(x => x.Colaborador != null && x.Colaborador.Cliente == cliente)
                    .Count();
                    
                var contestacoesPendentesOntem = _contestacaoRepository
                    .Buscar(x => x.Status == "pendente" && x.DataContestacao < hoje)
                    .Include(x => x.Colaborador)
                    .Where(x => x.Colaborador != null && x.Colaborador.Cliente == cliente)
                    .Count();
                    
                vm.ContestacoesPendentes = CalcularKPI(
                    contestacoesPendentesHoje,
                    contestacoesPendentesOntem,
                    "absoluto",
                    ObterSparklineContestacoes(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Contestações Pendentes: {contestacoesPendentesHoje} (Ontem: {contestacoesPendentesOntem})");
                
                // ========== NOVO: KPI - NÃO CONFORMIDADES DE ELEGIBILIDADE ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Não Conformidades de Elegibilidade...");
                
                // Contar não conformidades usando a view vw_nao_conformidade_elegibilidade
                var sqlNaoConformidadeHoje = @"SELECT COUNT(*) FROM vw_nao_conformidade_elegibilidade WHERE cliente = @p0";
                int naoConformidadesHoje = 0;
                var connectionNC = _context.Database.GetDbConnection();
                var commandNC = connectionNC.CreateCommand();
                commandNC.CommandText = sqlNaoConformidadeHoje;
                var paramNC = commandNC.CreateParameter();
                paramNC.ParameterName = "p0";
                paramNC.Value = cliente;
                commandNC.Parameters.Add(paramNC);
                if (connectionNC.State != System.Data.ConnectionState.Open)
                    connectionNC.Open();
                var scalarNC = commandNC.ExecuteScalar();
                naoConformidadesHoje = Convert.ToInt32(scalarNC);
                
                // Para ontem, considerar a mesma quantidade (pois a view é dinâmica)
                // Idealmente, deveria haver um histórico, mas por enquanto usamos o mesmo valor
                var naoConformidadesOntem = naoConformidadesHoje;
                    
                vm.NaoConformidadeElegibilidade = CalcularKPI(
                    naoConformidadesHoje,
                    naoConformidadesOntem,
                    "absoluto",
                    ObterSparklineNaoConformidade(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Não Conformidades: {naoConformidadesHoje} (Ontem: {naoConformidadesOntem})");
                
                // ========== NOVO: KPI - GARANTIAS CRÍTICAS (SEM DATA + EXPIRADAS) ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Garantias Críticas...");
                
                // Buscar equipamentos ativos sem data de garantia ou com garantia expirada
                var equipamentosGarantia = _equipamentoRepository
                    .Buscar(x => x.Ativo && x.Cliente == cliente)
                    .ToList();
                
                var garantiasCriticasHoje = equipamentosGarantia.Count(eq => 
                    !eq.Dtlimitegarantia.HasValue || // Sem data de garantia
                    eq.Dtlimitegarantia.Value < hoje  // Garantia expirada
                );
                
                // Para ontem, usar o mesmo valor (snapshot atual)
                var garantiasCriticasOntem = garantiasCriticasHoje;
                    
                vm.GarantiasCriticas = CalcularKPI(
                    garantiasCriticasHoje,
                    garantiasCriticasOntem,
                    "absoluto",
                    ObterSparklineGarantiasCriticas(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Garantias Críticas: {garantiasCriticasHoje} (Sem data + Expiradas)");
                
                // ========== NOVO: KPI - RECURSOS DESCARTADOS ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Recursos Descartados...");
                
                var recursosDescartadosHoje = _vwequipamentosdetalhesRepository
                    .Buscar(x => x.Cliente == cliente && x.Equipamentostatusid == 10)
                    .Count();
                    
                // Para ontem, usar o mesmo valor (a view é dinâmica e não tem histórico)
                var recursosDescartadosOntem = recursosDescartadosHoje;
                    
                vm.RecursosDescartados = CalcularKPI(
                    recursosDescartadosHoje,
                    recursosDescartadosOntem,
                    "absoluto",
                    ObterSparklineDescartados(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Recursos Descartados: {recursosDescartadosHoje} (Ontem: {recursosDescartadosOntem})");
                
                // ========== NOVO: KPI - RECURSOS PERDIDOS (ROUBADOS/EXTRAVIADOS) ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Recursos Perdidos (Roubados/Extraviados)...");
                
                var recursosPerdidosHoje = _vwequipamentosdetalhesRepository
                    .Buscar(x => x.Cliente == cliente && (x.Equipamentostatusid == 5 || x.Equipamentostatusid == 8))
                    .Count();
                    
                // Para ontem, usar o mesmo valor (a view é dinâmica e não tem histórico)
                var recursosPerdidosOntem = recursosPerdidosHoje;
                    
                vm.RecursosPerdidos = CalcularKPI(
                    recursosPerdidosHoje,
                    recursosPerdidosOntem,
                    "absoluto",
                    ObterSparklinePerdidos(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Recursos Perdidos: {recursosPerdidosHoje} (Roubados/Extraviados, Ontem: {recursosPerdidosOntem})");
                
                // ========== NOVO: KPI - ADMINISTRADORES ATIVOS ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Administradores Ativos...");
                
                var administradoresAtivosHoje = _usuarioRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo && x.Adm && !x.Su)
                    .Count();
                    
                var administradoresAtivosOntem = _usuarioRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo && x.Adm && !x.Su && x.Ultimologin.HasValue && x.Ultimologin.Value < hoje)
                    .Count();
                    
                vm.AdministradoresAtivos = CalcularKPI(
                    administradoresAtivosHoje,
                    administradoresAtivosOntem,
                    "absoluto",
                    ObterSparklineAdministradores(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Administradores Ativos: {administradoresAtivosHoje} (Limite recomendado: 5, Ontem: {administradoresAtivosOntem})");
                
                // ========== NOVO: KPI - CONTRATOS ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Contratos...");
                
                // Total de contratos ativos (não excluídos)
                var totalContratos = _contratoRepository
                    .Buscar(x => x.Cliente == cliente && !x.DTExclusao.HasValue)
                    .Count();
                
                // Contratos vencidos (DTFinalVigencia < hoje)
                var contratosVencidosHoje = _contratoRepository
                    .Buscar(x => x.Cliente == cliente 
                                 && !x.DTExclusao.HasValue 
                                 && x.DTFinalVigencia.HasValue 
                                 && x.DTFinalVigencia.Value.Date < hoje)
                    .Count();
                
                var contratosVencidosOntem = _contratoRepository
                    .Buscar(x => x.Cliente == cliente 
                                 && !x.DTExclusao.HasValue 
                                 && x.DTFinalVigencia.HasValue 
                                 && x.DTFinalVigencia.Value.Date < ontem)
                    .Count();
                
                vm.TotalContratos = totalContratos;
                vm.ContratosVencidos = CalcularKPI(
                    contratosVencidosHoje,
                    contratosVencidosOntem,
                    "absoluto",
                    ObterSparklineContratosVencidos(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Contratos: Total={totalContratos}, Vencidos={contratosVencidosHoje} (Ontem: {contratosVencidosOntem})");
                
                // ========== NOVO: KPI - LAUDOS (SINISTROS) ==========
                Console.WriteLine("[DASHBOARD] Calculando KPI: Laudos/Sinistros...");
                
                // Total de laudos ativos no sistema (não excluídos)
                var totalLaudos = _laudoRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo)
                    .Count();
                
                // Laudos em análise (sem data de laudo = ainda não finalizados)
                var laudosEmAnaliseHoje = _laudoRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo && !x.Dtlaudo.HasValue)
                    .Count();
                
                var laudosEmAnaliseOntem = _laudoRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo && !x.Dtlaudo.HasValue)
                    .Count(); // Para laudos, usamos snapshot atual pois não temos histórico temporal
                
                // Laudos encerrados (com data de laudo)
                var laudosEncerradosHoje = _laudoRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo && x.Dtlaudo.HasValue)
                    .Count();
                
                var laudosEncerradosOntem = _laudoRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo && x.Dtlaudo.HasValue)
                    .Count();
                
                vm.TotalLaudos = totalLaudos;
                vm.LaudosEmAnalise = laudosEmAnaliseHoje;
                vm.LaudosEncerrados = CalcularKPI(
                    laudosEncerradosHoje,
                    laudosEncerradosOntem,
                    "absoluto",
                    ObterSparklineLaudosEncerrados(cliente, 7)
                );
                
                Console.WriteLine($"[DASHBOARD] Laudos: Total={totalLaudos}, Em Análise={laudosEmAnaliseHoje}, Encerrados={laudosEncerradosHoje}");
                
                // ========== NOVO: KPIs DE ASSOCIAÇÕES COLABORADOR-RECURSO ==========
                Console.WriteLine("[DASHBOARD] Calculando KPIs de associações colaborador-recurso...");

                var hojeData = TimeZoneMapper.GetDateTimeNow().Date;

                // 1) Recursos associados HOJE (equipamentos entregues e não devolvidos + linhas sem devolução)
                var itensAtivosEquip = (from evm in _requisicaoequipamentosvmsRepository.Query()
                                        join r in _requisicaoRepository.Query() on evm.Requisicao equals r.Id
                                        where r.Cliente == cliente
                                              && r.Colaboradorfinal.HasValue
                                              && evm.Equipamentostatus.HasValue && evm.Equipamentostatus.Value == 4
                                              && !evm.Dtdevolucao.HasValue
                                        select new { r.Colaboradorfinal }).ToList();

                var itensAtivosLinhas = (from it in _requisicaoItensRepository.Query()
                                         join r in _requisicaoRepository.Query() on it.Requisicao equals r.Id
                                         where r.Cliente == cliente
                                               && r.Colaboradorfinal.HasValue
                                               && it.Linhatelefonica.HasValue && it.Linhatelefonica.Value > 0
                                               && !it.Dtdevolucao.HasValue
                                         select new { r.Colaboradorfinal }).ToList();

                var recursosAssociadosHoje = itensAtivosEquip.Count + itensAtivosLinhas.Count;
                var colaboradoresComRecursosHoje = itensAtivosEquip.Select(x => x.Colaboradorfinal.Value)
                    .Concat(itensAtivosLinhas.Select(x => x.Colaboradorfinal.Value))
                    .Distinct().Count();

                // 2) Colaboradores ATIVOS hoje (excluindo desligados e com inventário forçado pendente)
                var colaboradoresAtivosHoje = _colaboradorRepository
                    .Buscar(x => x.Cliente == cliente && (!x.Dtdemissao.HasValue || x.Dtdemissao.Value.Date > hojeData))
                    .Select(x => x.Id)
                    .ToList();
                
                // Buscar colaboradores com inventário forçado pendente
                var colaboradoresComInventarioPendente = _contestacaoRepository
                    .Buscar(x => x.TipoContestacao == "inventario_forcado" && x.Status == "pendente")
                    .Include(x => x.Colaborador)
                    .Where(x => x.Colaborador != null && x.Colaborador.Cliente == cliente)
                    .Select(x => x.Colaborador.Id)
                    .Distinct()
                    .ToList();
                
                Console.WriteLine($"[DASHBOARD] Colaboradores com inventário forçado pendente: {colaboradoresComInventarioPendente.Count}");
                
                // Filtrar colaboradores ativos removendo os que têm inventário pendente
                var colaboradoresAtivosElegiveisHoje = colaboradoresAtivosHoje
                    .Where(id => !colaboradoresComInventarioPendente.Contains(id))
                    .ToList();
                
                Console.WriteLine($"[DASHBOARD] Colaboradores ativos (total): {colaboradoresAtivosHoje.Count}, após filtrar inventário pendente: {colaboradoresAtivosElegiveisHoje.Count}");
                
                var totalColaboradoresAtivosHoje = colaboradoresAtivosElegiveisHoje.Count;
                var colaboradoresSemRecursoHoje = Math.Max(totalColaboradoresAtivosHoje - colaboradoresComRecursosHoje, 0);

                // 3) Valores de ONTEM para variação
                var recursosAssociadosOntemEquip = (from evm in _requisicaoequipamentosvmsRepository.Query()
                                                    join r in _requisicaoRepository.Query() on evm.Requisicao equals r.Id
                                                    where r.Cliente == cliente
                                                          && r.Colaboradorfinal.HasValue
                                                          && evm.Equipamentostatus.HasValue && evm.Equipamentostatus.Value == 4
                                                          && !evm.Dtdevolucao.HasValue
                                                          && evm.Dtentrega.HasValue && evm.Dtentrega.Value < hojeData
                                                    select 1).Count();

                var recursosAssociadosOntemLinhas = (from it in _requisicaoItensRepository.Query()
                                                     join r in _requisicaoRepository.Query() on it.Requisicao equals r.Id
                                                     where r.Cliente == cliente
                                                           && r.Colaboradorfinal.HasValue
                                                           && it.Linhatelefonica.HasValue && it.Linhatelefonica.Value > 0
                                                           && !it.Dtdevolucao.HasValue
                                                           && it.Dtentrega.HasValue && it.Dtentrega.Value < hojeData
                                                     select 1).Count();

                var recursosAssociadosOntem = recursosAssociadosOntemEquip + recursosAssociadosOntemLinhas;

                var colaboradoresComRecursosOntemEquip = (from evm in _requisicaoequipamentosvmsRepository.Query()
                                                           join r in _requisicaoRepository.Query() on evm.Requisicao equals r.Id
                                                           where r.Cliente == cliente
                                                                 && r.Colaboradorfinal.HasValue
                                                                 && evm.Equipamentostatus.HasValue && evm.Equipamentostatus.Value == 4
                                                                 && !evm.Dtdevolucao.HasValue
                                                                 && evm.Dtentrega.HasValue && evm.Dtentrega.Value < hojeData
                                                           select r.Colaboradorfinal.Value).Distinct();

                var colaboradoresComRecursosOntemLinhas = (from it in _requisicaoItensRepository.Query()
                                                            join r in _requisicaoRepository.Query() on it.Requisicao equals r.Id
                                                            where r.Cliente == cliente
                                                                  && r.Colaboradorfinal.HasValue
                                                                  && it.Linhatelefonica.HasValue && it.Linhatelefonica.Value > 0
                                                                  && !it.Dtdevolucao.HasValue
                                                                  && it.Dtentrega.HasValue && it.Dtentrega.Value < hojeData
                                                            select r.Colaboradorfinal.Value).Distinct();

                var colaboradoresComRecursosOntem = colaboradoresComRecursosOntemEquip
                    .Concat(colaboradoresComRecursosOntemLinhas)
                    .Distinct().Count();

                var colaboradoresAtivosOntemIds = _colaboradorRepository
                    .Buscar(x => x.Cliente == cliente && (!x.Dtdemissao.HasValue || x.Dtdemissao.Value.Date > hojeData.AddDays(-1)))
                    .Select(x => x.Id)
                    .ToList();
                
                // Filtrar colaboradores ativos de ontem removendo os que têm inventário pendente
                var colaboradoresAtivosElegiveisOntem = colaboradoresAtivosOntemIds
                    .Where(id => !colaboradoresComInventarioPendente.Contains(id))
                    .Count();
                
                var colaboradoresSemRecursoOntem = Math.Max(colaboradoresAtivosElegiveisOntem - colaboradoresComRecursosOntem, 0);

                vm.RecursosAssociados = CalcularKPI(recursosAssociadosHoje, recursosAssociadosOntem, "absoluto", new List<int>());
                vm.ColaboradoresComRecursos = CalcularKPI(colaboradoresComRecursosHoje, colaboradoresComRecursosOntem, "absoluto", new List<int>());
                vm.ColaboradoresSemRecurso = CalcularKPI(colaboradoresSemRecursoHoje, colaboradoresSemRecursoOntem, "absoluto", new List<int>());

                Console.WriteLine($"[DASHBOARD] Recursos Associados: {recursosAssociadosHoje} (ontem: {recursosAssociadosOntem})");
                Console.WriteLine($"[DASHBOARD] Colaboradores com Recurso: {colaboradoresComRecursosHoje} (ontem: {colaboradoresComRecursosOntem})");
                Console.WriteLine($"[DASHBOARD] Colaboradores sem Recurso: {colaboradoresSemRecursoHoje} (ontem: {colaboradoresSemRecursoOntem})");

                // ========== NOVO: MÉTRICAS DE SINALIZAÇÕES ==========
                Console.WriteLine("[DASHBOARD] Gerando métricas de sinalizações...");
                vm.Sinalizacoes = ObterMetricasSinalizacoes(cliente);
                
                // ========== NOVO: MÉTRICAS DE AUDITORIA ==========
                Console.WriteLine("[DASHBOARD] Gerando métricas de auditoria...");
                vm.Auditoria = ObterMetricasAuditoria(cliente);
                
                // ========== NOVO: MÉTRICAS DE CAMPANHAS ==========
                Console.WriteLine("[DASHBOARD] Gerando métricas de campanhas...");
                vm.MetricasCampanhas = ObterMetricasCampanhas(cliente);
                
                // ========== NOVO: NOTIFICAÇÕES ==========
                Console.WriteLine("[DASHBOARD] Gerando notificações...");
                vm.Notificacoes = GerarNotificacoes(cliente, vm);
                
                // ========== NOVO: MÉTRICAS DE MOVIMENTAÇÕES (DADOS REAIS) ==========
                Console.WriteLine("[DASHBOARD] Calculando métricas de movimentações...");
                
                // Movimentações do dia anterior para comparação (entregas + devoluções)
                var entregasOntem = (from evm in _requisicaoequipamentosvmsRepository.Query() 
                                    join r in _requisicaoRepository.Query() on evm.Requisicao equals r.Id 
                                    where r.Cliente == cliente && 
                                          evm.Dtentrega.HasValue && evm.Dtentrega.Value.Date == ontem
                                    select evm).Count();
                
                var devolucoesOntem = (from evm in _requisicaoequipamentosvmsRepository.Query() 
                                      join r in _requisicaoRepository.Query() on evm.Requisicao equals r.Id 
                                      where r.Cliente == cliente && 
                                            evm.Dtdevolucao.HasValue && evm.Dtdevolucao.Value.Date == ontem
                                      select evm).Count();
                
                vm.QtdeAtivosMovimentadoDiaAnterior = entregasOntem + devolucoesOntem;
                                                        
                Console.WriteLine($"[DASHBOARD] Movimentações HOJE: {vm.QtdeAtivosMovimentadoDia}, ONTEM: {vm.QtdeAtivosMovimentadoDiaAnterior}");
                
                // Distribuição de movimentações por tipo (dados reais baseados nos itens da requisição)
                vm.DistribuicaoMovimentacoes = new DistribuicaoMovimentacoesVM();
                
                // Reutilizar os valores já calculados acima
                vm.DistribuicaoMovimentacoes.Entregas = entregasHoje;
                vm.DistribuicaoMovimentacoes.Devolucoes = devolucoesHoje;
                
                // Outros: sempre 0 pois agora contamos entregas + devoluções separadamente
                // (antes contávamos equipamentos únicos, o que poderia gerar diferença)
                vm.DistribuicaoMovimentacoes.Outros = 0;
                    
                Console.WriteLine($"[DASHBOARD] Distribuição: {vm.DistribuicaoMovimentacoes.Entregas} entregas, " +
                                  $"{vm.DistribuicaoMovimentacoes.Devolucoes} devoluções, " +
                                  $"{vm.DistribuicaoMovimentacoes.Outros} outros");
                
                // ========== NOVO: MÉTRICAS DE REQUISIÇÕES (DADOS REAIS) ==========
                Console.WriteLine("[DASHBOARD] Calculando métricas de requisições...");
                
                vm.MetricasRequisicoes = new MetricasRequisicoesVM();
                
                // Total de requisitados (equipamentos por status)
                vm.MetricasRequisicoes.TotalRequisitados = (int)(vm.EquipamentosPorStatus?
                    .Sum(x => x.Requisitado) ?? 0);
                
                // Urgentes: requisições criadas nos últimos 3 dias ainda não processadas
                vm.MetricasRequisicoes.Urgentes = _requisicaoRepository
                    .Buscar(x => x.Cliente == cliente && 
                                 x.Dtsolicitacao.HasValue && x.Dtsolicitacao.Value >= hoje.AddDays(-3) && 
                                 (!x.Dtprocessamento.HasValue || x.Dtprocessamento.Value > hoje))
                    .Count();
                
                // Pendentes: requisições ainda não processadas (mais antigas que 3 dias)
                vm.MetricasRequisicoes.Pendentes = _requisicaoRepository
                    .Buscar(x => x.Cliente == cliente && 
                                 x.Dtsolicitacao.HasValue && x.Dtsolicitacao.Value < hoje.AddDays(-3) && 
                                 (!x.Dtprocessamento.HasValue || x.Dtprocessamento.Value > hoje))
                    .Count();
                    
                Console.WriteLine($"[DASHBOARD] Requisições: {vm.MetricasRequisicoes.TotalRequisitados} total, " +
                                  $"{vm.MetricasRequisicoes.Urgentes} urgentes, " +
                                  $"{vm.MetricasRequisicoes.Pendentes} pendentes");
                
                // ========== NOVO: MÉTRICAS DE DEVOLUÇÕES (DADOS REAIS) ==========
                Console.WriteLine("[DASHBOARD] Calculando métricas de devoluções...");
                
                vm.MetricasDevolvidas = new MetricasDevolvidasVM();
                
                // Total de devolvidos (equipamentos por status)
                vm.MetricasDevolvidas.TotalDevolvidos = (int)(vm.EquipamentosPorStatus?
                    .Sum(x => x.Devolvido) ?? 0);
                
                // Vencidos: devoluções programadas com data já passada
                vm.MetricasDevolvidas.Vencidos = vm.DevolucoesProgramadas?
                    .Count(x => x.Dtprogramadaretorno.HasValue && x.Dtprogramadaretorno.Value < hoje) ?? 0;
                
                // Próximos: devoluções programadas com data futura nos próximos 7 dias
                vm.MetricasDevolvidas.Proximos = vm.DevolucoesProgramadas?
                    .Count(x => x.Dtprogramadaretorno.HasValue && 
                                x.Dtprogramadaretorno.Value >= hoje && 
                                x.Dtprogramadaretorno.Value <= hoje.AddDays(7)) ?? 0;
                                
                Console.WriteLine($"[DASHBOARD] Devoluções: {vm.MetricasDevolvidas.TotalDevolvidos} total, " +
                                  $"{vm.MetricasDevolvidas.Vencidos} vencidos, " +
                                  $"{vm.MetricasDevolvidas.Proximos} próximos");
                
                Console.WriteLine($"[DASHBOARD] ✅ Dashboard gerado com sucesso!");
                Console.WriteLine($"[DASHBOARD]    - {vm.Sinalizacoes.Total} sinalizações totais");
                Console.WriteLine($"[DASHBOARD]    - {vm.Auditoria.AcessosHoje} acessos hoje");
                Console.WriteLine($"[DASHBOARD]    - {vm.Notificacoes.Count} notificações");
                Console.WriteLine($"[DASHBOARD]    - {vm.QtdeAtivosMovimentadoDia} movimentações hoje vs {vm.QtdeAtivosMovimentadoDiaAnterior} ontem");

            return vm;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DASHBOARD] ❌ Erro ao gerar dashboard: {ex.Message}");
                Console.WriteLine($"[DASHBOARD] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public (List<Vwlaudo>, List<LaudoVM>) LaudosComValor(int cliente, int empresa, int cc)
        {
            var laudos = _vwLaudoRepository
                        .Buscar(x => x.Cliente == cliente && x.Valormanutencao != null && 
                                ((empresa != 0) ? x.Empresa == empresa : 1 == 1) &&
                                ((cc != 0) ? x.Centrocusto == cc : 1 == 1)
                              ).OrderByDescending(x => x.Dtlaudo).ToList();
            //return laudos;

            var laudoAgrupado = (from l in _vwLaudoRepository.Query()
                                 where l.Cliente == cliente && l.Valormanutencao != null &&
                                    ((empresa != 0) ? l.Empresa == empresa : 1 == 1) &&
                                    ((cc != 0) ? l.Centrocusto == cc : 1 == 1)
                                 group l by new { l.Empresanome, l.Centrocustonome }
                                 into grp
                                 select new LaudoVM()
                                 {
                                     Empresa = grp.Key.Empresanome,
                                     CentroCusto = grp.Key.Centrocustonome,
                                     Quantidade = grp.Count(),
                                     //Valor = grp.Sum(l => l.Valormanutencao)
                                     Valor = grp.Sum(l => l.Valormanutencao).Value
                                 }).ToList();

            return (laudos, laudoAgrupado);
            //var laudoAgrupado = _vwLaudoRepository.Buscar(x => x.Cliente == cliente && x.Valormanutencao != null).GroupBy(x => new {x.Empresanome, x.Centrocustonome})
            //    .Select(x => new LaudoVM()
            //    {
            //        Empresa = x.
            //    })
        }

        public List<Equipamentohistoricovm> ListarLinhasTelefonicas(string pesquisa, int cliente)
        {
            // ✅ NOVO: Buscar linhas telefônicas via Requisicoesiten
            var linhasHist = _requisicaoItensRepository.Buscar(x => x.Linhatelefonica.HasValue && x.Linhatelefonica > 0)
                .Include(x => x.LinhatelefonicaNavigation)
                .ThenInclude(x => x.PlanoNavigation)
                .ThenInclude(x => x.ContratoNavigation)
                .ThenInclude(x => x.OperadoraNavigation)
                .ToList();

            // ✅ NOVO: Converter para Equipamentohistoricovm para compatibilidade
            var linhasVM = linhasHist.Select(lt => new Equipamentohistoricovm
            {
                Id = lt.Id,
                Equipamentoid = lt.Linhatelefonica,
                Tipoequipamento = "Linha Telefônica",
                Fabricante = lt.LinhatelefonicaNavigation?.PlanoNavigation?.ContratoNavigation?.OperadoraNavigation?.Nome ?? "N/A",
                Modelo = lt.LinhatelefonicaNavigation?.Numero.ToString() ?? "N/A",
                Numeroserie = lt.LinhatelefonicaNavigation?.Numero.ToString(),
                Patrimonio = lt.LinhatelefonicaNavigation?.PlanoNavigation?.ContratoNavigation?.OperadoraNavigation?.Nome ?? "N/A",
                Equipamentostatusid = lt.Dtdevolucao.HasValue ? 5 : 4, // 4 = Entregue, 5 = Devolvido
                Equipamentostatus = lt.Dtdevolucao.HasValue ? "Devolvido" : "Entregue",
                Colaboradorid = lt.Usuarioentrega,
                Colaborador = "Sistema", // Pode ser melhorado buscando nome do usuário
                Dtregistro = lt.Dtentrega ?? DateTime.Now,
                Usuarioid = lt.Usuarioentrega,
                Usuario = "Sistema", // Pode ser melhorado buscando nome do usuário
                Tecnicoresponsavelid = lt.Usuariodevolucao,
                Tecnicoresponsavel = "Sistema" // Pode ser melhorado buscando nome do usuário
            }).ToList();

            // ✅ CORREÇÃO: Remover duplicatas baseado no Equipamentoid (ID da linha)
            linhasVM = linhasVM
                .GroupBy(x => x.Equipamentoid)
                .Select(g => g.OrderByDescending(x => x.Dtregistro).First())
                .ToList();

            // ✅ NOVO: Filtrar por pesquisa se fornecida
            if (!string.IsNullOrEmpty(pesquisa) && pesquisa.ToLower() != "null")
            {
                var pesquisaLower = pesquisa.ToLower();
                linhasVM = linhasVM.Where(l => 
                    l.Fabricante?.ToLower().Contains(pesquisaLower) == true ||
                    l.Numeroserie?.ToLower().Contains(pesquisaLower) == true ||
                    l.Patrimonio?.ToLower().Contains(pesquisaLower) == true
                ).ToList();
            }

            return linhasVM.OrderByDescending(x => x.Dtregistro).ToList();
        }

        public List<LogAcessoVM> ConsultarLogsAcesso(LogAcessoFiltroVM filtros)
        {
            try
            {
                // Parse das datas
                DateTime dataInicio = DateTime.ParseExact(filtros.DataInicio, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime dataFim = DateTime.ParseExact(filtros.DataFim, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                dataFim = dataFim.AddDays(1).AddSeconds(-1); // Incluir o dia inteiro

                // Buscar logs com filtros
                var query = _patrimonioLogAcessoRepository.Buscar(x => 
                    x.CreatedAt >= dataInicio && 
                    x.CreatedAt <= dataFim
                );

                // Filtro por tipo de acesso
                if (!string.IsNullOrEmpty(filtros.TipoAcesso))
                {
                    query = query.Where(x => x.TipoAcesso == filtros.TipoAcesso);
                }

                // Filtro por CPF consultado
                if (!string.IsNullOrEmpty(filtros.CpfConsultado))
                {
                    query = query.Where(x => x.CpfConsultado.Contains(filtros.CpfConsultado));
                }

                // Incluir informações do colaborador
                query = query.Include(x => x.Colaborador);

                var logs = query.OrderByDescending(x => x.CreatedAt)
                    .Select(log => new LogAcessoVM
                    {
                        Id = log.Id,
                        TipoAcesso = log.TipoAcesso,
                        ColaboradorId = log.ColaboradorId,
                        ColaboradorNome = log.Colaborador != null ? log.Colaborador.Nome : null,
                        CpfConsultado = log.CpfConsultado,
                        IpAddress = log.IpAddress,
                        UserAgent = log.UserAgent,
                        DadosConsultados = log.DadosConsultados,
                        Sucesso = log.Sucesso,
                        MensagemErro = log.MensagemErro,
                        CreatedAt = log.CreatedAt
                    })
                    .ToList();

                return logs;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao consultar logs de acesso: {ex.Message}", ex);
            }
        }

        public List<GarantiaVM> ConsultarGarantias(GarantiaFiltroVM filtros)
        {
            try
            {
                DateTime hoje = DateTime.Today;
                DateTime vence30 = hoje.AddDays(30);
                DateTime vence90 = hoje.AddDays(90);
                DateTime vence180 = hoje.AddDays(180);

                // Buscar todos os equipamentos ativos do cliente
                var query = _equipamentoRepository.Buscar(x => 
                    x.Ativo && 
                    x.Cliente == filtros.ClienteId
                );

                // Incluir informações relacionadas
                query = query
                    .Include(x => x.TipoequipamentoNavigation)
                    .Include(x => x.FabricanteNavigation)
                    .Include(x => x.ModeloNavigation);

                // Aplicar filtros
                if (!string.IsNullOrEmpty(filtros.Patrimonio))
                {
                    query = query.Where(x => x.Patrimonio != null && x.Patrimonio.Contains(filtros.Patrimonio));
                }

                if (!string.IsNullOrEmpty(filtros.TipoEquipamento))
                {
                    query = query.Where(x => x.TipoequipamentoNavigation != null && 
                        x.TipoequipamentoNavigation.Descricao.Contains(filtros.TipoEquipamento));
                }

                if (!string.IsNullOrEmpty(filtros.Fabricante))
                {
                    query = query.Where(x => x.FabricanteNavigation != null && 
                        x.FabricanteNavigation.Descricao.Contains(filtros.Fabricante));
                }

                var equipamentos = query.ToList();

                // Mapear para ViewModel e calcular status
                var garantiasVM = equipamentos.Select(eq =>
                {
                    int? diasRestantes = null;
                    string statusGarantia = "naoInformado";

                    if (eq.Dtlimitegarantia.HasValue)
                    {
                        diasRestantes = (int)(eq.Dtlimitegarantia.Value.Date - hoje).TotalDays;

                        if (diasRestantes < 0)
                        {
                            statusGarantia = "expiradas";
                        }
                        else if (diasRestantes <= 30)
                        {
                            statusGarantia = "vence30";
                        }
                        else if (diasRestantes <= 90)
                        {
                            statusGarantia = "vence90";
                        }
                        else if (diasRestantes <= 180)
                        {
                            statusGarantia = "vence180";
                        }
                        else
                        {
                            statusGarantia = "vigentes";
                        }
                    }

                    return new GarantiaVM
                    {
                        Id = eq.Id,
                        Patrimonio = eq.Patrimonio,
                        TipoEquipamento = eq.TipoequipamentoNavigation?.Descricao,
                        Fabricante = eq.FabricanteNavigation?.Descricao,
                        Modelo = eq.ModeloNavigation?.Descricao,
                        NumeroSerie = eq.Numeroserie,
                        DataGarantia = eq.Dtlimitegarantia,
                        DiasRestantes = diasRestantes,
                        StatusGarantia = statusGarantia
                    };
                }).ToList();

                // Filtrar por status se especificado
                if (!string.IsNullOrEmpty(filtros.StatusGarantia))
                {
                    garantiasVM = garantiasVM.Where(x => x.StatusGarantia == filtros.StatusGarantia).ToList();
                }

                // Ordenar: primeiro por status de risco (expiradas, vence30, vence90, vence180, vigentes, não informado)
                var ordem = new Dictionary<string, int>
                {
                    { "expiradas", 1 },
                    { "vence30", 2 },
                    { "vence90", 3 },
                    { "vence180", 4 },
                    { "vigentes", 5 },
                    { "naoInformado", 6 }
                };

                garantiasVM = garantiasVM
                    .OrderBy(x => ordem.ContainsKey(x.StatusGarantia) ? ordem[x.StatusGarantia] : 99)
                    .ThenBy(x => x.DiasRestantes)
                    .ToList();

                return garantiasVM;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao consultar garantias: {ex.Message}", ex);
            }
        }

        public RelatorioNaoConformidadeResultVM ConsultarNaoConformidadeElegibilidade(RelatorioNaoConformidadeFiltroVM filtros)
        {
            try
            {
                Console.WriteLine($"[NÃO CONFORMIDADE] Consultando não conformidades - Cliente: {filtros.Cliente}");

                // Query base usando a view vw_nao_conformidade_elegibilidade
                var sql = @"
                    SELECT 
                        colaborador_id,
                        colaborador_nome,
                        colaborador_cpf,
                        colaborador_email,
                        colaborador_cargo,
                        tipo_colaborador,
                        tipo_colaborador_descricao,
                        empresa_nome,
                        centro_custo,
                        localidade,
                        equipamento_id,
                        equipamento_patrimonio,
                        equipamento_serie,
                        tipo_equipamento_id,
                        tipo_equipamento_descricao,
                        categoria_equipamento,
                        fabricante,
                        modelo,
                        equipamento_status,
                        politica_id,
                        permite_acesso,
                        quantidade_maxima,
                        politica_observacoes,
                        quantidade_atual,
                        dt_geracao_relatorio
                    FROM vw_nao_conformidade_elegibilidade
                    WHERE 1=1";

                var parametros = new List<object>();

                // Aplicar filtro de cliente (obrigatório)
                sql += " AND cliente = {" + parametros.Count + "}";
                parametros.Add(filtros.Cliente);

                // Aplicar filtros opcionais
                if (!string.IsNullOrEmpty(filtros.TipoColaborador))
                {
                    sql += " AND tipo_colaborador = {" + parametros.Count + "}";
                    parametros.Add(filtros.TipoColaborador);
                }

                if (filtros.TipoEquipamentoId.HasValue)
                {
                    sql += " AND tipo_equipamento_id = {" + parametros.Count + "}";
                    parametros.Add(filtros.TipoEquipamentoId.Value);
                }

                if (!string.IsNullOrEmpty(filtros.ColaboradorNome))
                {
                    sql += " AND LOWER(colaborador_nome) LIKE {" + parametros.Count + "}";
                    parametros.Add($"%{filtros.ColaboradorNome.ToLower()}%");
                }

                sql += " ORDER BY colaborador_nome, tipo_equipamento_descricao";

                // Executar query usando DbContext
                var connection = _context.Database.GetDbConnection();
                var command = connection.CreateCommand();
                command.CommandText = sql;

                // Adicionar parâmetros
                for (int i = 0; i < parametros.Count; i++)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = $"p{i}";
                    param.Value = parametros[i];
                    command.Parameters.Add(param);
                }

                // Substituir placeholders {0}, {1} etc por @p0, @p1
                for (int i = 0; i < parametros.Count; i++)
                {
                    command.CommandText = command.CommandText.Replace($"{{{i}}}", $"@p{i}");
                }

                var registros = new List<RelatorioNaoConformidadeVM>();

                Console.WriteLine($"[NÃO CONFORMIDADE] SQL Final: {command.CommandText}");
                Console.WriteLine($"[NÃO CONFORMIDADE] Parâmetros: {string.Join(", ", parametros.Select((p, i) => $"@p{i}={p}"))}");

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    Console.WriteLine($"[NÃO CONFORMIDADE] Query executada, lendo resultados...");
                    int count = 0;
                    while (reader.Read())
                    {
                        count++;
                        var registro = new RelatorioNaoConformidadeVM
                        {
                            ColaboradorId = reader.GetInt32(reader.GetOrdinal("colaborador_id")),
                            ColaboradorNome = reader.IsDBNull(reader.GetOrdinal("colaborador_nome")) ? "" : reader.GetString(reader.GetOrdinal("colaborador_nome")),
                            ColaboradorCpf = reader.IsDBNull(reader.GetOrdinal("colaborador_cpf")) ? "" : reader.GetString(reader.GetOrdinal("colaborador_cpf")),
                            ColaboradorEmail = reader.IsDBNull(reader.GetOrdinal("colaborador_email")) ? "" : reader.GetString(reader.GetOrdinal("colaborador_email")),
                            ColaboradorCargo = reader.IsDBNull(reader.GetOrdinal("colaborador_cargo")) ? "" : reader.GetString(reader.GetOrdinal("colaborador_cargo")),
                            TipoColaborador = reader.IsDBNull(reader.GetOrdinal("tipo_colaborador")) ? "" : reader.GetString(reader.GetOrdinal("tipo_colaborador")),
                            TipoColaboradorDescricao = reader.IsDBNull(reader.GetOrdinal("tipo_colaborador_descricao")) ? "" : reader.GetString(reader.GetOrdinal("tipo_colaborador_descricao")),
                            EmpresaNome = reader.IsDBNull(reader.GetOrdinal("empresa_nome")) ? "" : reader.GetString(reader.GetOrdinal("empresa_nome")),
                            CentroCusto = reader.IsDBNull(reader.GetOrdinal("centro_custo")) ? "" : reader.GetString(reader.GetOrdinal("centro_custo")),
                            Localidade = reader.IsDBNull(reader.GetOrdinal("localidade")) ? "" : reader.GetString(reader.GetOrdinal("localidade")),
                            EquipamentoId = reader.GetInt32(reader.GetOrdinal("equipamento_id")),
                            EquipamentoPatrimonio = reader.IsDBNull(reader.GetOrdinal("equipamento_patrimonio")) ? "" : reader.GetString(reader.GetOrdinal("equipamento_patrimonio")),
                            EquipamentoSerie = reader.IsDBNull(reader.GetOrdinal("equipamento_serie")) ? "" : reader.GetString(reader.GetOrdinal("equipamento_serie")),
                            TipoEquipamentoId = reader.GetInt32(reader.GetOrdinal("tipo_equipamento_id")),
                            TipoEquipamentoDescricao = reader.IsDBNull(reader.GetOrdinal("tipo_equipamento_descricao")) ? "" : reader.GetString(reader.GetOrdinal("tipo_equipamento_descricao")),
                            CategoriaEquipamento = reader.IsDBNull(reader.GetOrdinal("categoria_equipamento")) ? null : reader.GetInt32(reader.GetOrdinal("categoria_equipamento")).ToString(),
                            Fabricante = reader.IsDBNull(reader.GetOrdinal("fabricante")) ? null : reader.GetString(reader.GetOrdinal("fabricante")),
                            Modelo = reader.IsDBNull(reader.GetOrdinal("modelo")) ? null : reader.GetString(reader.GetOrdinal("modelo")),
                            EquipamentoStatus = reader.IsDBNull(reader.GetOrdinal("equipamento_status")) ? null : reader.GetInt32(reader.GetOrdinal("equipamento_status")).ToString(),
                            PoliticaId = reader.IsDBNull(reader.GetOrdinal("politica_id")) ? null : reader.GetInt32(reader.GetOrdinal("politica_id")),
                            PermiteAcesso = reader.GetBoolean(reader.GetOrdinal("permite_acesso")),
                            QuantidadeMaxima = reader.IsDBNull(reader.GetOrdinal("quantidade_maxima")) ? null : reader.GetInt32(reader.GetOrdinal("quantidade_maxima")),
                            PoliticaObservacoes = reader.IsDBNull(reader.GetOrdinal("politica_observacoes")) ? null : reader.GetString(reader.GetOrdinal("politica_observacoes")),
                            QuantidadeAtual = (int)reader.GetInt64(reader.GetOrdinal("quantidade_atual")),
                            DtGeracaoRelatorio = reader.GetDateTime(reader.GetOrdinal("dt_geracao_relatorio"))
                        };

                        // Determinar motivo da não conformidade
                        if (!registro.PermiteAcesso)
                        {
                            // Se tem cargo específico, incluir na mensagem
                            if (!string.IsNullOrEmpty(registro.ColaboradorCargo))
                            {
                                registro.MotivoNaoConformidade = $"Colaborador com cargo '{registro.ColaboradorCargo}' não é elegível para '{registro.TipoEquipamentoDescricao}'";
                            }
                            else
                            {
                                registro.MotivoNaoConformidade = $"Tipo de colaborador '{registro.TipoColaboradorDescricao}' não é elegível para '{registro.TipoEquipamentoDescricao}'";
                            }
                        }
                        else if (registro.QuantidadeMaxima.HasValue && registro.QuantidadeAtual > registro.QuantidadeMaxima.Value)
                        {
                            registro.MotivoNaoConformidade = $"Quantidade excedida: possui {registro.QuantidadeAtual}, máximo permitido: {registro.QuantidadeMaxima.Value}";
                        }

                        registros.Add(registro);
                    }
                    Console.WriteLine($"[NÃO CONFORMIDADE] Total de registros lidos: {count}");
                }

                // Calcular estatísticas
                var resultado = new RelatorioNaoConformidadeResultVM
                {
                    Registros = registros,
                    TotalRegistros = registros.Count,
                    TotalColaboradores = registros.Select(x => x.ColaboradorId).Distinct().Count(),
                    TotalEquipamentos = registros.Select(x => x.EquipamentoId).Distinct().Count(),
                    PorTipoColaborador = registros
                        .GroupBy(x => x.TipoColaborador)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    PorTipoEquipamento = registros
                        .GroupBy(x => x.TipoEquipamentoDescricao)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                Console.WriteLine($"[NÃO CONFORMIDADE] ✅ Encontradas {resultado.TotalRegistros} não conformidades");
                Console.WriteLine($"[NÃO CONFORMIDADE]    - {resultado.TotalColaboradores} colaboradores afetados");
                Console.WriteLine($"[NÃO CONFORMIDADE]    - {resultado.TotalEquipamentos} equipamentos envolvidos");

                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NÃO CONFORMIDADE] ❌ Erro ao consultar não conformidades: {ex.Message}");
                Console.WriteLine($"[NÃO CONFORMIDADE] StackTrace: {ex.StackTrace}");
                throw new Exception($"Erro ao consultar não conformidades de elegibilidade: {ex.Message}", ex);
            }
        }

        // ========== MÉTODOS AUXILIARES PARA DASHBOARD ==========
        // Adicionados para suportar as novas métricas do dashboard

        private KPIPrincipal CalcularKPI(int valorAtual, int valorAnterior, string tipoVariacao, List<int> sparkline)
        {
            var kpi = new KPIPrincipal
            {
                Valor = valorAtual,
                ValorAnterior = valorAnterior,
                TipoVariacao = tipoVariacao,
                SparklineUltimos7Dias = sparkline
            };
            
            // Calcular variação
            if (valorAnterior > 0)
            {
                if (tipoVariacao == "percentual")
                {
                    kpi.Variacao = Math.Round(((valorAtual - valorAnterior) / (decimal)valorAnterior) * 100, 1);
                }
                else
                {
                    kpi.Variacao = valorAtual - valorAnterior;
                }
            }
            else
            {
                kpi.Variacao = 0;
            }
            
            // Definir tendência
            if (kpi.Variacao > 0)
                kpi.Tendencia = "alta";
            else if (kpi.Variacao < 0)
                kpi.Tendencia = "baixa";
            else
                kpi.Tendencia = "estavel";
                
            return kpi;
        }

        private List<int> ObterSparklineRecursos(int cliente, int dias)
        {
            var sparkline = new List<int>();
            var hoje = DateTime.Today;
            
            for (int i = dias - 1; i >= 0; i--)
            {
                var data = hoje.AddDays(-i);
                var count = _equipamentoRepository
                    .Buscar(x => x.Cliente == cliente && x.Ativo && x.Dtcadastro < data.AddDays(1))
                    .Count();
                sparkline.Add(count);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineSinalizacoes(int cliente, int dias)
        {
            var sparkline = new List<int>();
            var hoje = DateTime.Today;
            
            for (int i = dias - 1; i >= 0; i--)
            {
                var data = hoje.AddDays(-i);
                var count = _sinalizacaoRepository
                    .Buscar(x => x.DataSinalizacao.Date == data)
                    .Include(x => x.Colaborador)
                    .Where(x => x.Colaborador.Cliente == cliente)
                    .Count();
                sparkline.Add(count);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineDevolucoes(int cliente, int dias)
        {
            var sparkline = new List<int>();
            var hoje = DateTime.Today;
            
            for (int i = dias - 1; i >= 0; i--)
            {
                var data = hoje.AddDays(-i);
                var count = _vwdevolucaoprogramadumRepository
                    .Buscar(x => x.Cliente == cliente && x.Dtprogramadaretorno.HasValue && x.Dtprogramadaretorno.Value.Date == data)
                    .Count();
                sparkline.Add(count);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineContestacoes(int cliente, int dias)
        {
            var sparkline = new List<int>();
            var hoje = DateTime.Today;
            
            for (int i = dias - 1; i >= 0; i--)
            {
                var data = hoje.AddDays(-i);
                var count = _contestacaoRepository
                    .Buscar(x => x.DataContestacao.Date == data)
                    .Include(x => x.Colaborador)
                    .Where(x => x.Colaborador != null && x.Colaborador.Cliente == cliente)
                    .Count();
                sparkline.Add(count);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineNaoConformidade(int cliente, int dias)
        {
            var sparkline = new List<int>();
            
            // Como a view de não conformidade é dinâmica (calcula em tempo real),
            // para o sparkline vamos usar o mesmo valor (snapshot atual)
            // Em uma implementação futura, poderia haver uma tabela de histórico
            var sqlCount = @"
                SELECT COUNT(*) 
                FROM vw_nao_conformidade_elegibilidade 
                WHERE cliente = {0}";
                
            int countAtual = 0;
            var connectionNC2 = _context.Database.GetDbConnection();
            var commandNC2 = connectionNC2.CreateCommand();
            commandNC2.CommandText = "SELECT COUNT(*) FROM vw_nao_conformidade_elegibilidade WHERE cliente = @p0";
            var paramNC2 = commandNC2.CreateParameter();
            paramNC2.ParameterName = "p0";
            paramNC2.Value = cliente;
            commandNC2.Parameters.Add(paramNC2);
            if (connectionNC2.State != System.Data.ConnectionState.Open)
                connectionNC2.Open();
            var scalarNC2 = commandNC2.ExecuteScalar();
            countAtual = Convert.ToInt32(scalarNC2);
            
            // Preencher com o valor atual para todos os dias
            for (int i = 0; i < dias; i++)
            {
                sparkline.Add(countAtual);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineGarantiasCriticas(int cliente, int dias)
        {
            var sparkline = new List<int>();
            var hoje = DateTime.Today;
            
            // Como as garantias são dados estáticos (data de garantia não muda diariamente),
            // vamos usar snapshot atual para todos os dias
            var equipamentosAtivos = _equipamentoRepository
                .Buscar(x => x.Ativo && x.Cliente == cliente)
                .ToList();
            
            var countAtual = equipamentosAtivos.Count(eq => 
                !eq.Dtlimitegarantia.HasValue || // Sem data de garantia
                eq.Dtlimitegarantia.Value < hoje  // Garantia expirada
            );
            
            // Preencher com o valor atual para todos os dias
            for (int i = 0; i < dias; i++)
            {
                sparkline.Add(countAtual);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineDescartados(int cliente, int dias)
        {
            var sparkline = new List<int>();
            
            // Como a view não tem histórico temporal, retornar o valor atual para todos os dias
            var countAtual = _vwequipamentosdetalhesRepository
                .Buscar(x => x.Cliente == cliente && x.Equipamentostatusid == 10)
                .Count();
            
            for (int i = 0; i < dias; i++)
            {
                sparkline.Add(countAtual);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklinePerdidos(int cliente, int dias)
        {
            var sparkline = new List<int>();
            
            // Como a view não tem histórico temporal, retornar o valor atual para todos os dias
            var countAtual = _vwequipamentosdetalhesRepository
                .Buscar(x => x.Cliente == cliente && 
                             (x.Equipamentostatusid == 5 || x.Equipamentostatusid == 8))
                .Count();
            
            for (int i = 0; i < dias; i++)
            {
                sparkline.Add(countAtual);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineAdministradores(int cliente, int dias)
        {
            var sparkline = new List<int>();
            var hoje = DateTime.Today;
            
            // Contar administradores ativos nos últimos N dias
            // Como o status de administrador não muda frequentemente, vamos usar snapshot atual
            var countAtual = _usuarioRepository
                .Buscar(x => x.Cliente == cliente && x.Ativo && x.Adm && !x.Su)
                .Count();
            
            // Preencher com o valor atual para todos os dias
            for (int i = 0; i < dias; i++)
            {
                sparkline.Add(countAtual);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineContratosVencidos(int cliente, int dias)
        {
            var sparkline = new List<int>();
            
            // Calcular contratos vencidos para cada dia
            for (int i = dias - 1; i >= 0; i--)
            {
                var dataReferencia = DateTime.Today.AddDays(-i);
                var contratosVencidos = _contratoRepository
                    .Buscar(x => x.Cliente == cliente 
                                 && !x.DTExclusao.HasValue 
                                 && x.DTFinalVigencia.HasValue 
                                 && x.DTFinalVigencia.Value.Date < dataReferencia)
                    .Count();
                
                sparkline.Add(contratosVencidos);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineLaudosEncerrados(int cliente, int dias)
        {
            var sparkline = new List<int>();
            
            // Como laudos não têm histórico temporal, usar snapshot atual de encerrados
            var countAtual = _laudoRepository
                .Buscar(x => x.Cliente == cliente && x.Ativo && x.Dtlaudo.HasValue)
                .Count();
            
            // Preencher com o valor atual para todos os dias
            for (int i = 0; i < dias; i++)
            {
                sparkline.Add(countAtual);
            }
            
            return sparkline;
        }

        private List<int> ObterSparklineAdesao(int cliente, int dias)
        {
            var sparkline = new List<int>();
            var hoje = DateTime.Today;
            
            for (int i = dias - 1; i >= 0; i--)
            {
                var data = hoje.AddDays(-i);
                var total = _requisicaoRepository
                    .Buscar(x => x.Cliente == cliente && x.Dtprocessamento.HasValue && x.Dtprocessamento.Value.Date == data)
                    .Count();
                    
                var assinados = _requisicaoRepository
                    .Buscar(x => x.Cliente == cliente && x.Dtprocessamento.HasValue && x.Dtprocessamento.Value.Date == data && x.Assinaturaeletronica)
                    .Count();
                    
                var taxa = total > 0 ? (int)Math.Round((assinados / (decimal)total) * 100) : 0;
                sparkline.Add(taxa);
            }
            
            return sparkline;
        }

        private MetricasSinalizacoes ObterMetricasSinalizacoes(int cliente)
        {
            var hoje = DateTime.Today;
            
            // Buscar todas as sinalizações do cliente
            var sinalizacoes = _sinalizacaoRepository
                .Buscar(x => true)
                .Include(x => x.Colaborador)
                .Where(x => x.Colaborador.Cliente == cliente)
                .ToList();
            
            var metricas = new MetricasSinalizacoes
            {
                // Totais por status
                Total = sinalizacoes.Count,
                Pendentes = sinalizacoes.Count(x => x.Status == "pendente"),
                EmInvestigacao = sinalizacoes.Count(x => x.Status == "em_investigacao"),
                Resolvidas = sinalizacoes.Count(x => x.Status == "resolvida"),
                ResolvidasHoje = sinalizacoes.Count(x => x.Status == "resolvida" && x.DataResolucao.HasValue && x.DataResolucao.Value.Date == hoje),
                Arquivadas = sinalizacoes.Count(x => x.Status == "arquivada"),
                
                // Totais por prioridade
                Criticas = sinalizacoes.Count(x => x.Prioridade == "critica"),
                Altas = sinalizacoes.Count(x => x.Prioridade == "alta"),
                Medias = sinalizacoes.Count(x => x.Prioridade == "media"),
                Baixas = sinalizacoes.Count(x => x.Prioridade == "baixa"),
                
                // Alertas
                PendentesHaMaisDe7Dias = sinalizacoes.Count(x => 
                    x.Status == "pendente" && (DateTime.Now - x.DataSinalizacao).TotalDays > 7),
                CriticasNaoAtendidas = sinalizacoes.Count(x => 
                    x.Status == "pendente" && x.Prioridade == "critica"),
                
                // Histórico de 7 dias
                Ultimos7Dias = ObterHistoricoSinalizacoes(sinalizacoes, 7),
                
                // Histórico de 30 dias
                Ultimos30Dias = ObterHistoricoSinalizacoes(sinalizacoes, 30)
            };
            
            return metricas;
        }

        private List<int> ObterHistoricoSinalizacoes(List<SinalizacaoSuspeita> sinalizacoes, int dias)
        {
            var historico = new List<int>();
            var hoje = DateTime.Today;
            
            for (int i = dias - 1; i >= 0; i--)
            {
                var data = hoje.AddDays(-i);
                var count = sinalizacoes.Count(x => x.DataSinalizacao.Date == data);
                historico.Add(count);
            }
            
            return historico;
        }

        private MetricasAuditoria ObterMetricasAuditoria(int cliente)
        {
            var hoje = DateTime.Today;
            var ontem = hoje.AddDays(-1);
            
            // Buscar logs através do relacionamento com Colaborador
            var logsHoje = _patrimonioLogAcessoRepository
                .Buscar(x => x.CreatedAt.Date == hoje)
                .Include(x => x.Colaborador)
                .Where(x => x.Colaborador != null && x.Colaborador.Cliente == cliente)
                .ToList();
                
            var logsOntem = _patrimonioLogAcessoRepository
                .Buscar(x => x.CreatedAt.Date == ontem)
                .Include(x => x.Colaborador)
                .Where(x => x.Colaborador != null && x.Colaborador.Cliente == cliente)
                .Count();
            
            var metricas = new MetricasAuditoria
            {
                AcessosHoje = logsHoje.Count,
                AcessosOntem = logsOntem,
                TentativasFalhas = logsHoje.Count(x => !x.Sucesso),
                UsuariosAtivosHoje = logsHoje.Where(x => x.ColaboradorId.HasValue).Select(x => x.ColaboradorId.Value).Distinct().Count(),
                ConsultasCPFHoje = logsHoje.Count(x => x.TipoAcesso == "consulta_cpf"),
                AcessosUltimos7Dias = ObterHistoricoAcessos(cliente, 7),
                TopUsuariosAtivos = ObterTopUsuariosAtivos(cliente, 5)
            };
            
            return metricas;
        }

        private List<int> ObterHistoricoAcessos(int cliente, int dias)
        {
            var historico = new List<int>();
            var hoje = DateTime.Today;
            
            for (int i = dias - 1; i >= 0; i--)
            {
                var data = hoje.AddDays(-i);
                var count = _patrimonioLogAcessoRepository
                    .Buscar(x => x.CreatedAt.Date == data)
                    .Include(x => x.Colaborador)
                    .Where(x => x.Colaborador != null && x.Colaborador.Cliente == cliente)
                    .Count();
                historico.Add(count);
            }
            
            return historico;
        }

        private List<UsuarioAtivo> ObterTopUsuariosAtivos(int cliente, int limite)
        {
            var hoje = DateTime.Today;
            
            var topColaboradores = _patrimonioLogAcessoRepository
                .Buscar(x => x.CreatedAt.Date == hoje && x.ColaboradorId.HasValue)
                .Include(x => x.Colaborador)
                .Where(x => x.Colaborador != null && x.Colaborador.Cliente == cliente)
                .GroupBy(x => x.ColaboradorId.Value)
                .Select(g => new { ColaboradorId = g.Key, Acessos = g.Count() })
                .OrderByDescending(x => x.Acessos)
                .Take(limite)
                .ToList();
            
            // Buscar nomes dos colaboradores
            var colaboradorIds = topColaboradores.Select(x => x.ColaboradorId).ToList();
            var colaboradores = _colaboradorRepository
                .Buscar(x => colaboradorIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x.Nome);
            
            return topColaboradores.Select(x => new UsuarioAtivo
            {
                Nome = colaboradores.ContainsKey(x.ColaboradorId) ? colaboradores[x.ColaboradorId] : "Desconhecido",
                Acessos = x.Acessos
            }).ToList();
        }

        private MetricasCampanhasVM ObterMetricasCampanhas(int cliente)
        {
            Console.WriteLine("[DASHBOARD] Iniciando cálculo de métricas de campanhas...");
            
            // Buscar todas as campanhas do cliente
            var todasCampanhas = _campanhaRepository
                .Buscar(x => x.Cliente == cliente)
                .ToList();
            
            Console.WriteLine($"[DASHBOARD] Total de campanhas encontradas: {todasCampanhas.Count}");
            
            // Calcular métricas
            var campanhasAbertas = todasCampanhas.Count(x => x.Status == 'A');
            var campanhasEncerradas = todasCampanhas.Count(x => x.Status == 'C');
            var campanhasAgendadas = todasCampanhas.Count(x => x.Status == 'G');
            
            // Somar colaboradores e assinaturas apenas de campanhas ativas
            var campanhasAtivas = todasCampanhas.Where(x => x.Status == 'A').ToList();
            var totalColaboradoresEmCampanhas = campanhasAtivas.Sum(x => x.TotalColaboradores);
            var totalAssinaturasRealizadas = campanhasAtivas.Sum(x => x.TotalAssinados);
            var totalAssinaturasPendentes = campanhasAtivas.Sum(x => x.TotalPendentes);
            
            // Calcular taxa média de adesão
            decimal taxaAdesaoMedia = 0;
            if (campanhasAtivas.Any())
            {
                var campanhasComAdesao = campanhasAtivas.Where(x => x.PercentualAdesao.HasValue).ToList();
                if (campanhasComAdesao.Any())
                {
                    taxaAdesaoMedia = (decimal)campanhasComAdesao.Average(x => x.PercentualAdesao.Value);
                }
            }
            
            // Buscar últimas 5 campanhas (ordenadas por data de criação desc)
            var ultimasCampanhas = todasCampanhas
                .OrderByDescending(x => x.DataCriacao)
                .Take(5)
                .Select(c => new CampanhaResumoSimples
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Status = c.Status,
                    StatusDescricao = ObterDescricaoStatus(c.Status),
                    TotalColaboradores = c.TotalColaboradores,
                    TotalAssinados = c.TotalAssinados,
                    TotalPendentes = c.TotalPendentes,
                    PercentualAdesao = c.PercentualAdesao,
                    DataInicio = c.DataInicio,
                    DataFim = c.DataFim
                })
                .ToList();
            
            var metricas = new MetricasCampanhasVM
            {
                CampanhasAbertas = campanhasAbertas,
                CampanhasEncerradas = campanhasEncerradas,
                CampanhasAgendadas = campanhasAgendadas,
                TotalColaboradoresEmCampanhas = totalColaboradoresEmCampanhas,
                TotalAssinaturasRealizadas = totalAssinaturasRealizadas,
                TotalAssinaturasPendentes = totalAssinaturasPendentes,
                TaxaAdesaoMedia = Math.Round(taxaAdesaoMedia, 1),
                UltimasCampanhas = ultimasCampanhas
            };
            
            Console.WriteLine($"[DASHBOARD] Métricas de Campanhas:");
            Console.WriteLine($"[DASHBOARD]   - Abertas: {campanhasAbertas}");
            Console.WriteLine($"[DASHBOARD]   - Encerradas: {campanhasEncerradas}");
            Console.WriteLine($"[DASHBOARD]   - Agendadas: {campanhasAgendadas}");
            Console.WriteLine($"[DASHBOARD]   - Colaboradores em campanhas: {totalColaboradoresEmCampanhas}");
            Console.WriteLine($"[DASHBOARD]   - Assinaturas realizadas: {totalAssinaturasRealizadas}");
            Console.WriteLine($"[DASHBOARD]   - Assinaturas pendentes: {totalAssinaturasPendentes}");
            Console.WriteLine($"[DASHBOARD]   - Taxa média de adesão: {taxaAdesaoMedia}%");
            
            return metricas;
        }
        
        private string ObterDescricaoStatus(char status)
        {
            switch (status)
            {
                case 'A': return "Ativa";
                case 'C': return "Concluída";
                case 'G': return "Agendada";
                case 'I': return "Inativa";
                default: return "Desconhecido";
            }
        }

        private List<NotificacaoDashboard> GerarNotificacoes(int cliente, DashboardWebVM vm)
        {
            var notificacoes = new List<NotificacaoDashboard>();
            int idCounter = 1;
            
            // 🔴 CRÍTICO: Sinalizações críticas não atendidas
            if (vm.Sinalizacoes.CriticasNaoAtendidas > 0)
            {
                notificacoes.Add(new NotificacaoDashboard
                {
                    Id = idCounter++,
                    Tipo = "critico",
                    Titulo = "Sinalizações Críticas Pendentes",
                    Mensagem = $"{vm.Sinalizacoes.CriticasNaoAtendidas} sinalização(ões) crítica(s) aguardando atendimento imediato",
                    DataHora = DateTime.Now,
                    Lida = false,
                    Link = "/relatorios/sinalizacoes-suspeitas",
                    Icone = "warning"
                });
            }
            
            // 🔴 CRÍTICO: Devoluções vencidas há mais de 7 dias
            var devolucoesVencidas = vm.DevolucoesProgramadas?
                .Count(x => x.Dtprogramadaretorno.HasValue && (DateTime.Now - x.Dtprogramadaretorno.Value).TotalDays > 7) ?? 0;
            
            if (devolucoesVencidas > 0)
            {
                notificacoes.Add(new NotificacaoDashboard
                {
                    Id = idCounter++,
                    Tipo = "critico",
                    Titulo = "Devoluções Vencidas",
                    Mensagem = $"{devolucoesVencidas} devolução(ões) vencida(s) há mais de 7 dias",
                    DataHora = DateTime.Now,
                    Lida = false,
                    Link = "/movimentacoes/devolucoes",
                    Icone = "schedule"
                });
            }
            
            // 🔴 CRÍTICO: Colaboradores desligados com recursos ativos
            if (vm.TotalDesligadosComRecursos > 0)
            {
                notificacoes.Add(new NotificacaoDashboard
                {
                    Id = idCounter++,
                    Tipo = "critico",
                    Titulo = "Desligados com Recursos Ativos",
                    Mensagem = $"{vm.TotalDesligadosComRecursos} colaborador(es) desligado(s) com {vm.TotalRecursosDesligados} recurso(s) ativo(s)",
                    DataHora = DateTime.Now,
                    Lida = false,
                    Link = "/dashboard#colaboradores-desligados",
                    Icone = "person_off"
                });
            }
            
            // 🟡 ATENÇÃO: Tentativas de acesso falhas
            if (vm.Auditoria.TentativasFalhas >= 5)
            {
                notificacoes.Add(new NotificacaoDashboard
                {
                    Id = idCounter++,
                    Tipo = "atencao",
                    Titulo = "Tentativas de Acesso Falhas",
                    Mensagem = $"{vm.Auditoria.TentativasFalhas} tentativas de acesso falharam hoje. Verifique segurança.",
                    DataHora = DateTime.Now,
                    Lida = false,
                    Link = "/relatorios/auditoria-acessos",
                    Icone = "security"
                });
            }
            
            // 🟡 ATENÇÃO: Sinalizações pendentes há mais de 7 dias
            if (vm.Sinalizacoes.PendentesHaMaisDe7Dias > 0)
            {
                notificacoes.Add(new NotificacaoDashboard
                {
                    Id = idCounter++,
                    Tipo = "atencao",
                    Titulo = "Sinalizações Antigas Pendentes",
                    Mensagem = $"{vm.Sinalizacoes.PendentesHaMaisDe7Dias} sinalização(ões) pendente(s) há mais de 7 dias",
                    DataHora = DateTime.Now,
                    Lida = false,
                    Link = "/relatorios/sinalizacoes-suspeitas",
                    Icone = "schedule"
                });
            }
            
            // 🔵 INFO: Novas sinalizações resolvidas hoje
            if (vm.Sinalizacoes.ResolvidasHoje > 0)
            {
                notificacoes.Add(new NotificacaoDashboard
                {
                    Id = idCounter++,
                    Tipo = "info",
                    Titulo = "Sinalizações Resolvidas Hoje",
                    Mensagem = $"{vm.Sinalizacoes.ResolvidasHoje} sinalização(ões) foi(ram) resolvida(s) hoje",
                    DataHora = DateTime.Now,
                    Lida = false,
                    Link = "/relatorios/sinalizacoes-suspeitas",
                    Icone = "check_circle"
                });
            }
            
            // 🔵 INFO: Recursos movimentados hoje
            if (vm.QtdeAtivosMovimentadoDia > 0)
            {
                notificacoes.Add(new NotificacaoDashboard
                {
                    Id = idCounter++,
                    Tipo = "info",
                    Titulo = "Movimentações de Hoje",
                    Mensagem = $"{vm.QtdeAtivosMovimentadoDia} recurso(s) foi(ram) movimentado(s) hoje",
                    DataHora = DateTime.Now,
                    Lida = false,
                    Link = "/movimentacoes/historico",
                    Icone = "sync"
                });
            }
            
            return notificacoes;
        }

        /// <summary>
        /// 👥 Consulta colaboradores que não possuem recursos associados
        /// </summary>
        public List<ColaboradoresSemRecursosVM> ConsultarColaboradoresSemRecursos(ColaboradoresSemRecursosFiltroVM filtros)
        {
            try
            {
                Console.WriteLine("[COLABORADORES SEM RECURSOS] 🔍 Iniciando consulta...");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS] ClienteId: {filtros.ClienteId}");
                
                // Buscar todos os colaboradores ativos do cliente
                var query = _colaboradorRepository.Buscar(c => 
                    c.Cliente == filtros.ClienteId && 
                    c.Situacao == "A" // Ativo
                );

                // Aplicar filtros
                if (!string.IsNullOrWhiteSpace(filtros.Cargo))
                {
                    var cargoLower = filtros.Cargo.ToLower();
                    query = query.Where(c => c.Cargo != null && c.Cargo.ToLower().Contains(cargoLower));
                    Console.WriteLine($"[COLABORADORES SEM RECURSOS] Filtrando por cargo (case-insensitive): {filtros.Cargo}");
                }

                if (!string.IsNullOrWhiteSpace(filtros.TipoColaborador) && filtros.TipoColaborador.Length > 0)
                {
                    char tipoChar = filtros.TipoColaborador[0];
                    query = query.Where(c => c.Tipocolaborador == tipoChar);
                    Console.WriteLine($"[COLABORADORES SEM RECURSOS] Filtrando por tipo: {filtros.TipoColaborador}");
                }

                if (filtros.Empresa.HasValue && filtros.Empresa.Value > 0)
                {
                    Console.WriteLine($"[COLABORADORES SEM RECURSOS] ⚠️ APLICANDO filtro por empresa: {filtros.Empresa.Value}");
                    query = query.Where(c => c.Empresa == filtros.Empresa.Value);
                }

                if (filtros.Localidade.HasValue && filtros.Localidade.Value > 0)
                {
                    Console.WriteLine($"[COLABORADORES SEM RECURSOS] ⚠️ APLICANDO filtro por localidade: {filtros.Localidade.Value}");
                    query = query.Where(c => c.Localidade == filtros.Localidade.Value);
                }

                if (filtros.CentroCusto.HasValue && filtros.CentroCusto.Value > 0)
                {
                    Console.WriteLine($"[COLABORADORES SEM RECURSOS] ⚠️ APLICANDO filtro por centro de custo: {filtros.CentroCusto.Value}");
                    query = query.Where(c => c.Centrocusto == filtros.CentroCusto.Value);
                }

                if (!string.IsNullOrWhiteSpace(filtros.Nome))
                {
                    var nomeLower = filtros.Nome.ToLower();
                    query = query.Where(c => c.Nome != null && c.Nome.ToLower().Contains(nomeLower));
                    Console.WriteLine($"[COLABORADORES SEM RECURSOS] Filtrando por nome (case-insensitive): {filtros.Nome}");
                }

                Console.WriteLine("🚀🚀🚀 [CÓDIGO NOVO V3.0 - USANDO REQUISIÇÕES + COMPARTILHADOS] 🚀🚀🚀");
                
                // 🎯 LÓGICA CORRETA: Um colaborador TEM recursos se:
                // 1) Possui equipamento EXCLUSIVO (requisição com Colaboradorfinal)
                // 2) Possui equipamento COMPARTILHADO (requisicoes_itens_compartilhados)
                
                // ✅ 1) Colaboradores com recursos EXCLUSIVOS
                var colaboradoresComRecursosExclusivos = _requisicaoItensRepository
                    .Buscar(ri => 
                        ri.Equipamento.HasValue &&
                        ri.Dtentrega.HasValue &&
                        !ri.Dtdevolucao.HasValue // Ainda não devolveu
                    )
                    .Include(ri => ri.EquipamentoNavigation)
                    .Include(ri => ri.RequisicaoNavigation)
                    .Where(ri => ri.EquipamentoNavigation.Ativo == true) // Equipamento ativo
                    .Select(ri => ri.RequisicaoNavigation.Colaboradorfinal)
                    .Where(colabId => colabId.HasValue)
                    .Select(colabId => colabId.Value)
                    .Distinct()
                    .ToList();
                
                Console.WriteLine($"✅ [RECURSOS EXCLUSIVOS] {colaboradoresComRecursosExclusivos.Count} colaboradores");
                
                // ✅ 2) Colaboradores com recursos COMPARTILHADOS
                var colaboradoresComRecursosCompartilhados = _context.Set<RequisicaoItemCompartilhado>()
                    .Where(ric => 
                        ric.Ativo == true &&
                        !ric.DataFim.HasValue // Compartilhamento ainda ativo
                    )
                    .Include(ric => ric.RequisicaoItem)
                        .ThenInclude(ri => ri.EquipamentoNavigation)
                    .Where(ric => 
                        ric.RequisicaoItem.Equipamento.HasValue &&
                        ric.RequisicaoItem.Dtentrega.HasValue &&
                        !ric.RequisicaoItem.Dtdevolucao.HasValue && // Equipamento não devolvido
                        ric.RequisicaoItem.EquipamentoNavigation.Ativo == true // Equipamento ativo
                    )
                    .Select(ric => ric.ColaboradorId)
                    .Distinct()
                    .ToList();
                
                Console.WriteLine($"✅ [RECURSOS COMPARTILHADOS] {colaboradoresComRecursosCompartilhados.Count} colaboradores");
                
                // 🎯 UNIÃO: Todos os colaboradores que TÊM recursos (exclusivos OU compartilhados)
                var colaboradoresComRecursos = colaboradoresComRecursosExclusivos
                    .Union(colaboradoresComRecursosCompartilhados)
                    .Distinct()
                    .ToList();
                
                Console.WriteLine($"🚀 [TOTAL] {colaboradoresComRecursos.Count} colaboradores COM recursos (exclusivos + compartilhados)");
                Console.WriteLine($"🚀 [DETALHES] Primeiros 20 IDs: {string.Join(", ", colaboradoresComRecursos.Take(20))}");

                // 🔍 DEBUG: Verificar colaboradores ANTES do filtro
                var colaboradoresAntesDoFiltro = query.ToList();
                Console.WriteLine($"[COLABORADORES SEM RECURSOS] 📊 Total de colaboradores ANTES do filtro: {colaboradoresAntesDoFiltro.Count}");
                
                // Pegar um exemplo específico para debug
                var exemploIsabelle = colaboradoresAntesDoFiltro.FirstOrDefault(c => c.Nome.Contains("Isabelle"));
                if (exemploIsabelle != null)
                {
                    Console.WriteLine($"[COLABORADORES SEM RECURSOS] 🔍 DEBUG Isabelle Felipe:");
                    Console.WriteLine($"  - Colaborador.Id: {exemploIsabelle.Id}");
                    Console.WriteLine($"  - Colaborador ID {exemploIsabelle.Id} está na lista de colaboradores COM recursos? {colaboradoresComRecursos.Contains(exemploIsabelle.Id)}");
                }

                // Filtrar colaboradores SEM recursos (comparando pelo ID do colaborador)
                var idsSemRecursos = colaboradoresAntesDoFiltro
                    .Where(c => !colaboradoresComRecursos.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToList();
                
                // Carregar as navegações
                var colaboradoresSemRecursos = query
                    .Where(c => idsSemRecursos.Contains(c.Id))
                    .Include(c => c.EmpresaNavigation)
                    .Include(c => c.LocalidadeNavigation)
                    .Include(c => c.CentrocustoNavigation)
                    .OrderBy(c => c.Nome)
                    .ToList();

                Console.WriteLine($"[COLABORADORES SEM RECURSOS] ✅ {colaboradoresSemRecursos.Count} colaboradores SEM recursos encontrados");

                // Mapear para ViewModel
                var resultado = colaboradoresSemRecursos.Select(c => new ColaboradoresSemRecursosVM
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Matricula = c.Matricula,
                    CargoDescricao = c.Cargo ?? "N/A",
                    EmpresaId = c.Empresa,
                    EmpresaDescricao = c.EmpresaNavigation?.Nome ?? "N/A",
                    LocalidadeId = c.Localidade,
                    LocalidadeDescricao = c.LocalidadeNavigation?.Descricao ?? "N/A",
                    CentroCustoId = c.Centrocusto,
                    CentroCustoDescricao = c.CentrocustoNavigation?.Nome ?? "N/A",
                    TipoColaboradorId = c.Tipocolaborador,
                    TipoColaboradorDescricao = ObterDescricaoTipoColaborador(c.Tipocolaborador),
                    DataAdmissao = c.Dtadmissao,
                    DataDemissao = c.Dtdemissao // 🚫 Data de demissão/desligamento
                }).ToList();

                Console.WriteLine($"[COLABORADORES SEM RECURSOS] ✅ Consulta finalizada: {resultado.Count} registros");

                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[COLABORADORES SEM RECURSOS] ❌ Erro: {ex.Message}");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Obter descrição amigável do tipo de colaborador
        /// </summary>
        private string ObterDescricaoTipoColaborador(char tipo)
        {
            return tipo switch
            {
                'F' => "Funcionário",
                'T' => "Terceiro",
                'C' => "Consultor",
                _ => tipo.ToString()
            };
        }

        /// <summary>
        /// Obter empresas que possuem colaboradores ativos
        /// </summary>
        public List<dynamic> ObterEmpresasComColaboradores(int clienteId)
        {
            try
            {
                Console.WriteLine($"[FILTROS COLABORADORES] Buscando empresas com colaboradores - Cliente: {clienteId}");
                
                var empresas = _colaboradorRepository
                    .Buscar(c => c.Cliente == clienteId && c.Situacao == "A" && c.Empresa > 0)
                    .Include(c => c.EmpresaNavigation)
                    .Where(c => c.EmpresaNavigation != null)
                    .GroupBy(c => new { c.Empresa, c.EmpresaNavigation.Nome })
                    .Select(g => new { id = g.Key.Empresa, nome = g.Key.Nome })
                    .OrderBy(e => e.nome)
                    .ToList<dynamic>();
                
                Console.WriteLine($"[FILTROS COLABORADORES] ✅ {empresas.Count} empresas encontradas");
                
                // Log detalhado
                foreach (var emp in empresas)
                {
                    Console.WriteLine($"  - Empresa ID: {emp.id}, Nome: {emp.nome}");
                }
                
                return empresas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILTROS COLABORADORES] ❌ Erro ao buscar empresas: {ex.Message}");
                Console.WriteLine($"[FILTROS COLABORADORES] StackTrace: {ex.StackTrace}");
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Obter localidades que possuem colaboradores ativos
        /// </summary>
        public List<dynamic> ObterLocalidadesComColaboradores(int clienteId)
        {
            try
            {
                Console.WriteLine($"[FILTROS COLABORADORES] Buscando localidades com colaboradores - Cliente: {clienteId}");
                
                var localidades = _colaboradorRepository
                    .Buscar(c => c.Cliente == clienteId && c.Situacao == "A" && c.Localidade > 0)
                    .Include(c => c.LocalidadeNavigation)
                    .Where(c => c.LocalidadeNavigation != null)
                    .GroupBy(c => new { c.Localidade, c.LocalidadeNavigation.Descricao })
                    .Select(g => new { id = g.Key.Localidade, descricao = g.Key.Descricao })
                    .OrderBy(l => l.descricao)
                    .ToList<dynamic>();
                
                Console.WriteLine($"[FILTROS COLABORADORES] ✅ {localidades.Count} localidades encontradas");
                
                // Log detalhado
                foreach (var loc in localidades)
                {
                    Console.WriteLine($"  - Localidade ID: {loc.id}, Descrição: {loc.descricao}");
                }
                
                return localidades;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILTROS COLABORADORES] ❌ Erro ao buscar localidades: {ex.Message}");
                Console.WriteLine($"[FILTROS COLABORADORES] StackTrace: {ex.StackTrace}");
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Obter centros de custo que possuem colaboradores ativos
        /// </summary>
        public List<dynamic> ObterCentrosCustoComColaboradores(int clienteId)
        {
            try
            {
                Console.WriteLine($"[FILTROS COLABORADORES] Buscando centros de custo com colaboradores - Cliente: {clienteId}");
                
                var centrosCusto = _colaboradorRepository
                    .Buscar(c => c.Cliente == clienteId && c.Situacao == "A" && c.Centrocusto > 0)
                    .Include(c => c.CentrocustoNavigation)
                    .Where(c => c.CentrocustoNavigation != null)
                    .GroupBy(c => new { c.Centrocusto, c.CentrocustoNavigation.Nome })
                    .Select(g => new { id = g.Key.Centrocusto, nome = g.Key.Nome })
                    .OrderBy(cc => cc.nome)
                    .ToList<dynamic>();
                
                Console.WriteLine($"[FILTROS COLABORADORES] ✅ {centrosCusto.Count} centros de custo encontrados");
                
                // Log detalhado
                foreach (var cc in centrosCusto)
                {
                    Console.WriteLine($"  - Centro de Custo ID: {cc.id}, Nome: {cc.nome}");
                }
                
                return centrosCusto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILTROS COLABORADORES] ❌ Erro ao buscar centros de custo: {ex.Message}");
                Console.WriteLine($"[FILTROS COLABORADORES] StackTrace: {ex.StackTrace}");
                return new List<dynamic>();
            }
        }

        #region MAPA DE RECURSOS
        /// <summary>
        /// 🗺️ Obter Mapa de Recursos com visualização hierárquica em árvore com drilldown
        /// Permite navegar por: Empresa → Localidade → Filial → Centro de Custo → Colaboradores → Recursos
        /// </summary>
        public MapaRecursosVM ObterMapaRecursos(MapaRecursosFiltroVM filtros)
        {
            try
            {
                Console.WriteLine($"[MAPA RECURSOS] 🗺️ Iniciando consulta - Cliente: {filtros.ClienteId}");
                Console.WriteLine($"[MAPA RECURSOS] Filtros - Empresa: {filtros.EmpresaId}, Localidade: {filtros.LocalidadeId}, Filial: {filtros.FilialId}, CentroCusto: {filtros.CentroCustoId}");
                Console.WriteLine($"[MAPA RECURSOS] Opções - IncluirSemRecursos: {filtros.IncluirColaboradoresSemRecursos}, IncluirHistorico: {filtros.IncluirHistoricoRecursos}");
                
                var resultado = new MapaRecursosVM();
                
                // 1️⃣ CONSTRUIR HIERARQUIA (Drilldown)
                resultado.RaizHierarquia = ConstruirHierarquiaRecursos(filtros);
                
                // 2️⃣ BUSCAR COLABORADORES E RECURSOS (quando drilldown chegar até colaboradores)
                if (ShouldLoadColaboradores(filtros))
                {
                    resultado.Colaboradores = ObterColaboradoresComRecursos(filtros);
                    
                    if (filtros.IncluirColaboradoresSemRecursos)
                    {
                        resultado.ColaboradoresSemRecursos = ObterColaboradoresSemRecursosMapa(filtros);
                    }
                }
                
                // 3️⃣ CALCULAR MÉTRICAS
                resultado.Metricas = CalcularMetricasMapa(filtros, resultado);
                
                Console.WriteLine($"[MAPA RECURSOS] ✅ Mapa gerado com sucesso");
                Console.WriteLine($"[MAPA RECURSOS] 📊 Total Colaboradores: {resultado.Metricas.TotalColaboradores}");
                Console.WriteLine($"[MAPA RECURSOS] 📊 Total Recursos: {resultado.Metricas.TotalRecursos}");
                
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MAPA RECURSOS] ❌ Erro: {ex.Message}");
                Console.WriteLine($"[MAPA RECURSOS] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Construir hierarquia de nós para visualização em árvore
        /// </summary>
        private MapaRecursosNoVM ConstruirHierarquiaRecursos(MapaRecursosFiltroVM filtros)
        {
            var raiz = new MapaRecursosNoVM
            {
                Id = 0,
                Nome = "Organização",
                Tipo = "raiz",
                Icone = "cil-building",
                Cor = "#4CAF50"
            };

            // Se não tem filtros específicos, mostrar empresas
            if (!filtros.EmpresaId.HasValue)
            {
                raiz.Filhos = ObterNosEmpresas(filtros.ClienteId);
                raiz.TemFilhos = raiz.Filhos.Any();
                return raiz;
            }

            // Se tem empresa mas não tem localidade, mostrar localidades
            if (filtros.EmpresaId.HasValue && !filtros.LocalidadeId.HasValue)
            {
                raiz.Filhos = ObterNosLocalidades(filtros.ClienteId, filtros.EmpresaId.Value);
                raiz.TemFilhos = raiz.Filhos.Any();
                return raiz;
            }

            // Se tem localidade mas não tem filial, mostrar filiais
            if (filtros.LocalidadeId.HasValue && !filtros.FilialId.HasValue)
            {
                raiz.Filhos = ObterNosFiliais(filtros.ClienteId, filtros.LocalidadeId.Value);
                raiz.TemFilhos = raiz.Filhos.Any();
                return raiz;
            }

            // Se tem filial mas não tem centro de custo, mostrar centros de custo
            if (filtros.FilialId.HasValue && !filtros.CentroCustoId.HasValue)
            {
                raiz.Filhos = ObterNosCentrosCusto(filtros.ClienteId, filtros.FilialId.Value);
                raiz.TemFilhos = raiz.Filhos.Any();
                return raiz;
            }

            // Se tem centro de custo, mostrar colaboradores (nós serão montados em outra parte)
            return raiz;
        }

        /// <summary>
        /// Obter nós de empresas para a árvore
        /// </summary>
        private List<MapaRecursosNoVM> ObterNosEmpresas(int clienteId)
        {
            var empresas = _colaboradorRepository
                .Buscar(c => c.Cliente == clienteId && c.Situacao == "A" && c.Empresa > 0)
                .Include(c => c.EmpresaNavigation)
                .GroupBy(c => new { c.Empresa, c.EmpresaNavigation.Nome })
                .Select(g => new
                {
                    Id = g.Key.Empresa,
                    Nome = g.Key.Nome,
                    TotalColaboradores = g.Count()
                })
                .ToList();

            return empresas.Select(e => new MapaRecursosNoVM
            {
                Id = e.Id,
                Nome = e.Nome,
                Tipo = "empresa",
                Descricao = $"{e.TotalColaboradores} colaboradores",
                TotalColaboradores = e.TotalColaboradores,
                Icone = "cil-building",
                Cor = "#2196F3",
                TemFilhos = true
            }).ToList();
        }

        /// <summary>
        /// Obter nós de localidades para a árvore
        /// </summary>
        private List<MapaRecursosNoVM> ObterNosLocalidades(int clienteId, int empresaId)
        {
            var localidades = _colaboradorRepository
                .Buscar(c => c.Cliente == clienteId && c.Situacao == "A" && c.Empresa == empresaId && c.Localidade > 0)
                .Include(c => c.LocalidadeNavigation)
                .GroupBy(c => new { c.Localidade, c.LocalidadeNavigation.Descricao })
                .Select(g => new
                {
                    Id = g.Key.Localidade,
                    Nome = g.Key.Descricao,
                    TotalColaboradores = g.Count()
                })
                .ToList();

            return localidades.Select(l => new MapaRecursosNoVM
            {
                Id = l.Id,
                Nome = l.Nome,
                Tipo = "localidade",
                Descricao = $"{l.TotalColaboradores} colaboradores",
                TotalColaboradores = l.TotalColaboradores,
                Icone = "cil-location-pin",
                Cor = "#FF9800",
                TemFilhos = true
            }).ToList();
        }

        /// <summary>
        /// Obter nós de filiais para a árvore
        /// </summary>
        private List<MapaRecursosNoVM> ObterNosFiliais(int clienteId, int localidadeId)
        {
            var filiais = _colaboradorRepository
                .Buscar(c => c.Cliente == clienteId && c.Situacao == "A" && c.Localidade == localidadeId && c.FilialId.HasValue)
                .Include(c => c.Filial)
                .GroupBy(c => new { c.FilialId, c.Filial.Descricao })
                .Select(g => new
                {
                    Id = g.Key.FilialId.Value,
                    Nome = g.Key.Descricao,
                    TotalColaboradores = g.Count()
                })
                .ToList();

            return filiais.Select(f => new MapaRecursosNoVM
            {
                Id = f.Id,
                Nome = f.Nome,
                Tipo = "filial",
                Descricao = $"{f.TotalColaboradores} colaboradores",
                TotalColaboradores = f.TotalColaboradores,
                Icone = "cil-bank",
                Cor = "#9C27B0",
                TemFilhos = true
            }).ToList();
        }

        /// <summary>
        /// Obter nós de centros de custo para a árvore
        /// </summary>
        private List<MapaRecursosNoVM> ObterNosCentrosCusto(int clienteId, int filialId)
        {
            var centrosCusto = _colaboradorRepository
                .Buscar(c => c.Cliente == clienteId && c.Situacao == "A" && c.FilialId == filialId && c.Centrocusto > 0)
                .Include(c => c.CentrocustoNavigation)
                .GroupBy(c => new { c.Centrocusto, c.CentrocustoNavigation.Nome })
                .Select(g => new
                {
                    Id = g.Key.Centrocusto,
                    Nome = g.Key.Nome,
                    TotalColaboradores = g.Count()
                })
                .ToList();

            return centrosCusto.Select(cc => new MapaRecursosNoVM
            {
                Id = cc.Id,
                Nome = cc.Nome,
                Tipo = "centroCusto",
                Descricao = $"{cc.TotalColaboradores} colaboradores",
                TotalColaboradores = cc.TotalColaboradores,
                Icone = "cil-folder",
                Cor = "#4CAF50",
                TemFilhos = true
            }).ToList();
        }

        /// <summary>
        /// Verificar se deve carregar colaboradores (drilldown chegou até o nível final)
        /// </summary>
        private bool ShouldLoadColaboradores(MapaRecursosFiltroVM filtros)
        {
            // Carregar colaboradores apenas quando houver centro de custo selecionado
            return filtros.CentroCustoId.HasValue;
        }

        /// <summary>
        /// Obter colaboradores com seus recursos
        /// </summary>
        private List<MapaColaboradorVM> ObterColaboradoresComRecursos(MapaRecursosFiltroVM filtros)
        {
            var query = _colaboradorRepository.Buscar(c => c.Cliente == filtros.ClienteId && c.Situacao == "A");

            if (filtros.EmpresaId.HasValue)
                query = query.Where(c => c.Empresa == filtros.EmpresaId.Value);

            if (filtros.LocalidadeId.HasValue)
                query = query.Where(c => c.Localidade == filtros.LocalidadeId.Value);

            if (filtros.FilialId.HasValue)
                query = query.Where(c => c.FilialId == filtros.FilialId.Value);

            if (filtros.CentroCustoId.HasValue)
                query = query.Where(c => c.Centrocusto == filtros.CentroCustoId.Value);

            var colaboradores = query
                .Include(c => c.EmpresaNavigation)
                .Include(c => c.LocalidadeNavigation)
                .Include(c => c.Filial)
                .Include(c => c.CentrocustoNavigation)
                .ToList();

            var resultado = new List<MapaColaboradorVM>();

            foreach (var colab in colaboradores)
            {
                var recursos = ObterRecursosColaborador(colab.Id, filtros.IncluirHistoricoRecursos);
                
                // Se não incluir histórico e colaborador não tem recursos ativos, pular
                if (!filtros.IncluirHistoricoRecursos && !recursos.Any())
                    continue;

                var colabVM = new MapaColaboradorVM
                {
                    Id = colab.Id,
                    Nome = colab.Nome,
                    Email = colab.Email,
                    Cpf = colab.Cpf,
                    Cargo = colab.Cargo,
                    TipoColaborador = ObterDescricaoTipoColaborador(colab.Tipocolaborador),
                    Empresa = colab.EmpresaNavigation?.Nome,
                    Localidade = colab.LocalidadeNavigation?.Descricao,
                    Filial = colab.Filial?.Descricao,
                    CentroCusto = colab.CentrocustoNavigation?.Nome,
                    Recursos = recursos.Where(r => r.Status == "E").ToList(), // Apenas entregues (ativos)
                    HistoricoRecursos = filtros.IncluirHistoricoRecursos ? recursos.Where(r => r.Status != "E").ToList() : new List<MapaRecursoDetalheVM>(),
                    TotalRecursos = recursos.Count,
                    TotalRecursosAtivos = recursos.Count(r => r.Status == "E")
                };

                resultado.Add(colabVM);
            }

            return resultado.Where(c => c.TotalRecursos > 0).ToList();
        }

        /// <summary>
        /// Obter recursos de um colaborador específico
        /// </summary>
        private List<MapaRecursoDetalheVM> ObterRecursosColaborador(int colaboradorId, bool incluirHistorico)
        {
            var recursos = new List<MapaRecursoDetalheVM>();

            // Buscar requisições do colaborador
            var requisicoes = _requisicaoItensRepository
                .Buscar(ri => 
                    ri.RequisicaoNavigation.Colaboradorfinal == colaboradorId &&
                    ri.Equipamento.HasValue &&
                    ri.Dtentrega.HasValue)
                .Include(ri => ri.EquipamentoNavigation)
                    .ThenInclude(e => e.TipoequipamentoNavigation)
                .Include(ri => ri.EquipamentoNavigation)
                    .ThenInclude(e => e.ModeloNavigation)
                .ToList();

            foreach (var req in requisicoes)
            {
                var equip = req.EquipamentoNavigation;
                if (equip == null || !equip.Ativo) continue;

                var status = req.Dtdevolucao.HasValue ? "D" : "E"; // Devolvido ou Entregue
                
                // Se não incluir histórico, pular recursos devolvidos
                if (!incluirHistorico && status == "D")
                    continue;

                recursos.Add(new MapaRecursoDetalheVM
                {
                    Id = equip.Id,
                    Tipo = equip.TipoequipamentoNavigation?.Descricao ?? "N/A",
                    Modelo = equip.ModeloNavigation?.Nome ?? "N/A",
                    NumeroSerie = equip.Numeroserie,
                    Patrimonio = equip.Patrimonio,
                    Status = status,
                    StatusDescricao = status == "E" ? "Entregue" : "Devolvido",
                    DataEntrega = req.Dtentrega,
                    DataDevolucao = req.Dtdevolucao,
                    Cor = status == "E" ? "#4CAF50" : "#9E9E9E",
                    Icone = ObterIconePorTipo(equip.TipoequipamentoNavigation?.Descricao)
                });
            }

            return recursos;
        }

        /// <summary>
        /// Obter colaboradores SEM recursos para o mapa
        /// </summary>
        private List<MapaColaboradorVM> ObterColaboradoresSemRecursosMapa(MapaRecursosFiltroVM filtros)
        {
            var query = _colaboradorRepository.Buscar(c => c.Cliente == filtros.ClienteId && c.Situacao == "A");

            if (filtros.EmpresaId.HasValue)
                query = query.Where(c => c.Empresa == filtros.EmpresaId.Value);

            if (filtros.LocalidadeId.HasValue)
                query = query.Where(c => c.Localidade == filtros.LocalidadeId.Value);

            if (filtros.FilialId.HasValue)
                query = query.Where(c => c.FilialId == filtros.FilialId.Value);

            if (filtros.CentroCustoId.HasValue)
                query = query.Where(c => c.Centrocusto == filtros.CentroCustoId.Value);

            var colaboradores = query
                .Include(c => c.EmpresaNavigation)
                .Include(c => c.LocalidadeNavigation)
                .Include(c => c.Filial)
                .Include(c => c.CentrocustoNavigation)
                .ToList();

            // IDs de colaboradores COM recursos
            var colaboradoresComRecursos = _requisicaoItensRepository
                .Buscar(ri => 
                    ri.Equipamento.HasValue &&
                    ri.Dtentrega.HasValue &&
                    !ri.Dtdevolucao.HasValue)
                .Select(ri => ri.RequisicaoNavigation.Colaboradorfinal)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            // Filtrar apenas colaboradores SEM recursos
            var colaboradoresSemRecursos = colaboradores
                .Where(c => !colaboradoresComRecursos.Contains(c.Id))
                .Select(c => new MapaColaboradorVM
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Email = c.Email,
                    Cpf = c.Cpf,
                    Cargo = c.Cargo,
                    TipoColaborador = ObterDescricaoTipoColaborador(c.Tipocolaborador),
                    Empresa = c.EmpresaNavigation?.Nome,
                    Localidade = c.LocalidadeNavigation?.Descricao,
                    Filial = c.Filial?.Descricao,
                    CentroCusto = c.CentrocustoNavigation?.Nome,
                    TotalRecursos = 0,
                    TotalRecursosAtivos = 0
                })
                .ToList();

            return colaboradoresSemRecursos;
        }

        /// <summary>
        /// Calcular métricas agregadas do mapa
        /// </summary>
        private MapaRecursosMetricasVM CalcularMetricasMapa(MapaRecursosFiltroVM filtros, MapaRecursosVM mapa)
        {
            var metricas = new MapaRecursosMetricasVM();

            metricas.TotalColaboradores = mapa.Colaboradores.Count + mapa.ColaboradoresSemRecursos.Count;
            metricas.TotalColaboradoresComRecursos = mapa.Colaboradores.Count;
            metricas.TotalColaboradoresSemRecursos = mapa.ColaboradoresSemRecursos.Count;
            metricas.TotalRecursos = mapa.Colaboradores.Sum(c => c.TotalRecursos);
            metricas.TotalRecursosEntregues = mapa.Colaboradores.Sum(c => c.TotalRecursosAtivos);

            if (metricas.TotalColaboradoresComRecursos > 0)
            {
                metricas.MediaRecursosPorColaborador = (decimal)metricas.TotalRecursos / metricas.TotalColaboradoresComRecursos;
            }

            if (mapa.Colaboradores.Any())
            {
                metricas.ColaboradorComMaisRecursos = mapa.Colaboradores
                    .OrderByDescending(c => c.TotalRecursos)
                    .FirstOrDefault();
            }

            return metricas;
        }

        /// <summary>
        /// Obter ícone visual baseado no tipo de recurso
        /// </summary>
        private string ObterIconePorTipo(string tipo)
        {
            if (string.IsNullOrEmpty(tipo)) return "cil-laptop";

            tipo = tipo.ToLower();
            
            if (tipo.Contains("notebook") || tipo.Contains("laptop"))
                return "cil-laptop";
            if (tipo.Contains("desktop") || tipo.Contains("computador"))
                return "cil-monitor";
            if (tipo.Contains("mouse"))
                return "cil-mouse";
            if (tipo.Contains("teclado"))
                return "cil-keyboard";
            if (tipo.Contains("celular") || tipo.Contains("smartphone"))
                return "cil-mobile";
            if (tipo.Contains("monitor"))
                return "cil-tv";
            if (tipo.Contains("telefon"))
                return "cil-phone";
            
            return "cil-laptop";
        }
        #endregion
    }
}
