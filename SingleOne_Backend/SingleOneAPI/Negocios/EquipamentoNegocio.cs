using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOne.Util;
using SingleOneAPI;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SingleOne.Negocios
{
    public class EquipamentoNegocio : IEquipamentoNegocio
    {
        // Campo removido pois não estava sendo utilizado
        private SendMail mail;
        private readonly IMapper _mapper;
        private readonly IRepository<Colaboradore> _colaboradoresRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Equipamentohistorico> _equipamentohistoricoRepository;
        private readonly IRepository<Equipamentosstatus> _equipamentosstatusRepository;
        private readonly IRepository<Laudo> _laudoRepository;
        private readonly IRepository<Requisico> _requisicaoRepository;
        private readonly IRepository<Requisicoesiten> _requisicaoItensRepository;
        private readonly IRepository<Equipamentoanexo> _equipamentoanexoRepository;
        private readonly IRepository<Parametro> _parametroRepository;
        private readonly IRepository<Contrato> _contratoRepository;
        private readonly IReadOnlyRepository<Equipamentovm> _equipamentovmRepository;
        private readonly IReadOnlyRepository<Termoentregavm> _termoentregavmRepository;
        private readonly IReadOnlyRepository<Vwexportacaoexcel> _vwexportacaoexcelRepository;
        private readonly IRepository<Telefonialinha> _telefonialinhaRepository;
        private readonly IRepository<Telefoniaplano> _telefoniaplanoRepository;
        private readonly IRepository<Telefoniacontrato> _telefoniacontratoRepository;
        private readonly IRepository<Telefoniaoperadora> _telefoniaoperadoraRepository;
        private readonly IRepository<Centrocusto> _centrocustoRepository;
        private readonly IRepository<CargoConfianca> _cargoConfiancaRepository;
        private readonly IRepository<DescarteEvidencia> _descarteEvidenciaRepository;

        public EquipamentoNegocio(EnvironmentApiSettings environmentApiSettings,
            IMapper mapper,
            IRepository<Colaboradore> colaboradoresRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Equipamentohistorico> equipamentohistoricoRepository,
            IRepository<Equipamentosstatus> equipamentosstatusRepository,
            IRepository<Laudo> laudoRepository,
            IRepository<Requisico> requisicaoRepository,
            IRepository<Requisicoesiten> requisicaoItensRepository,
            IRepository<Equipamentoanexo> equipamentoanexoRepository,
            IRepository<Parametro> parametroRepository,
            IRepository<Contrato> contratoRepository,
            IReadOnlyRepository<Equipamentovm> equipamentovmRepository,
            IReadOnlyRepository<Termoentregavm> termoentregavm,
            IReadOnlyRepository<Vwexportacaoexcel> vwexportacaoexcelRepository,
            IRepository<Telefonialinha> telefonialinhaRepository,
            IRepository<Telefoniaplano> telefoniaplanoRepository,
            IRepository<Telefoniacontrato> telefoniacontratoRepository,
            IRepository<Telefoniaoperadora> telefoniaoperadoraRepository,
            IRepository<Centrocusto> centrocustoRepository,
            IRepository<CargoConfianca> cargoConfiancaRepository,
            IRepository<DescarteEvidencia> descarteEvidenciaRepository)
        {
            this.mail = new SendMail(environmentApiSettings);
            _mapper = mapper;
            _colaboradoresRepository = colaboradoresRepository;
            _equipamentoRepository = equipamentoRepository;
            _equipamentohistoricoRepository = equipamentohistoricoRepository;
            _equipamentosstatusRepository = equipamentosstatusRepository;
            _laudoRepository = laudoRepository;
            _requisicaoRepository = requisicaoRepository;
            _requisicaoItensRepository = requisicaoItensRepository;
            _equipamentoanexoRepository = equipamentoanexoRepository;
            _parametroRepository = parametroRepository;
            _equipamentovmRepository = equipamentovmRepository;
            _termoentregavmRepository = termoentregavm;
            _vwexportacaoexcelRepository = vwexportacaoexcelRepository;
            _contratoRepository = contratoRepository;
            _telefonialinhaRepository = telefonialinhaRepository;
            _telefoniaplanoRepository = telefoniaplanoRepository;
            _telefoniacontratoRepository = telefoniacontratoRepository;
            _telefoniaoperadoraRepository = telefoniaoperadoraRepository;
            _centrocustoRepository = centrocustoRepository;
            _cargoConfiancaRepository = cargoConfiancaRepository;
            _descarteEvidenciaRepository = descarteEvidenciaRepository;
        }

        public PagedResult<Equipamentovm> ListarEquipamentos(string pesquisa, int cliente, int? contrato, int pagina, int paginaTamanho, int? modeloId = null, int? localidadeId = null)
        {
            Console.WriteLine($"[EQUIPAMENTOS] Listando equipamentos - Cliente: {cliente}, Contrato: {contrato}, Pesquisa: '{pesquisa}', Página: {pagina}, Tamanho: {paginaTamanho}, Modelo: {modeloId}, Localidade: {localidadeId}");
            
            // ✅ CORREÇÃO: Tratar pesquisa null ou "null"
            if (string.IsNullOrWhiteSpace(pesquisa) || (pesquisa != null && pesquisa.ToLower() == "null"))
            {
                pesquisa = null;
            }
            else if (pesquisa != null)
            {
                pesquisa = pesquisa.ToLower();
            }
            
            // Construir filtro de forma mais simples
            if (contrato.HasValue && contrato.Value > 0)
            {
                Console.WriteLine($"[EQUIPAMENTOS] Aplicando filtro por contrato: {contrato.Value}");
                var vm = _equipamentovmRepository
                    .Buscar(x => x.Cliente == cliente && 
                    x.Ativo.Value &&
                    x.Contratoid == contrato.Value &&
                    (modeloId.HasValue && modeloId.Value > 0 ? x.Modeloid == modeloId.Value : true) &&
                    (localidadeId.HasValue && localidadeId.Value > 0 ? x.Localizacaoid == localidadeId.Value : true) &&
                    (pesquisa != null ?
                        x.Fabricante.ToLower().Contains(pesquisa) ||
                        x.Modelo.ToLower().Contains(pesquisa) ||
                        x.Numeroserie.ToLower().Contains(pesquisa) ||
                        x.Patrimonio.ToLower().Contains(pesquisa) ||
                        (x.Colaboradornome != null && x.Colaboradornome.ToLower().Contains(pesquisa))
                        : 1 == 1))
                    .OrderByDescending(x => x.Id).ThenBy(x => x.Tipoequipamento).ThenBy(x => x.Fabricante).ThenBy(x => x.Modelo)
                    .GetPaged(pagina, paginaTamanho);
                
                Console.WriteLine($"[EQUIPAMENTOS] Resultado filtrado por contrato: {vm.Results.Count} equipamentos encontrados");
                return vm;
            }
            else
            {
                var vm = _equipamentovmRepository
                    .Buscar(x => x.Cliente == cliente && 
                    x.Ativo.Value &&
                    (modeloId.HasValue && modeloId.Value > 0 ? x.Modeloid == modeloId.Value : true) &&
                    (localidadeId.HasValue && localidadeId.Value > 0 ? x.Localizacaoid == localidadeId.Value : true) &&
                    (pesquisa != null ?
                        x.Fabricante.ToLower().Contains(pesquisa) ||
                        x.Modelo.ToLower().Contains(pesquisa) ||
                        x.Numeroserie.ToLower().Contains(pesquisa) ||
                        x.Patrimonio.ToLower().Contains(pesquisa) ||
                        (x.Colaboradornome != null && x.Colaboradornome.ToLower().Contains(pesquisa))
                        : 1 == 1))
                    .OrderByDescending(x => x.Id).ThenBy(x => x.Tipoequipamento).ThenBy(x => x.Fabricante).ThenBy(x => x.Modelo)
                    .GetPaged(pagina, paginaTamanho);
                
                Console.WriteLine($"[EQUIPAMENTOS] Resultado: {vm.Results.Count} equipamentos encontrados");
                return vm;
            }
        }

        // 📊 MÉTODO PARA RESUMO - SEM LIMITAÇÃO DE PAGINAÇÃO
        public List<Equipamentovm> ListarTodosEquipamentosParaResumo(int cliente)
        {
            Console.WriteLine($"[EQUIPAMENTOS-RESUMO] Listando TODOS os equipamentos para resumo - Cliente: {cliente}");
            
            // ✅ CORREÇÃO: Aplicar mesmo filtro da tabela (apenas ativos)
            var equipamentos = _equipamentovmRepository
                .Buscar(x => x.Cliente == cliente && x.Ativo.Value)
                .OrderByDescending(x => x.Id)
                .ToList();
            
            Console.WriteLine($"[EQUIPAMENTOS-RESUMO] Total de equipamentos ativos encontrados: {equipamentos.Count}");
            return equipamentos;
        }

        //Por hora não será usado...
        //public PagedResult<Equipamento> ListarEquipamentosContrato(int idContrato)
        //{
        //    var equips = _contratoRepository
        //            .IncludeWithThenInclude(i => i
        //                .Include(i => i.Equipamentos)
        //                    .ThenInclude(t => t.TipoequipamentoNavigation)
        //                .Include(i => i.Equipamentos)
        //                    .ThenInclude(t => t.FabricanteNavigation)
        //                .Include(i => i.Equipamentos)
        //                    .ThenInclude(t => t.ModeloNavigation))
        //            .Where(x => x.Id == idContrato)
        //            .Select(s => s.Equipamentos)
        //            .FirstOrDefault();
        //    return new PagedResult<Equipamento>
        //    {
        //        CurrentPage = 1,
        //        PageCount = 1,
        //        PageSize = 1,
        //        Results = equips.ToList(),
        //        RowCount = equips.Count
        //    };
        //}

        public List<Equipamentovm> ListarEquipamentosDisponiveis(string pesquisa, int cliente)
        {
            pesquisa = pesquisa.ToLower();
            var eqps = _equipamentovmRepository
                        .Buscar(x => x.Cliente == cliente && x.Ativo == true &&
                                    x.Equipamentostatusid == 3 &&
                                    x.Numeroserie != "Não cadastrado" &&
                                    ((pesquisa != "null") ? 
                                        x.Tipoequipamento.ToLower().Contains(pesquisa) ||
                                        x.Fabricante.ToLower().Contains(pesquisa) ||
                                        x.Modelo.ToLower().Contains(pesquisa) ||
                                        x.Numeroserie.ToLower().Contains(pesquisa) ||
                                        x.Patrimonio.ToLower().Contains(pesquisa)
                                    : 1 == 1)).ToList();
            return eqps;
        }
        public List<Equipamentovm> ListarEquipamentoDisponivelParaLaudos(string pesquisa, int cliente)
        {
            pesquisa = pesquisa.ToLower();
            var eqps = _equipamentovmRepository
                        .Buscar(x => x.Cliente == cliente && x.Ativo == true &&
                            ((pesquisa != "null") ?
                                        x.Tipoequipamento.ToLower().Contains(pesquisa) ||
                                        x.Fabricante.ToLower().Contains(pesquisa) ||
                                        x.Modelo.ToLower().Contains(pesquisa) ||
                                        x.Numeroserie.ToLower().Contains(pesquisa) ||
                                        x.Patrimonio.ToLower().Contains(pesquisa)
                                    : 1 == 1)
                        && (x.Equipamentostatusid == 2 || x.Equipamentostatusid == 7)).ToList();
            //&& (x.Equipamentostatusid != 11 && x.Equipamentostatusid != 9 && x.Equipamentostatusid != 8 && x.Equipamentostatusid != 5 && x.Equipamentostatusid != 1)).ToList();
            return eqps;
        }
        public List<Equipamentovm> ListarEquipamentosDisponiveisParaEstoque(int cliente)
        {
            // Correção de precedência lógica: todos devem respeitar cliente e ativo
            var eqps = _equipamentovmRepository
                .Buscar(x => x.Cliente == cliente 
                             && x.Ativo == true 
                             && (x.Equipamentostatusid == 6 
                                 || x.Equipamentostatusid == 5 
                                 || x.Equipamentostatusid == 2))
                .ToList();
            return eqps;
        }
        public List<Equipamentosstatus> ListarStatusEquipamentos()
        {
            var status = _equipamentosstatusRepository.Buscar(x => x.Ativo).OrderBy(x => x.Descricao).ToList();
            return status;
        }
        public Equipamento BuscarEquipamentoPorId(int id)
        {
            try
            {
                var equipamentoVM = _equipamentovmRepository
                    .Buscar(x => x.Id == id)
                    .FirstOrDefault();
                
                if (equipamentoVM == null)
                {
                    return null;
                }
                
                // ✅ DEBUG: Log dos dados do ViewModel
                Console.WriteLine($"[DEBUG-EQUIPAMENTO] Dados do ViewModel para ID {id}:");
                Console.WriteLine($"  - Empresaid: {equipamentoVM.Empresaid}");
                Console.WriteLine($"  - Centrocustoid: {equipamentoVM.Centrocustoid}");
                Console.WriteLine($"  - Localizacaoid: {equipamentoVM.Localizacaoid}");
                Console.WriteLine($"  - Localizacao (texto): {equipamentoVM.Localizacao}");
                Console.WriteLine($"  - Filialid: {equipamentoVM.Filialid}");
                
                // ✅ CORREÇÃO: Se empresa estiver vazia mas centro de custo estiver preenchido, buscar empresa do centro de custo
                int? empresaFinal = equipamentoVM.Empresaid;
                if (!empresaFinal.HasValue && equipamentoVM.Centrocustoid.HasValue)
                {
                    var centroCusto = _centrocustoRepository.Buscar(x => x.Id == equipamentoVM.Centrocustoid.Value).FirstOrDefault();
                    if (centroCusto != null)
                    {
                        empresaFinal = centroCusto.Empresa;
                        Console.WriteLine($"[DEBUG-EQUIPAMENTO] Empresa derivada do centro de custo {equipamentoVM.Centrocustoid}: {empresaFinal}");
                    }
                }
                
                var equipamento = new Equipamento
                {
                    Id = equipamentoVM.Id ?? 0,
                    Cliente = equipamentoVM.Cliente,
                    Tipoequipamento = equipamentoVM.Tipoequipamentoid ?? 0,
                    Fabricante = equipamentoVM.Fabricanteid ?? 0,
                    Modelo = equipamentoVM.Modeloid ?? 0,
                    Notafiscal = equipamentoVM.Notafiscalid,
                    Equipamentostatus = equipamentoVM.Equipamentostatusid,
                    Usuario = equipamentoVM.Usuarioid,
                    Tipoaquisicao = equipamentoVM.TipoAquisicao,
                    Fornecedor = equipamentoVM.Fornecedor,
                    Possuibo = equipamentoVM.Possuibo ?? false,
                    Descricaobo = equipamentoVM.Descricaobo,
                    Numeroserie = equipamentoVM.Numeroserie,
                    Patrimonio = equipamentoVM.Patrimonio,
                    Dtlimitegarantia = equipamentoVM.Dtlimitegarantia,
                    Dtcadastro = equipamentoVM.Dtcadastro ?? DateTime.Now,
                    Ativo = equipamentoVM.Ativo ?? true,
                    Migrateid = null, // Não disponível no ViewModel
                    Enviouemailreporte = null, // Não disponível no ViewModel
                    Empresa = empresaFinal, // ✅ CORREÇÃO: Usar empresa derivada do centro de custo se necessário
                    Centrocusto = equipamentoVM.Centrocustoid,
                    Contrato = equipamentoVM.Contratoid,
                    FilialId = equipamentoVM.Filialid, // ✅ CORREÇÃO: Usar Filialid do ViewModel
                    Localidade = equipamentoVM.Localizacaoid, // ✅ CORREÇÃO: Usar Localizacaoid como Localidade
                    // Localizacao = equipamentoVM.Localizacaoid // ✅ REMOVIDO: Coluna localizacao não existe mais no banco
                };
                
                // ✅ DEBUG: Log dos dados mapeados
                Console.WriteLine($"[DEBUG-EQUIPAMENTO] Dados mapeados para Equipamento:");
                Console.WriteLine($"  - Empresa: {equipamento.Empresa}");
                Console.WriteLine($"  - Centrocusto: {equipamento.Centrocusto}");
                Console.WriteLine($"  - Localidade: {equipamento.Localidade}");
                Console.WriteLine($"  - FilialId: {equipamento.FilialId}");
                
                return equipamento;
            }
            catch
            {
                throw;
            }
        }
        public PagedResult<Equipamentovm> BuscarEquipamentoPorNumeroSeriePatrimonio(int cliente, string numeroSerie)
        {
            numeroSerie = numeroSerie.ToLower();
            var vm = _equipamentovmRepository
                   .Buscar(x => x.Cliente == cliente && x.Ativo == true && (x.Numeroserie.ToLower() == numeroSerie || x.Patrimonio.ToLower() == numeroSerie))
                   .OrderBy(x => x.Tipoequipamento).ThenBy(x => x.Fabricante).ThenBy(x => x.Modelo)
                   .GetPaged(1, 10);
            return vm;
        }
        public string SalvarEquipamento(Equipamento eq)
        {
            
            // Garantir consistência de Empresa/Cliente/Localidade antes de salvar
            try
            {
                // Se veio Empresa, e Cliente não veio, herdar cliente da empresa via navegação (quando disponível)
                if (eq.Empresa.HasValue && !eq.Cliente.HasValue)
                {
                    // Cliente é obrigatório para regras de unicidade; se não carregado por navegação, manter nulo para não quebrar
                    // Será preenchido por trigger/regra no banco, se existir
                }

                // Priorizar Localidade moderna quando informada: zerar Localizacao legada para evitar conflito
                // ✅ REMOVIDO: Coluna localizacao não existe mais no banco
                // if (eq.Localidade.HasValue)
                // {
                //     eq.Localizacao = null;
                // }
            }
            catch { /* manter defensivo, não impedir persistência */ }

            if (eq.Id == 0)
            {
                //bool existe = _equipamentoRepository.Buscar(x => x.NumeroSerie == eq.NumeroSerie || x.Patrimonio == eq.Patrimonio).Any();
                //if (!existe)
                bool existePatrimonio = (eq.Patrimonio == null) ? false : _equipamentoRepository.Buscar(x => x.Patrimonio == eq.Patrimonio && x.Cliente == eq.Cliente).Any();
                bool existeNumeroSerie = _equipamentoRepository.Buscar(x => x.Numeroserie == eq.Numeroserie && x.Cliente == eq.Cliente).Any();
                if (!existeNumeroSerie && !existePatrimonio)
                {
                    try
                    {
                        // ✅ SOLUÇÃO BASEADA NA NOTA FISCAL: Usar transação como na nota fiscal
                        return _equipamentoRepository.ExecuteInTransaction(() =>
                        {
                            // Configurar equipamento
                            eq.Dtcadastro = TimeZoneMapper.GetDateTimeNow();
                            if (!eq.Ativo) eq.Ativo = true;

                            // Adicionar equipamento ao contexto (sem salvar ainda)
                            _equipamentoRepository.AdicionarSemSalvar(eq);
                            
                            // Salvar equipamento para obter ID
                            _equipamentoRepository.SalvarAlteracoes();
                            
                            // Agora criar histórico com o ID já gerado
                            var hst = new Equipamentohistorico();
                            hst.Equipamento = eq.Id; // ID já foi gerado
                            hst.Equipamentostatus = eq.Equipamentostatus ?? 3;
                            hst.Usuario = eq.Usuario ?? 0;
                            hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                            
                            // Adicionar histórico ao contexto (sem salvar ainda)
                            _equipamentohistoricoRepository.AdicionarSemSalvar(hst);
                            
                            // Salvar histórico
                            _equipamentohistoricoRepository.SalvarAlteracoes();

                            return JsonConvert.SerializeObject(new { Mensagem = "Equipamento salvo com sucesso!", Status = "200" });
                        });
                    }
                    catch (Exception)
                    {
                        return JsonConvert.SerializeObject(new { Mensagem = "Falha ao salvar equipamento...", Status = "500" });
                    }
                }
                else
                {
                    return JsonConvert.SerializeObject(new { Mensagem = "Já existe um equipamento com o serial/numero de patrimonio informado", Status = "200.1" });
                }
            }
            else
            {
                var eqp = _equipamentoRepository.Buscar(x => x.Id == eq.Id).AsNoTracking().FirstOrDefault();
                
                // ✅ VALIDAÇÃO: Bloquear edição de equipamentos com status críticos
                // Status 8 = Roubado (B.O. registrado)
                // Status 10 = Descartado (já finalizado processo de descarte)
                if (eqp.Equipamentostatus == 8 || eqp.Equipamentostatus == 10)
                {
                    string statusDescricao = eqp.Equipamentostatus == 8 ? "Roubado (B.O. registrado)" : "Descartado";
                    Console.WriteLine($"[EQUIPAMENTO] Tentativa de editar equipamento ID {eq.Id} com status bloqueado: {statusDescricao}");
                    return JsonConvert.SerializeObject(new { 
                        Mensagem = $"Não é possível editar este equipamento. Status: {statusDescricao}. Equipamentos com este status não podem ser modificados.", 
                        Status = "403" 
                    });
                }
                
                if (eqp.Numeroserie == eq.Numeroserie && eqp.Patrimonio == eq.Patrimonio)
                {
                    // ✅ REMOVIDO: Coluna localizacao não existe mais no banco
                    // if (eq.Localidade.HasValue) eq.Localizacao = null;
                    
                    _equipamentoRepository.Atualizar(eq);
                }
                else
                {
                    bool existePatrimonio = _equipamentoRepository.Buscar(x => x.Patrimonio == eq.Patrimonio).AsNoTracking().Any();
                    bool existeNumeroSerie = _equipamentoRepository.Buscar(x => x.Numeroserie == eq.Numeroserie).AsNoTracking().Any();
                    if (!existeNumeroSerie && !existePatrimonio)
                    {
                        // ✅ REMOVIDO: Coluna localizacao não existe mais no banco
                        // if (eq.Localidade.HasValue) eq.Localizacao = null;
                        _equipamentoRepository.Atualizar(eq);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new { Mensagem = "Já existe um equipamento com o serial/numero de patrimonio informado", Status = "200.1" });
                    }
                }
                return JsonConvert.SerializeObject(new { Mensagem = "Equipamento salvo com sucesso!", Status = "200" });
            }
        }
        public void ExcluirEquipamento(int id)
        {
            var eqp = _equipamentoRepository.Buscar(x => x.Id == id).AsNoTracking().FirstOrDefault();
            try
            {
                //db.Remove(eqp);
                //db.SaveChanges();
                _equipamentoRepository.Remover(eqp);
            }
            catch
            {
                eqp.Ativo = false;
                //db.Update(eqp);
                //db.SaveChanges();
                _equipamentoRepository.Atualizar(eqp);
            }
        }
        public void IncluirAnexo(Equipamentoanexo anexo)
        {
            try
            {
                anexo.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                _equipamentoanexoRepository.Adicionar(anexo);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExcluirAnexo(int id)
        {
            try
            {
                var anexo = _equipamentoanexoRepository.ObterPorId(id);
                //db.Remove(anexo);
                //db.SaveChanges();
                _equipamentoanexoRepository.Remover(anexo);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<Equipamentoanexo> AnexosDoEquipamento(int idEquipamento)
        {
            var anexos = _equipamentoanexoRepository.Buscar(x => x.Equipamento == idEquipamento).ToList();
            return anexos;
        }

        public int LiberarParaEstoque(int idUsuario, int idEquipamento)
        {
            //Busco por equipamentos Novos, extraviados ou devolvidos que estão com o numero de serie e patrimonio preenchido
            var aptos = _equipamentoRepository.Buscar(x => (x.Equipamentostatus == 6 || x.Equipamentostatus == 5 || x.Equipamentostatus == 2) && x.Numeroserie != "Não cadastrado" && x.Patrimonio != null && ((idEquipamento > 0) ? x.Id == idEquipamento : 1 == 1)).ToList();
            if (aptos.Count > 0)
            {
                try
                {
                    foreach (var apto in aptos)
                    {
                        //Atualizo o status do equipamento para "Em estoque"
                        apto.Equipamentostatus = 3;
                        apto.Ativo = true;
                        
                        _equipamentoRepository.Atualizar(apto);

                        //Registro no historico do equipamento que ele está indo para estoque
                        var hst = new Equipamentohistorico();
                        hst.Equipamento = apto.Id;
                        hst.Equipamentostatus = 3;
                        hst.Usuario = idUsuario;
                        hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }
                    
                    // Salvar todas as alterações
                    _equipamentoRepository.SalvarAlteracoes();
                    _equipamentohistoricoRepository.SalvarAlteracoes();
                    
                    return aptos.Count();
                }
                catch (Exception ex)
                {
                    // Log do erro para debug
                    Console.WriteLine($"[ERRO] Falha ao liberar equipamento para estoque: {ex.Message}");
                    throw;
                }
            }
            return 0;
        }

        public void RegistrarBO(Equipamento eqp)
        {
            var eqpto = _equipamentoRepository
                    .Include(x => x.TipoequipamentoNavigation)
                    .Include(x => x.FabricanteNavigation)
                    .Include(x => x.ModeloNavigation)
                .Where(x => x.Id == eqp.Id).FirstOrDefault();

            using (var trans = _equipamentoRepository.BeginTransaction())
            {
                try
                {
                    eqpto.Possuibo = true;
                    eqpto.Descricaobo = eqp.Descricaobo;
                    eqpto.Equipamentostatus = 8;

                    var ri = _requisicaoItensRepository.Buscar(x => x.Equipamento == eqpto.Id).Single();
                    ri.Dtdevolucao = TimeZoneMapper.GetDateTimeNow();
                    //db.Update(ri);
                    //db.SaveChanges();
                    _requisicaoItensRepository.Atualizar(ri);

                    //db.Entry(eqpto).State = EntityState.Modified;
                    //db.SaveChanges();
                    _equipamentoRepository.Atualizar(eqpto);

                    var hst = new Equipamentohistorico();
                    hst.Equipamento = eqpto.Id;
                    hst.Equipamentostatus = eqpto.Equipamentostatus.Value;
                    hst.Usuario = eqp.Usuario.Value;
                    hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();

                    //db.Equipamentohistoricos.Add(hst);
                    //db.SaveChanges();
                    _equipamentohistoricoRepository.Adicionar(hst);

                    trans.Commit();
                    NotificarRH(eqpto, true);
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public List<Termoentregavm> EquipamentosDoTermoDeEntrega(int cliente, int idColaborador, bool byod = false)
        {
            // ✅ CORREÇÃO: Buscar equipamentos da view
            var equipamentos = _termoentregavmRepository.Buscar(x => x.Colaboradorfinal == idColaborador && 
                                              x.Cliente == cliente &&
                                              (byod ? x.TipoAquisicao == 2 : x.TipoAquisicao != 2)).ToList();
            
            // ✅ NOVO: Buscar linhas telefônicas diretamente do banco (se não-BYOD)
            if (!byod)
            {
                try
                {
                    // ✅ CORREÇÃO: Fazer JOINs explícitos para evitar problemas com navegações
                    // Buscar dados em memória primeiro para evitar queries complexas
                    var requisicoes = _requisicaoRepository.Buscar(x => 
                        x.Cliente == cliente &&
                        x.Colaboradorfinal == idColaborador &&
                        x.Requisicaostatus == 3).ToList();
                    
                    var requisicaoIds = requisicoes.Select(r => r.Id).ToList();
                    
                    var itens = _requisicaoItensRepository.Buscar(x => 
                        requisicaoIds.Contains(x.Requisicao) &&
                        x.Linhatelefonica.HasValue &&
                        x.Linhatelefonica > 0 &&
                        x.Dtentrega.HasValue &&
                        x.Dtdevolucao == null).ToList();
                    
                    var linhaIds = itens.Select(i => i.Linhatelefonica.Value).Distinct().ToList();
                    var linhas = _telefonialinhaRepository.Buscar(x => linhaIds.Contains(x.Id)).ToList();
                    var planoIds = linhas.Select(l => l.Plano).Distinct().ToList();
                    var planos = _telefoniaplanoRepository.Buscar(x => planoIds.Contains(x.Id)).ToList();
                    var contratoIds = planos.Select(p => p.Contrato).Distinct().ToList();
                    var contratos = _telefoniacontratoRepository.Buscar(x => contratoIds.Contains(x.Id)).ToList();
                    var operadoraIds = contratos.Select(c => c.Operadora).Distinct().ToList();
                    var operadoras = _telefoniaoperadoraRepository.Buscar(x => operadoraIds.Contains(x.Id)).ToList();
                    
                    var linhasTelefonicas = (from ri in itens
                        join r in requisicoes on ri.Requisicao equals r.Id
                        join l in linhas on ri.Linhatelefonica equals l.Id
                        join p in planos on l.Plano equals p.Id
                        join c in contratos on p.Contrato equals c.Id
                        join o in operadoras on c.Operadora equals o.Id
                        select new Termoentregavm
                        {
                            Tipoequipamento = "Linha Telefônica",
                            Fabricante = o.Nome ?? "N/A",
                            Modelo = p.Nome ?? "N/A",
                            Numeroserie = l.Numero.ToString(),
                            Patrimonio = l.Numero.ToString(),
                            Dtentrega = ri.Dtentrega,
                            Observacaoentrega = ri.Observacaoentrega ?? "",
                            Dtprogramadaretorno = ri.Dtprogramadaretorno,
                            Hashrequisicao = r.Hashrequisicao ?? "",
                            Colaboradorfinal = idColaborador,
                            Cliente = cliente,
                            TipoAquisicao = 1 // Não-BYOD
                        }).ToList();
                    
                    // ✅ COMBINAR: Juntar equipamentos e linhas telefônicas
                    equipamentos.AddRange(linhasTelefonicas);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EQUIPAMENTOS_TERMO] Erro ao buscar linhas telefônicas: {ex.Message}");
                    // Continuar sem linhas telefônicas se houver erro
                }
            }
            
            return equipamentos;
        }

        public List<Vwexportacaoexcel> ExportarParaExcel(int cliente)
        {
            var eqpts = _vwexportacaoexcelRepository.Buscar(x => x.Cliente == cliente).ToList();
            return eqpts;
        }


        public void NotificarRH(Equipamento eqpto, bool perda)
        {
            try
            {
                eqpto = this.BuscarEquipamentoPorId(eqpto.Id);
                var paramts = _parametroRepository.Buscar(x => x.Cliente == eqpto.Cliente).FirstOrDefault();

                var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "notificacaoRH.html");
                var template = File.ReadAllText(file);

                template = template.Replace("@tipo", eqpto.TipoequipamentoNavigation.Descricao)
                                .Replace("@fabricante", eqpto.FabricanteNavigation.Descricao)
                                .Replace("@modelo", eqpto.ModeloNavigation.Descricao)
                                .Replace("@numeroSerie", eqpto.Numeroserie)
                                .Replace("@patrimonio", eqpto.Patrimonio)
                                .Replace("@ocorrencia", ((perda) ? "perdido/roubado" : "danificado"))
                                .Replace("@dataAtual", TimeZoneMapper.GetDateTimeNow().ToString("dddd, dd MMMM yyyy", CultureInfo.GetCultureInfo("pt-BR")));
                if(perda)
                {
                    template = template.Replace("@descricao", "<b>Descrição do reporte:</b><br clear='all'>" + eqpto.Descricaobo);
                }
                else
                {
                    var laudo = _laudoRepository.Include(x => x.TecnicoNavigation).Where(x => x.Equipamento == eqpto.Id).FirstOrDefault();
                    string descricaoLaudo = "<b>Caracteristicas da abertura do Laudo:</b>" + laudo.Descricao + "<br><br>"
                                        + "<b>Caracteristicas do encerramento do Laudo:</b>" + laudo.Laudo1 + "<br><br>"
                                        + "<b>Caracterizou mau uso?</b> " + ((laudo.Mauuso) ? "Sim" : "Não") + "<br><br>"
                                        + "<b>Técnico analista: " + laudo.TecnicoNavigation.Nome;

                    template = template.Replace("@descricao", descricaoLaudo);
                                        
                }

                var filecss = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "ckeditor.css");
                string css = File.ReadAllText(filecss);

                mail.Enviar(paramts.Emailreporte, "[SingleOne] - Reporte de dano ou perda de recurso", template);

                //Atualizo o campo de envio do e-mail de reporte
                eqpto.Enviouemailreporte = true;
                //db.Update(eqpto);
                //db.SaveChanges();
                _equipamentoRepository.Atualizar(eqpto);
            }
            catch (Exception)
            {

            }
        }


        //Descarte
        public List<DescarteVM> ListarEquipamentosDisponiveisParaDescarte(int cliente, string pesquisa)
        {
            pesquisa = pesquisa.ToUpper();
            var descartes = new List<DescarteVM>();
            //Status de equipamento disponíveis para descarte:
            // 2-Devolvido, 3-Em estoque, 9-Sinistrado
            //Status 10 = Descartado (não deve aparecer na lista)
            //Status 6 = Novo (não faz sentido descartar um item novo)
            //Outros status não devem aparecer para evitar quebrar a lógica do sistema
            
            // ✅ CORREÇÃO: Usar tabela equipamentos diretamente ao invés da view
            // A view vwexportacaoexcel tem filtros que excluem alguns registros
            var equipamentosBase = _equipamentoRepository
                .Buscar(x => x.Cliente == cliente && 
                    x.Ativo == true &&
                    (x.Equipamentostatus == 2 || x.Equipamentostatus == 3 || x.Equipamentostatus == 9) &&
                    (x.Numeroserie.ToUpper().Contains(pesquisa) || x.Patrimonio.ToUpper().Contains(pesquisa)))
                .ToList();
            
            // Converter para Vwexportacaoexcel para manter compatibilidade
            var equipamentos = equipamentosBase.Select(e => _vwexportacaoexcelRepository
                .Buscar(v => v.Id == e.Id)
                .FirstOrDefault())
                .Where(v => v != null)
                .ToList();

            // Buscar todos os cargos de confiança ativos do cliente
            var cargosConfianca = _cargoConfiancaRepository.Buscar(x => x.Cliente == cliente && x.Ativo).ToList();

            foreach(var eqp in equipamentos)
            {
                var descarte = new DescarteVM();
                descarte.Equipamento = eqp;
                descarte.CargosConfiancaEncontrados = new List<string>();
                
                //Busco o ultimo histórico do equipamento que foi entregue, para ver com quem ficou
                descarte.Historico = _equipamentohistoricoRepository.Buscar(x => x.Equipamento == eqp.Id && x.Equipamentostatus == 4).OrderByDescending(x => x.Dtregistro).Take(1).FirstOrDefault();
                if(descarte.Historico != null)
                {
                    descarte.Cargo = _colaboradoresRepository.Buscar(x => x.Id == descarte.Historico.Colaborador).Select(x => x.Cargo).FirstOrDefault();
                }

                // Buscar TODO o histórico do equipamento para verificar todos os cargos que já passaram por ele
                var historicos = _equipamentohistoricoRepository
                    .Buscar(x => x.Equipamento == eqp.Id && x.Equipamentostatus == 4 && x.Colaborador != null)
                    .ToList();

                // Para cada histórico, verificar se o colaborador tinha cargo de confiança
                foreach(var hist in historicos)
                {
                    var cargoColaborador = _colaboradoresRepository
                        .Buscar(x => x.Id == hist.Colaborador)
                        .Select(x => x.Cargo)
                        .FirstOrDefault();

                    if(!string.IsNullOrEmpty(cargoColaborador))
                    {
                        // Verificar se esse cargo está na lista de cargos de confiança
                        foreach(var cargoConf in cargosConfianca)
                        {
                            bool match = false;
                            
                            if (cargoConf.Usarpadrao)
                            {
                                // Usa LIKE '%cargo%' (match parcial)
                                match = cargoColaborador.ToUpper().Contains(cargoConf.Cargo.ToUpper());
                            }
                            else
                            {
                                // Match exato
                                match = cargoColaborador.Equals(cargoConf.Cargo, StringComparison.OrdinalIgnoreCase);
                            }

                            if(match && !descarte.CargosConfiancaEncontrados.Contains(cargoConf.Cargo))
                            {
                                descarte.CargosConfiancaEncontrados.Add(cargoConf.Cargo);
                                
                                // Aplicar os processos obrigatórios (usando OR - se qualquer cargo exigir, fica obrigatório)
                                descarte.ProcessosObrigatorios = true;
                                if(cargoConf.Obrigarsanitizacao) descarte.ObrigarSanitizacao = true;
                                if(cargoConf.Obrigardescaracterizacao) descarte.ObrigarDescaracterizacao = true;
                                if(cargoConf.Obrigarperfuracaodisco) descarte.ObrigarPerfuracaoDisco = true;
                                if(cargoConf.Obrigarevidencias) descarte.ObrigarEvidencias = true;
                                
                                // Pegar o nível de criticidade mais alto
                                if(string.IsNullOrEmpty(descarte.NivelCriticidade) || 
                                   cargoConf.Nivelcriticidade == "ALTO" ||
                                   (cargoConf.Nivelcriticidade == "MEDIO" && descarte.NivelCriticidade == "BAIXO"))
                                {
                                    descarte.NivelCriticidade = cargoConf.Nivelcriticidade;
                                }
                            }
                        }
                    }
                }

                // Carregar evidências já anexadas para este equipamento
                descarte.Evidencias = _descarteEvidenciaRepository
                    .Buscar(e => e.Equipamento == eqp.Id && e.Ativo)
                    .OrderByDescending(e => e.Dataupload)
                    .ToList();

                descartes.Add(descarte);
            }

            return descartes;
        }
        public void RealizarDescarte(List<DescarteVM> descartes)
        {
            // Validar processos obrigatórios antes de realizar o descarte
            var errosValidacao = new List<string>();
            
            foreach(var dsc in descartes)
            {
                if(dsc.ProcessosObrigatorios)
                {
                    var processosFaltando = new List<string>();
                    
                    // Se NÃO exige evidências, apenas verificar se os checkboxes foram marcados
                    if(!dsc.ObrigarEvidencias)
                    {
                        if(dsc.ObrigarSanitizacao && !dsc.SanitizacaoExecutada)
                            processosFaltando.Add("Sanitização");
                        
                        if(dsc.ObrigarDescaracterizacao && !dsc.DescaracterizacaoExecutada)
                            processosFaltando.Add("Descaracterização");
                        
                        if(dsc.ObrigarPerfuracaoDisco && !dsc.PerfuracaoDiscoExecutada)
                            processosFaltando.Add("Perfuração de Disco");
                    }
                    // Se EXIGE evidências, verificar se tem evidências FOTOGRÁFICAS de CADA processo obrigatório
                    else
                    {
                        var evidencias = _descarteEvidenciaRepository
                            .Buscar(e => e.Equipamento == dsc.Equipamento.Id && e.Ativo)
                            .ToList();
                        
                        // Verificar se tem evidências de cada tipo de processo obrigatório
                        if(dsc.ObrigarSanitizacao)
                        {
                            var temEvidenciaSanitizacao = evidencias.Any(e => e.Tipoprocesso == "SANITIZACAO");
                            if(!temEvidenciaSanitizacao)
                                processosFaltando.Add("Evidência de Sanitização (foto/arquivo)");
                        }
                        
                        if(dsc.ObrigarDescaracterizacao)
                        {
                            var temEvidenciaDescaracterizacao = evidencias.Any(e => e.Tipoprocesso == "DESCARACTERIZACAO");
                            if(!temEvidenciaDescaracterizacao)
                                processosFaltando.Add("Evidência de Descaracterização (foto/arquivo)");
                        }
                        
                        if(dsc.ObrigarPerfuracaoDisco)
                        {
                            var temEvidenciaPerfuracao = evidencias.Any(e => e.Tipoprocesso == "PERFURACAO_DISCO");
                            if(!temEvidenciaPerfuracao)
                                processosFaltando.Add("Evidência de Perfuração de Disco (foto/arquivo)");
                        }
                    }
                    
                    if(processosFaltando.Count > 0)
                    {
                        var equipIdentificacao = dsc.Equipamento.Numeroserie ?? dsc.Equipamento.Patrimonio;
                        errosValidacao.Add($"Recurso {equipIdentificacao}: processos obrigatórios não executados - {string.Join(", ", processosFaltando)}");
                    }
                }
            }
            
            if(errosValidacao.Count > 0)
            {
                throw new Exception($"Não é possível realizar o descarte. {string.Join("; ", errosValidacao)}");
            }

            using(var tran = _equipamentoRepository.BeginTransaction())
            {
                try
                {
                    foreach(var dsc in descartes)
                    {
                        var equipamento = _equipamentoRepository.Buscar(x => x.Id == dsc.Equipamento.Id).AsNoTracking().FirstOrDefault();
                        equipamento.Equipamentostatus = 10; // Status 10 = Descartado
                        //db.Update(equipamento);
                        //db.SaveChanges();
                        _equipamentoRepository.Atualizar(equipamento);

                        var historico = new Equipamentohistorico();
                        historico.Equipamento = equipamento.Id;
                        historico.Equipamentostatus = 10; // Status 10 = Descartado
                        historico.Usuario = dsc.UsuarioDescarte;
                        historico.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        //db.Add(historico);
                        //db.SaveChanges();
                        _equipamentohistoricoRepository.Adicionar(historico);
                    }
                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }


        //Reativação de equipamento
        public void ReativarEquipamento(int id)
        {
            var eqp = _equipamentoRepository.ObterPorId(id);
            try
            {
                eqp.Ativo = true;
                //db.Update(eqp);
                //db.SaveChanges();
                _equipamentoRepository.Atualizar(eqp);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public Equipamento VisualizarRecurso(int id)
        {
            // ✅ CORREÇÃO: Buscar equipamento real no banco de dados usando ViewModel
            try
            {
                Console.WriteLine($"[EQUIPAMENTO] Visualizando recurso por ID: {id}");
                
                // Usar o repositório que sabemos que funciona
                var equipamentoVM = _equipamentovmRepository
                    .Buscar(x => x.Id == id)
                    .FirstOrDefault();
                
                if (equipamentoVM == null)
                {
                    Console.WriteLine($"[EQUIPAMENTO] Recurso com ID {id} não encontrado");
                    return null;
                }
                
                // Converter ViewModel para Model
                var equipamento = new Equipamento
                {
                    Id = equipamentoVM.Id ?? 0,
                    Cliente = equipamentoVM.Cliente,
                    Tipoequipamento = equipamentoVM.Tipoequipamentoid ?? 0,
                    Fabricante = equipamentoVM.Fabricanteid ?? 0,
                    Modelo = equipamentoVM.Modeloid ?? 0,
                    Notafiscal = equipamentoVM.Notafiscalid,
                    Equipamentostatus = equipamentoVM.Equipamentostatusid,
                    Usuario = equipamentoVM.Usuarioid,
                    Tipoaquisicao = equipamentoVM.TipoAquisicao,
                    Fornecedor = equipamentoVM.Fornecedor,
                    Possuibo = equipamentoVM.Possuibo ?? false,
                    Descricaobo = equipamentoVM.Descricaobo,
                    Numeroserie = equipamentoVM.Numeroserie,
                    Patrimonio = equipamentoVM.Patrimonio,
                    Dtlimitegarantia = equipamentoVM.Dtlimitegarantia,
                    Dtcadastro = equipamentoVM.Dtcadastro ?? DateTime.Now,
                    Ativo = equipamentoVM.Ativo ?? true,
                    Migrateid = null, // Não disponível no ViewModel
                    Enviouemailreporte = null, // Não disponível no ViewModel
                    Empresa = equipamentoVM.Empresaid,
                    Centrocusto = equipamentoVM.Centrocustoid,
                    Contrato = equipamentoVM.Contratoid,
                    FilialId = null, // Não disponível no ViewModel
                    Localidade = equipamentoVM.Localizacaoid, // ✅ CORREÇÃO: Usar Localizacaoid
                    // Localizacao = equipamentoVM.Localizacaoid // ✅ REMOVIDO: Coluna localizacao não existe mais no banco
                };
                
                Console.WriteLine($"[EQUIPAMENTO] Recurso encontrado: ID={equipamento.Id}, S/N={equipamento.Numeroserie}, Patrimônio={equipamento.Patrimonio}");
                return equipamento;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EQUIPAMENTO] Erro ao visualizar recurso ID {id}: {ex.Message}");
                Console.WriteLine($"[EQUIPAMENTO] Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
