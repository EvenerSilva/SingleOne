using Microsoft.EntityFrameworkCore;
using SingleOne.Util;
using SingleOne.Models;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Models.Enums;
using SingleOneAPI.Models.ViewModels;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios
{
    /// <summary>
    /// Classe de negócio para Protocolo de Descarte
    /// </summary>
    public class ProtocoloDescarteNegocio : IProtocoloDescarteNegocio
    {
        private readonly IRepository<ProtocoloDescarte> _protocoloRepository;
        private readonly IRepository<ProtocoloDescarteItem> _itemRepository;
        private readonly IRepository<DescarteEvidencia> _evidenciaRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Cliente> _clienteRepository;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<CargoConfianca> _cargoConfiancaRepository;
        private readonly IRepository<Equipamentohistorico> _equipamentoHistoricoRepository;
        private readonly IRepository<Colaboradore> _colaboradoresRepository;
        private readonly IRepository<Template> _templateRepository;

        public ProtocoloDescarteNegocio(
            IRepository<ProtocoloDescarte> protocoloRepository,
            IRepository<ProtocoloDescarteItem> itemRepository,
            IRepository<DescarteEvidencia> evidenciaRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Cliente> clienteRepository,
            IRepository<Usuario> usuarioRepository,
            IRepository<CargoConfianca> cargoConfiancaRepository,
            IRepository<Equipamentohistorico> equipamentoHistoricoRepository,
            IRepository<Colaboradore> colaboradoresRepository,
            IRepository<Template> templateRepository)
        {
            _protocoloRepository = protocoloRepository;
            _itemRepository = itemRepository;
            _evidenciaRepository = evidenciaRepository;
            _equipamentoRepository = equipamentoRepository;
            _clienteRepository = clienteRepository;
            _usuarioRepository = usuarioRepository;
            _cargoConfiancaRepository = cargoConfiancaRepository;
            _equipamentoHistoricoRepository = equipamentoHistoricoRepository;
            _colaboradoresRepository = colaboradoresRepository;
            _templateRepository = templateRepository;
        }

        /// <summary>
        /// Listar protocolos de descarte de um cliente
        /// </summary>
        public async Task<List<ProtocoloDescarteVM>> ListarProtocolos(int clienteId, bool incluirInativos = false)
        {
            var query = _protocoloRepository
                .Buscar(p => p.Cliente == clienteId)
                .Include(p => p.ClienteNavigation)
                .Include(p => p.ResponsavelNavigation)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.EquipamentoNavigation)
                .AsQueryable();

            if (!incluirInativos)
                query = query.Where(p => p.Ativo);

            var protocolos = await query
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            return protocolos.Select(ConverterParaVM).ToList();
        }

        /// <summary>
        /// Obter protocolo específico por ID
        /// </summary>
        public async Task<ProtocoloDescarteVM> ObterProtocolo(int protocoloId)
        {
            var protocolo = await _protocoloRepository
                .Buscar(p => p.Id == protocoloId)
                .Include(p => p.ClienteNavigation)
                .Include(p => p.ResponsavelNavigation)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.EquipamentoNavigation)
                .FirstOrDefaultAsync();

            if (protocolo == null)
                return null;

            return ConverterParaVM(protocolo);
        }

        /// <summary>
        /// Criar novo protocolo de descarte
        /// </summary>
        public async Task<ProtocoloDescarteVM> CriarProtocolo(ProtocoloDescarteVM protocoloVM, int usuarioId)
        {
            var protocolo = new ProtocoloDescarte
            {
                Protocolo = await GerarNumeroProtocolo(),
                Cliente = protocoloVM.Cliente,
                TipoDescarte = protocoloVM.TipoDescarte,
                MotivoDescarte = protocoloVM.MotivoDescarte,
                DestinoFinal = protocoloVM.DestinoFinal,
                // ✅ NOVO: Campos de Logística Reversa
                EmpresaDestinoFinal = protocoloVM.EmpresaDestinoFinal,
                CnpjDestinoFinal = protocoloVM.CnpjDestinoFinal,
                CertificadoDescarte = protocoloVM.CertificadoDescarte,
                // ✅ NOVO: Campos do MTR
                MtrObrigatorio = protocoloVM.MtrObrigatorio,
                MtrNumero = protocoloVM.MtrNumero,
                MtrEmitidoPor = protocoloVM.MtrEmitidoPor,
                MtrDataEmissao = protocoloVM.MtrDataEmissao,
                MtrValidade = protocoloVM.MtrValidade,
                MtrArquivo = protocoloVM.MtrArquivo,
                MtrEmpresaTransportadora = protocoloVM.MtrEmpresaTransportadora,
                MtrCnpjTransportadora = protocoloVM.MtrCnpjTransportadora,
                MtrPlacaVeiculo = protocoloVM.MtrPlacaVeiculo,
                MtrMotorista = protocoloVM.MtrMotorista,
                MtrCpfMotorista = protocoloVM.MtrCpfMotorista,
                // Campos padrão
                ResponsavelProtocolo = usuarioId,
                DataCriacao = TimeZoneMapper.GetDateTimeNow(),
                Status = "EM_ANDAMENTO",
                ValorTotalEstimado = protocoloVM.ValorTotalEstimado,
                Observacoes = protocoloVM.Observacoes,
                Ativo = true
            };

            _protocoloRepository.Adicionar(protocolo);
            _protocoloRepository.SalvarAlteracoes();

            return await ObterProtocolo(protocolo.Id);
        }

        /// <summary>
        /// Atualizar protocolo existente
        /// </summary>
        public async Task<ProtocoloDescarteVM> AtualizarProtocolo(ProtocoloDescarteVM protocoloVM, int usuarioId)
        {
            var protocolo = _protocoloRepository.ObterPorId(protocoloVM.Id);
            if (protocolo == null)
                throw new Exception("Protocolo não encontrado");

            // Só permite editar se ainda estiver em andamento
            if (protocolo.Status != "EM_ANDAMENTO")
                throw new Exception("Só é possível editar protocolos em andamento");

            protocolo.TipoDescarte = protocoloVM.TipoDescarte;
            protocolo.MotivoDescarte = protocoloVM.MotivoDescarte;
            protocolo.DestinoFinal = protocoloVM.DestinoFinal;
            protocolo.ValorTotalEstimado = protocoloVM.ValorTotalEstimado;
            protocolo.Observacoes = protocoloVM.Observacoes;
            
            // ✅ NOVO: Atualizar campos de Logística Reversa
            protocolo.EmpresaDestinoFinal = protocoloVM.EmpresaDestinoFinal;
            protocolo.CnpjDestinoFinal = protocoloVM.CnpjDestinoFinal;
            protocolo.CertificadoDescarte = protocoloVM.CertificadoDescarte;
            
            // ✅ NOVO: Atualizar campos do MTR
            protocolo.MtrObrigatorio = protocoloVM.MtrObrigatorio;
            protocolo.MtrNumero = protocoloVM.MtrNumero;
            protocolo.MtrEmitidoPor = protocoloVM.MtrEmitidoPor;
            protocolo.MtrDataEmissao = protocoloVM.MtrDataEmissao;
            protocolo.MtrValidade = protocoloVM.MtrValidade;
            protocolo.MtrArquivo = protocoloVM.MtrArquivo;
            protocolo.MtrEmpresaTransportadora = protocoloVM.MtrEmpresaTransportadora;
            protocolo.MtrCnpjTransportadora = protocoloVM.MtrCnpjTransportadora;
            protocolo.MtrPlacaVeiculo = protocoloVM.MtrPlacaVeiculo;
            protocolo.MtrMotorista = protocoloVM.MtrMotorista;
            protocolo.MtrCpfMotorista = protocoloVM.MtrCpfMotorista;

            _protocoloRepository.Atualizar(protocolo);
            _protocoloRepository.SalvarAlteracoes();

            return await ObterProtocolo(protocolo.Id);
        }

        /// <summary>
        /// Adicionar equipamento ao protocolo
        /// </summary>
        public async Task<ProtocoloDescarteItemVM> AdicionarEquipamento(int protocoloId, int equipamentoId, int usuarioId)
        {
            // Verificar se o protocolo existe e está em andamento
            var protocolo = _protocoloRepository.ObterPorId(protocoloId);
            if (protocolo == null)
                throw new Exception("Protocolo não encontrado");
            if (protocolo.Status != "EM_ANDAMENTO")
                throw new Exception("Só é possível adicionar equipamentos em protocolos em andamento");

            // Verificar se o equipamento já está no protocolo
            var itemExistente = await _itemRepository
                .Buscar(i => i.ProtocoloId == protocoloId && i.Equipamento == equipamentoId && i.Ativo)
                .FirstOrDefaultAsync();
            if (itemExistente != null)
                throw new Exception("Equipamento já está no protocolo");

            // Verificar se o equipamento está disponível para descarte
            var equipamento = _equipamentoRepository.ObterPorId(equipamentoId);
            if (equipamento == null)
                throw new Exception("Equipamento não encontrado");
            if (equipamento.Ativo == false)
                throw new Exception("Equipamento inativo não pode ser descartado");

            Console.WriteLine($"[DESCARTE] Validando equipamento {equipamentoId}: Status={equipamento.Equipamentostatus}, TipoAquisicao={equipamento.Tipoaquisicao}");

            // ✅ VALIDAR STATUS DO EQUIPAMENTO:
            // Regra 1: Status 9 (Sinistrado) pode ser descartado - já passou por laudo técnico
            // Regra 2: BYOD (TipoAquisicao = 2) pode ser descartado em qualquer status EXCETO 4 (Entregue) e 10 (Descartado)
            bool isByod = equipamento.Tipoaquisicao == 2;
            bool statusPermitido = false;
            
            if (isByod)
            {
                // BYOD: permitir descarte em qualquer status EXCETO Entregue (4) e Descartado (10)
                statusPermitido = equipamento.Equipamentostatus.HasValue && 
                                  equipamento.Equipamentostatus.Value != 4 && 
                                  equipamento.Equipamentostatus.Value != 10;
                
                if (!statusPermitido)
                {
                    var statusId = equipamento.Equipamentostatus ?? 0;
                    var mensagemStatus = StatusDescarteEnum.ObterMensagemStatus(statusId);
                    throw new Exception($"Equipamento BYOD não pode ser descartado no status atual. {mensagemStatus}");
                }
            }
            else
            {
                // Equipamento corporativo: apenas Status 9 (Sinistrado) pode ser descartado
                if (!equipamento.Equipamentostatus.HasValue || equipamento.Equipamentostatus.Value != 9)
                {
                    var statusId = equipamento.Equipamentostatus ?? 0;
                    var mensagemStatus = StatusDescarteEnum.ObterMensagemStatus(statusId);
                    throw new Exception($"Equipamento não pode ser descartado. {mensagemStatus}");
                }
                statusPermitido = true;
            }

            // ✅ Verificar cargos de confiança no histórico do equipamento
            var cargosConfianca = _cargoConfiancaRepository
                .Buscar(c => c.Cliente == equipamento.Cliente && c.Ativo)
                .ToList();

            bool processosObrigatorios = false;
            bool obrigarSanitizacao = false;
            bool obrigarDescaracterizacao = false;
            bool obrigarPerfuracaoDisco = false;
            bool obrigarEvidencias = false;

            if (cargosConfianca.Any())
            {
                // Buscar histórico do equipamento quando estava entregue (status 4)
                var historicos = _equipamentoHistoricoRepository
                    .Buscar(h => h.Equipamento == equipamentoId && h.Equipamentostatus == 4 && h.Colaborador != null)
                    .ToList();

                foreach (var historico in historicos)
                {
                    var cargoColaborador = _colaboradoresRepository
                        .Buscar(c => c.Id == historico.Colaborador)
                        .Select(c => c.Cargo)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(cargoColaborador))
                    {
                        foreach (var cargoConf in cargosConfianca)
                        {
                            bool match = cargoConf.Usarpadrao
                                ? cargoColaborador.ToUpper().Contains(cargoConf.Cargo.ToUpper())
                                : cargoColaborador.Equals(cargoConf.Cargo, StringComparison.OrdinalIgnoreCase);

                            if (match)
                            {
                                processosObrigatorios = true;
                                if (cargoConf.Obrigarsanitizacao) obrigarSanitizacao = true;
                                if (cargoConf.Obrigardescaracterizacao) obrigarDescaracterizacao = true;
                                if (cargoConf.Obrigarperfuracaodisco) obrigarPerfuracaoDisco = true;
                                if (cargoConf.Obrigarevidencias) obrigarEvidencias = true;
                            }
                        }
                    }
                }
            }

            // ✅ CORRIGIDO: Equipamentos sem processos obrigatórios devem ser CONCLUIDO automaticamente
            var statusInicial = processosObrigatorios ? "PENDENTE" : "CONCLUIDO";
            var dataProcessoConcluido = processosObrigatorios ? (DateTime?)null : TimeZoneMapper.GetDateTimeNow();

            // Criar item do protocolo com processos obrigatórios configurados
            var item = new ProtocoloDescarteItem
            {
                ProtocoloId = protocoloId,
                Equipamento = equipamentoId,
                ProcessosObrigatorios = processosObrigatorios,
                ObrigarSanitizacao = obrigarSanitizacao,
                ObrigarDescaracterizacao = obrigarDescaracterizacao,
                ObrigarPerfuracaoDisco = obrigarPerfuracaoDisco,
                EvidenciasObrigatorias = obrigarEvidencias,
                ProcessoSanitizacao = false,
                ProcessoDescaracterizacao = false,
                ProcessoPerfuracaoDisco = false,
                EvidenciasExecutadas = false,
                StatusItem = statusInicial,
                DataProcessoIniciado = TimeZoneMapper.GetDateTimeNow(),
                DataProcessoConcluido = dataProcessoConcluido,
                Ativo = true
            };

            _itemRepository.Adicionar(item);
            _itemRepository.SalvarAlteracoes();

            return await ConverterItemParaVM(item);
        }

        /// <summary>
        /// Remover equipamento do protocolo
        /// </summary>
        public async Task<bool> RemoverEquipamento(int protocoloId, int equipamentoId, int usuarioId)
        {
            var item = await _itemRepository
                .Buscar(i => i.ProtocoloId == protocoloId && i.Equipamento == equipamentoId && i.Ativo)
                .FirstOrDefaultAsync();

            if (item == null)
                return false;

            // Só permite remover se o protocolo estiver em andamento
            var protocolo = _protocoloRepository.ObterPorId(protocoloId);
            if (protocolo.Status != "EM_ANDAMENTO")
                throw new Exception("Só é possível remover equipamentos de protocolos em andamento");

            item.Ativo = false;
            _itemRepository.Atualizar(item);
            _itemRepository.SalvarAlteracoes();

            return true;
        }

        /// <summary>
        /// Atualizar um processo específico de um item
        /// </summary>
        public async Task<ProtocoloDescarteItemVM> AtualizarProcessoItem(int itemId, string processo, bool valor, int usuarioId)
        {
            var item = _itemRepository.ObterPorId(itemId);
            if (item == null)
                throw new Exception("Item não encontrado");

            // Atualizar o processo específico
            switch (processo.ToLower())
            {
                case "sanitizacao":
                    item.ProcessoSanitizacao = valor;
                    break;
                case "descaracterizacao":
                    item.ProcessoDescaracterizacao = valor;
                    break;
                case "perfuracao":
                    item.ProcessoPerfuracaoDisco = valor;
                    break;
                case "evidencias":
                    item.EvidenciasExecutadas = valor;
                    break;
                default:
                    throw new Exception($"Processo '{processo}' não reconhecido");
            }

            // Atualizar status do item baseado nos processos
            AtualizarStatusItem(item);

            _itemRepository.Atualizar(item);
            _itemRepository.SalvarAlteracoes();

            return await ConverterItemParaVM(item);
        }

        /// <summary>
        /// Atualizar campo específico de um item (método sanitização, ferramenta, observações)
        /// </summary>
        public async Task AtualizarCampoItem(int itemId, string campo, string valor, int usuarioId)
        {
            var item = _itemRepository.ObterPorId(itemId);
            if (item == null)
                throw new Exception("Item não encontrado");

            // Atualizar o campo específico
            switch (campo.ToLower())
            {
                case "metodosanitizacao":
                    item.MetodoSanitizacao = valor;
                    break;
                case "ferramentautilizada":
                    item.FerramentaUtilizada = valor;
                    break;
                case "observacoessanitizacao":
                    item.ObservacoesSanitizacao = valor;
                    break;
                default:
                    throw new Exception($"Campo '{campo}' não reconhecido");
            }

            _itemRepository.Atualizar(item);
            _itemRepository.SalvarAlteracoes();
        }

        /// <summary>
        /// Atualizar status do item baseado no progresso dos processos
        /// </summary>
        private void AtualizarStatusItem(ProtocoloDescarteItem item)
        {
            // Se algum processo foi iniciado, status = EM_PROCESSO
            bool algumaExecutado = item.ProcessoSanitizacao || item.ProcessoDescaracterizacao || 
                                   item.ProcessoPerfuracaoDisco || item.EvidenciasExecutadas;

            // Verificar se todos processos obrigatórios foram concluídos
            bool todosProcessosConcluidos = true;
            if (item.ObrigarSanitizacao && !item.ProcessoSanitizacao) todosProcessosConcluidos = false;
            if (item.ObrigarDescaracterizacao && !item.ProcessoDescaracterizacao) todosProcessosConcluidos = false;
            if (item.ObrigarPerfuracaoDisco && !item.ProcessoPerfuracaoDisco) todosProcessosConcluidos = false;
            if (item.EvidenciasObrigatorias && !item.EvidenciasExecutadas) todosProcessosConcluidos = false;

            if (todosProcessosConcluidos && item.ProcessosObrigatorios)
            {
                item.StatusItem = "CONCLUIDO";
                item.DataProcessoConcluido = TimeZoneMapper.GetDateTimeNow();
            }
            else if (algumaExecutado)
            {
                item.StatusItem = "EM_PROCESSO";
                if (item.DataProcessoIniciado == null)
                    item.DataProcessoIniciado = TimeZoneMapper.GetDateTimeNow();
            }
            else
            {
                item.StatusItem = "PENDENTE";
            }
        }

        /// <summary>
        /// Atualizar status de processo de um equipamento no protocolo
        /// </summary>
        public async Task<ProtocoloDescarteItemVM> AtualizarProcessoEquipamento(int itemId, bool sanitizacao, bool descaracterizacao, bool perfuracao, bool evidencias, int usuarioId)
        {
            var item = _itemRepository.ObterPorId(itemId);
            if (item == null)
                throw new Exception("Item do protocolo não encontrado");

            item.ProcessoSanitizacao = sanitizacao;
            item.ProcessoDescaracterizacao = descaracterizacao;
            item.ProcessoPerfuracaoDisco = perfuracao;
            item.EvidenciasExecutadas = evidencias;

            // Atualizar status do item
            if (item.StatusItem == "PENDENTE")
                item.StatusItem = "EM_PROCESSO";

            // Verificar se está concluído
            if (sanitizacao && descaracterizacao && perfuracao && evidencias)
            {
                item.StatusItem = "CONCLUIDO";
                item.DataProcessoConcluido = TimeZoneMapper.GetDateTimeNow();
            }

            _itemRepository.Atualizar(item);
            _itemRepository.SalvarAlteracoes();

            return await ConverterItemParaVM(item);
        }

        /// <summary>
        /// Finalizar protocolo (quando todos os equipamentos estão prontos)
        /// </summary>
        public async Task<ProtocoloDescarteVM> FinalizarProtocolo(int protocoloId, int usuarioId)
        {
            var protocolo = _protocoloRepository.ObterPorId(protocoloId);
            if (protocolo == null)
                throw new Exception("Protocolo não encontrado");
            if (protocolo.Status != "EM_ANDAMENTO")
                throw new Exception("Só é possível finalizar protocolos em andamento");

            // ✅ CORRIGIDO: Verificar se todos os equipamentos podem ser finalizados
            // (considerando equipamentos sem processos obrigatórios)
            var itens = await _itemRepository
                .Buscar(i => i.ProtocoloId == protocoloId && i.Ativo)
                .ToListAsync();

            var itensNaoFinalizaveis = itens.Where(i => {
                // Se o equipamento tem processos obrigatórios, deve estar CONCLUIDO
                if (i.ProcessosObrigatorios)
                {
                    return i.StatusItem != "CONCLUIDO";
                }
                // Se não tem processos obrigatórios, pode estar em qualquer status
                else
                {
                    return false; // Sempre pode finalizar
                }
            }).ToList();

            if (itensNaoFinalizaveis.Any())
                throw new Exception("Não é possível finalizar protocolo com equipamentos que possuem processos obrigatórios não concluídos");

            // ✅ Atualizar status de TODOS os equipamentos do protocolo para DESCARTADO (status 10)
            var itensDoProtocolo = await _itemRepository
                .Buscar(i => i.ProtocoloId == protocoloId && i.Ativo)
                .ToListAsync();

            var dataFinalizacao = TimeZoneMapper.GetDateTimeNow();

            foreach (var item in itensDoProtocolo)
            {
                var equipamento = _equipamentoRepository.ObterPorId(item.Equipamento);
                if (equipamento != null)
                {
                    // Atualizar status do equipamento
                    equipamento.Equipamentostatus = 10; // Status 10 = Descartado
                    _equipamentoRepository.Atualizar(equipamento);

                    // 📋 Registrar no histórico (timeline) do equipamento
                    var historico = new Equipamentohistorico
                    {
                        Equipamento = equipamento.Id,
                        Equipamentostatus = 10, // Descartado
                        Usuario = usuarioId,
                        Colaborador = null,
                        Requisicao = null,
                        Linhatelefonica = null,
                        Linhaemuso = null,
                        Dtregistro = dataFinalizacao
                    };
                    _equipamentoHistoricoRepository.Adicionar(historico);
                }
            }

            protocolo.Status = "CONCLUIDO";
            protocolo.DataConclusao = dataFinalizacao;

            _protocoloRepository.Atualizar(protocolo);
            _protocoloRepository.SalvarAlteracoes();

            return await ObterProtocolo(protocolo.Id);
        }

        /// <summary>
        /// Cancelar protocolo
        /// </summary>
        public async Task<bool> CancelarProtocolo(int protocoloId, int usuarioId)
        {
            var protocolo = _protocoloRepository.ObterPorId(protocoloId);
            if (protocolo == null)
                return false;

            if (protocolo.Status != "EM_ANDAMENTO")
                throw new Exception("Só é possível cancelar protocolos em andamento");

            protocolo.Status = "CANCELADO";
            _protocoloRepository.Atualizar(protocolo);
            _protocoloRepository.SalvarAlteracoes();

            return true;
        }

        /// <summary>
        /// Gerar número de protocolo único
        /// </summary>
        public async Task<string> GerarNumeroProtocolo()
        {
            var ano = DateTime.Now.Year;
            var prefixo = $"DESC-{ano}-";
            
            var ultimoProtocolo = await _protocoloRepository
                .Buscar(p => p.Protocolo.StartsWith(prefixo))
                .OrderByDescending(p => p.Protocolo)
                .FirstOrDefaultAsync();

            int proximoNumero = 1;
            if (ultimoProtocolo != null)
            {
                var numeroStr = ultimoProtocolo.Protocolo.Substring(prefixo.Length);
                if (int.TryParse(numeroStr, out int ultimoNumero))
                    proximoNumero = ultimoNumero + 1;
            }

            return $"{prefixo}{proximoNumero:D6}";
        }

        /// <summary>
        /// Validar se protocolo pode ser finalizado
        /// </summary>
        public async Task<bool> ValidarFinalizacaoProtocolo(int protocoloId)
        {
            var itens = await _itemRepository
                .Buscar(i => i.ProtocoloId == protocoloId && i.Ativo)
                .ToListAsync();

            // ✅ CORRIGIDO: Lógica para validação considera equipamentos sem processos obrigatórios
            return itens.All(i => {
                // Se o equipamento tem processos obrigatórios, deve estar CONCLUIDO
                if (i.ProcessosObrigatorios)
                {
                    return i.StatusItem == "CONCLUIDO";
                }
                // Se não tem processos obrigatórios, pode estar PENDENTE, EM_PROCESSO ou CONCLUIDO
                else
                {
                    return i.StatusItem == "PENDENTE" || i.StatusItem == "EM_PROCESSO" || i.StatusItem == "CONCLUIDO";
                }
            });
        }

        /// <summary>
        /// Listar equipamentos disponíveis para adicionar ao protocolo
        /// Aplica regras de boas práticas: 
        /// - Status 9 (Sinistrado) pode ser descartado
        /// - BYOD (TipoAquisicao = 2) pode ser descartado em qualquer status, EXCETO Entregue (4) e Descartado (10)
        /// </summary>
        public async Task<List<EquipamentoDisponivelVM>> ListarEquipamentosDisponiveis(int clienteId, string filtro = "")
        {
            Console.WriteLine($"[DESCARTE] Listando equipamentos disponíveis para cliente {clienteId}");
            
            // ✅ REGRAS DE DESCARTE:
            // 1. Status 9 = Sinistrado (já passou por laudo técnico e foi avaliado como sem conserto)
            // 2. BYOD (TipoAquisicao = 2 - Particular) em qualquer status EXCETO: 4 (Entregue) e 10 (Descartado)
            var query = _equipamentoRepository
                .Buscar(e => e.Cliente == clienteId 
                          && e.Ativo == true 
                          && (e.Equipamentostatus == 9 || // Status 9 = Sinistrado (regra original)
                              (e.Tipoaquisicao == 2 && e.Equipamentostatus != 4 && e.Equipamentostatus != 10))) // BYOD (exceto Entregue e Descartado)
                .Include(e => e.FabricanteNavigation)
                .Include(e => e.ModeloNavigation)
                .Include(e => e.EquipamentostatusNavigation)
                .Include(e => e.TipoequipamentoNavigation)
                .Include(e => e.TipoaquisicaoNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.ToUpper();
                query = query.Where(e => 
                    e.Numeroserie.ToUpper().Contains(filtro) || 
                    e.Patrimonio.ToUpper().Contains(filtro) ||
                    e.FabricanteNavigation.Descricao.ToUpper().Contains(filtro) ||
                    e.ModeloNavigation.Descricao.ToUpper().Contains(filtro));
            }

            var equipamentos = await query.ToListAsync();
            
            Console.WriteLine($"[DESCARTE] Total de equipamentos encontrados: {equipamentos.Count}");
            var byodCount = equipamentos.Count(e => e.Tipoaquisicao == 2);
            var sinistradoCount = equipamentos.Count(e => e.Equipamentostatus == 9);
            Console.WriteLine($"[DESCARTE]   - BYOD: {byodCount}");
            Console.WriteLine($"[DESCARTE]   - Sinistrado: {sinistradoCount}");

            // Verificar quais já estão em protocolos ativos
            var equipamentosEmProtocolos = await _itemRepository
                .Buscar(i => i.Ativo && i.Protocolo.Status == "EM_ANDAMENTO")
                .Include(i => i.Protocolo)
                .ToListAsync();

            return equipamentos.Select(e => new EquipamentoDisponivelVM
            {
                Id = e.Id,
                NumeroSerie = e.Numeroserie,
                Patrimonio = e.Patrimonio,
                Descricao = $"{e.FabricanteNavigation?.Descricao} {e.ModeloNavigation?.Descricao}",
                Fabricante = e.FabricanteNavigation?.Descricao,
                Modelo = e.ModeloNavigation?.Descricao,
                Status = e.EquipamentostatusNavigation?.Descricao,
                TipoEquipamento = e.TipoequipamentoNavigation?.Descricao,
                TipoAquisicao = e.TipoaquisicaoNavigation?.Nome,
                JaEstaEmProtocolo = equipamentosEmProtocolos.Any(ep => ep.Equipamento == e.Id),
                ProtocoloId = equipamentosEmProtocolos.FirstOrDefault(ep => ep.Equipamento == e.Id)?.ProtocoloId,
                NumeroProtocolo = equipamentosEmProtocolos.FirstOrDefault(ep => ep.Equipamento == e.Id)?.Protocolo.Protocolo,
                ProcessosObrigatorios = false, // TODO: Implementar lógica de cargos de confiança
                ObrigarSanitizacao = false,
                ObrigarDescaracterizacao = false,
                ObrigarPerfuracaoDisco = false,
                ObrigarEvidencias = false
            }).ToList();
        }

        /// <summary>
        /// Obter estatísticas do protocolo
        /// </summary>
        public async Task<EstatisticasProtocoloVM> ObterEstatisticas(int protocoloId)
        {
            var protocolo = _protocoloRepository.ObterPorId(protocoloId);
            if (protocolo == null)
                return null;

            var itens = await _itemRepository
                .Buscar(i => i.ProtocoloId == protocoloId && i.Ativo)
                .ToListAsync();

            var evidencias = await _evidenciaRepository
                .Buscar(e => e.ProtocoloId == protocoloId && e.Ativo)
                .CountAsync();

            // ✅ CORRIGIDO: Lógica para PodeFinalizar considera equipamentos sem processos obrigatórios
            var podeFinalizar = itens.All(i => {
                // Se o equipamento tem processos obrigatórios, deve estar CONCLUIDO
                if (i.ProcessosObrigatorios)
                {
                    return i.StatusItem == "CONCLUIDO";
                }
                // Se não tem processos obrigatórios, pode estar PENDENTE, EM_PROCESSO ou CONCLUIDO
                else
                {
                    return i.StatusItem == "PENDENTE" || i.StatusItem == "EM_PROCESSO" || i.StatusItem == "CONCLUIDO";
                }
            });

            return new EstatisticasProtocoloVM
            {
                ProtocoloId = protocolo.Id,
                NumeroProtocolo = protocolo.Protocolo,
                TotalEquipamentos = itens.Count,
                EquipamentosPendentes = itens.Count(i => i.StatusItem == "PENDENTE"),
                EquipamentosEmProcesso = itens.Count(i => i.StatusItem == "EM_PROCESSO"),
                EquipamentosConcluidos = itens.Count(i => i.StatusItem == "CONCLUIDO"),
                PercentualConclusao = itens.Count > 0 ? (decimal)itens.Count(i => i.StatusItem == "CONCLUIDO") / itens.Count * 100 : 0,
                TotalEvidencias = evidencias,
                ValorTotalEstimado = protocolo.ValorTotalEstimado,
                PodeFinalizar = podeFinalizar,
                StatusProtocolo = protocolo.Status,
                DataCriacao = protocolo.DataCriacao,
                DataConclusao = protocolo.DataConclusao
            };
        }

        #region Métodos Auxiliares

        /// <summary>
        /// Converter ProtocoloDescarte para ProtocoloDescarteVM
        /// </summary>
        private ProtocoloDescarteVM ConverterParaVM(ProtocoloDescarte protocolo)
        {
            // Converter itens do protocolo
            var itensVM = new List<ProtocoloDescarteItemVM>();
            if (protocolo.Itens != null && protocolo.Itens.Any())
            {
                foreach (var item in protocolo.Itens.Where(i => i.Ativo))
                {
                    var itemVM = ConverterItemParaVMSimples(item);
                    itensVM.Add(itemVM);
                }
            }

            var vm = new ProtocoloDescarteVM
            {
                Id = protocolo.Id,
                Protocolo = protocolo.Protocolo,
                Cliente = protocolo.Cliente,
                NomeCliente = protocolo.ClienteNavigation?.Razaosocial,
                TipoDescarte = protocolo.TipoDescarte,
                TipoDescarteDescricao = ObterDescricaoTipoDescarte(protocolo.TipoDescarte),
                MotivoDescarte = protocolo.MotivoDescarte,
                DestinoFinal = protocolo.DestinoFinal,
                EmpresaDestinoFinal = protocolo.EmpresaDestinoFinal,
                CnpjDestinoFinal = protocolo.CnpjDestinoFinal,
                CertificadoDescarte = protocolo.CertificadoDescarte,
                // Campos do MTR
                MtrObrigatorio = protocolo.MtrObrigatorio,
                MtrNumero = protocolo.MtrNumero,
                MtrEmitidoPor = protocolo.MtrEmitidoPor,
                MtrDataEmissao = protocolo.MtrDataEmissao,
                MtrValidade = protocolo.MtrValidade,
                MtrArquivo = protocolo.MtrArquivo,
                MtrEmpresaTransportadora = protocolo.MtrEmpresaTransportadora,
                MtrCnpjTransportadora = protocolo.MtrCnpjTransportadora,
                MtrPlacaVeiculo = protocolo.MtrPlacaVeiculo,
                MtrMotorista = protocolo.MtrMotorista,
                MtrCpfMotorista = protocolo.MtrCpfMotorista,
                // Demais campos
                ResponsavelProtocolo = protocolo.ResponsavelProtocolo,
                NomeResponsavel = protocolo.ResponsavelNavigation?.Nome,
                DataCriacao = protocolo.DataCriacao,
                DataConclusao = protocolo.DataConclusao,
                Status = protocolo.Status,
                StatusDescricao = ObterDescricaoStatus(protocolo.Status),
                ValorTotalEstimado = protocolo.ValorTotalEstimado,
                DocumentoGerado = protocolo.DocumentoGerado,
                CaminhoDocumento = protocolo.CaminhoDocumento,
                Observacoes = protocolo.Observacoes,
                Ativo = protocolo.Ativo,
                Itens = itensVM,
                QuantidadeEquipamentos = itensVM.Count,
                QuantidadeConcluidos = itensVM.Count(i => i.StatusItem == "CONCLUIDO")
            };

            vm.PercentualConclusao = vm.QuantidadeEquipamentos > 0 ? 
                (decimal)vm.QuantidadeConcluidos / vm.QuantidadeEquipamentos * 100 : 0;
            vm.PodeFinalizar = vm.QuantidadeEquipamentos > 0 && vm.QuantidadeEquipamentos == vm.QuantidadeConcluidos;

            return vm;
        }

        /// <summary>
        /// Converter ProtocoloDescarteItem para VM simplificado (sem async)
        /// </summary>
        private ProtocoloDescarteItemVM ConverterItemParaVMSimples(ProtocoloDescarteItem item)
        {
            var equipamento = _equipamentoRepository
                .Buscar(e => e.Id == item.Equipamento)
                .Include(e => e.FabricanteNavigation)
                .Include(e => e.ModeloNavigation)
                .Include(e => e.EquipamentostatusNavigation)
                .Include(e => e.TipoequipamentoNavigation)
                .Include(e => e.TipoaquisicaoNavigation)
                .FirstOrDefault();

            return new ProtocoloDescarteItemVM
            {
                Id = item.Id,
                ProtocoloId = item.ProtocoloId,
                Equipamento = item.Equipamento,
                EquipamentoNavigation = equipamento != null ? new SingleOne.Models.Equipamentovm
                {
                    Id = equipamento.Id,
                    Numeroserie = equipamento.Numeroserie,
                    Patrimonio = equipamento.Patrimonio,
                    Fabricante = equipamento.FabricanteNavigation?.Descricao,
                    Modelo = equipamento.ModeloNavigation?.Descricao,
                    Equipamentostatus = equipamento.EquipamentostatusNavigation?.Descricao,
                    Tipoequipamento = equipamento.TipoequipamentoNavigation?.Descricao,
                    TipoAquisicaoNome = equipamento.TipoaquisicaoNavigation?.Nome
                } : null,
                ProcessosObrigatorios = item.ProcessosObrigatorios,
                ObrigarSanitizacao = item.ObrigarSanitizacao,
                ObrigarDescaracterizacao = item.ObrigarDescaracterizacao,
                ObrigarPerfuracaoDisco = item.ObrigarPerfuracaoDisco,
                EvidenciasObrigatorias = item.EvidenciasObrigatorias,
                ProcessoSanitizacao = item.ProcessoSanitizacao,
                ProcessoDescaracterizacao = item.ProcessoDescaracterizacao,
                ProcessoPerfuracaoDisco = item.ProcessoPerfuracaoDisco,
                EvidenciasExecutadas = item.EvidenciasExecutadas,
                ValorEstimado = item.ValorEstimado,
                ObservacoesItem = item.ObservacoesItem,
                DataProcessoIniciado = item.DataProcessoIniciado,
                DataProcessoConcluido = item.DataProcessoConcluido,
                StatusItem = item.StatusItem,
                StatusItemDescricao = ObterDescricaoStatusItem(item.StatusItem),
                Ativo = item.Ativo
            };
        }

        /// <summary>
        /// Converter ProtocoloDescarteItem para ProtocoloDescarteItemVM
        /// </summary>
        private async Task<ProtocoloDescarteItemVM> ConverterItemParaVM(ProtocoloDescarteItem item)
        {
            var evidencias = await _evidenciaRepository
                .Buscar(e => e.Equipamento == item.Equipamento && e.Ativo)
                .ToListAsync();

            return new ProtocoloDescarteItemVM
            {
                Id = item.Id,
                ProtocoloId = item.ProtocoloId,
                Equipamento = item.Equipamento,
                EquipamentoNavigation = ConverterEquipamentoParaVM(item.Equipamento),
                ProcessoSanitizacao = item.ProcessoSanitizacao,
                ProcessoDescaracterizacao = item.ProcessoDescaracterizacao,
                ProcessoPerfuracaoDisco = item.ProcessoPerfuracaoDisco,
                EvidenciasObrigatorias = item.EvidenciasObrigatorias,
                EvidenciasExecutadas = item.EvidenciasExecutadas,
                QuantidadeEvidencias = evidencias.Count,
                Evidencias = evidencias.Select(ConverterEvidenciaParaVM).ToList(),
                ValorEstimado = item.ValorEstimado,
                ObservacoesItem = item.ObservacoesItem,
                DataProcessoIniciado = item.DataProcessoIniciado,
                DataProcessoConcluido = item.DataProcessoConcluido,
                StatusItem = item.StatusItem,
                StatusItemDescricao = ObterDescricaoStatusItem(item.StatusItem),
                Ativo = item.Ativo,
                ProntoParaConclusao = item.ProcessoSanitizacao && item.ProcessoDescaracterizacao && 
                                     item.ProcessoPerfuracaoDisco && item.EvidenciasExecutadas
            };
        }

        /// <summary>
        /// Converter Equipamento para EquipamentoVM
        /// </summary>
        private SingleOne.Models.Equipamentovm ConverterEquipamentoParaVM(int equipamentoId)
        {
            var equipamento = _equipamentoRepository
                .Buscar(e => e.Id == equipamentoId)
                .Include(e => e.FabricanteNavigation)
                .Include(e => e.ModeloNavigation)
                .Include(e => e.EquipamentostatusNavigation)
                .FirstOrDefault();

            if (equipamento == null)
                return null;

            return new SingleOne.Models.Equipamentovm
            {
                Id = equipamento.Id,
                Fabricante = equipamento.FabricanteNavigation?.Descricao,
                Modelo = equipamento.ModeloNavigation?.Descricao,
                Numeroserie = equipamento.Numeroserie,
                Patrimonio = equipamento.Patrimonio,
                Equipamentostatus = equipamento.EquipamentostatusNavigation?.Descricao
            };
        }

        /// <summary>
        /// Converter DescarteEvidencia para DescarteEvidenciaVM
        /// </summary>
        private DescarteEvidenciaVM ConverterEvidenciaParaVM(DescarteEvidencia evidencia)
        {
            return new DescarteEvidenciaVM
            {
                Id = evidencia.Id,
                Equipamento = evidencia.Equipamento,
                ProtocoloId = evidencia.ProtocoloId,
                Descricao = evidencia.Descricao,
                TipoProcesso = evidencia.Tipoprocesso,
                NomeArquivo = evidencia.Nomearquivo,
                CaminhoArquivo = evidencia.Caminhoarquivo,
                TipoArquivo = evidencia.Tipoarquivo,
                TamanhoArquivo = evidencia.Tamanhoarquivo,
                UsuarioUpload = evidencia.Usuarioupload,
                DataUpload = evidencia.Dataupload,
                Ativo = evidencia.Ativo
            };
        }

        /// <summary>
        /// Obter descrição do tipo de descarte
        /// </summary>
        private string ObterDescricaoTipoDescarte(string tipo)
        {
            return tipo switch
            {
                "DOACAO" => "Doação",
                "VENDA" => "Venda",
                "DEVOLUCAO" => "Devolução",
                "LOGISTICA_REVERSA" => "Logística Reversa",
                "DESCARTE_FINAL" => "Descarte Geral (destruição)",
                _ => tipo
            };
        }

        /// <summary>
        /// Obter descrição do status do protocolo
        /// </summary>
        private string ObterDescricaoStatus(string status)
        {
            return status switch
            {
                "EM_ANDAMENTO" => "Em Andamento",
                "CONCLUIDO" => "Concluído",
                "CANCELADO" => "Cancelado",
                _ => status
            };
        }

        /// <summary>
        /// Obter descrição do status do item
        /// </summary>
        private string ObterDescricaoStatusItem(string status)
        {
            return status switch
            {
                "PENDENTE" => "Pendente",
                "EM_PROCESSO" => "Em Processo",
                "CONCLUIDO" => "Concluído",
                _ => status
            };
        }

        /// <summary>
        /// Gerar documento PDF de descarte usando template
        /// </summary>
        public async Task<byte[]> GerarDocumentoDescarte(int protocoloId, int usuarioId)
        {
            var protocolo = await _protocoloRepository
                .Buscar(p => p.Id == protocoloId)
                .Include(p => p.ClienteNavigation)
                .Include(p => p.ResponsavelNavigation)
                .FirstOrDefaultAsync();

            if (protocolo == null)
                throw new Exception("Protocolo não encontrado");

            if (protocolo.Status != "CONCLUIDO")
                throw new Exception("Só é possível gerar documento de protocolos concluídos");

            // Buscar template ID 5
            var template = _templateRepository.ObterPorId(5);
            
            // Buscar itens do protocolo com equipamentos
            var itens = await _itemRepository
                .Buscar(i => i.ProtocoloId == protocoloId && i.Ativo)
                .Include(i => i.EquipamentoNavigation)
                    .ThenInclude(e => e.FabricanteNavigation)
                .Include(i => i.EquipamentoNavigation)
                    .ThenInclude(e => e.ModeloNavigation)
                .Include(i => i.EquipamentoNavigation)
                    .ThenInclude(e => e.TipoequipamentoNavigation)
                .Include(i => i.EquipamentoNavigation)
                    .ThenInclude(e => e.TipoaquisicaoNavigation)
                .ToListAsync();

            // ✅ CORREÇÃO: Verificar se template contém as variáveis do MTR (padrão @)
            string html;
            if (template == null || 
                !template.Conteudo.Contains("@listaEquipamentos") ||
                !template.Conteudo.Contains("@mtrNumero"))
            {
                Console.WriteLine("⚠️ Template ID 5 não encontrado ou sem variáveis do MTR. Usando template padrão atualizado.");
                html = ObterTemplatePadrao();
            }
            else
            {
                Console.WriteLine("✅ Usando template ID 5 do banco de dados.");
                html = template.Conteudo;
            }
            
            Console.WriteLine($"📄 Template original tem {html.Length} caracteres");
            Console.WriteLine($"🔍 Template contém {{{{NUMERO_PROTOCOLO}}}}: {html.Contains("{{NUMERO_PROTOCOLO}}")}");
            Console.WriteLine($"📋 Protocolo: {protocolo.Protocolo}");
            Console.WriteLine($"🏢 Cliente: {protocolo.ClienteNavigation?.Razaosocial}");
            Console.WriteLine($"📦 Quantidade de itens: {itens.Count}");

            // Substituir variáveis do protocolo - Padrão @ (igual aos templates de colaboradores)
            html = html.Replace("@numeroProtocolo", protocolo.Protocolo);
            html = html.Replace("@cliente", protocolo.ClienteNavigation?.Razaosocial ?? "N/A");
            html = html.Replace("@tipoDescarte", ObterDescricaoTipoDescarte(protocolo.TipoDescarte));
            html = html.Replace("@motivoDescarte", protocolo.MotivoDescarte ?? "Não informado");
            html = html.Replace("@destinoFinal", protocolo.DestinoFinal ?? "Não informado");
            html = html.Replace("@empresaDestino", protocolo.EmpresaDestinoFinal ?? "Não informado");
            html = html.Replace("@cnpjDestino", protocolo.CnpjDestinoFinal ?? "Não informado");
            html = html.Replace("@certificado", protocolo.CertificadoDescarte ?? "Não informado");
            html = html.Replace("@responsavel", protocolo.ResponsavelNavigation?.Nome ?? "");
            html = html.Replace("@dataCriacao", protocolo.DataCriacao.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
            html = html.Replace("@dataConclusao", protocolo.DataConclusao?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) ?? "");
            html = html.Replace("@quantidadeEquipamentos", itens.Count.ToString());
            html = html.Replace("@observacoes", protocolo.Observacoes ?? "Nenhuma observação");
            
            // ✅ Variáveis do MTR (Manifesto de Transporte de Resíduos)
            html = html.Replace("@mtrObrigatorio", protocolo.MtrObrigatorio ? "Sim" : "Não");
            html = html.Replace("@mtrNumero", protocolo.MtrNumero ?? "Não informado");
            html = html.Replace("@mtrEmitidoPor", ObterDescricaoMtrEmitidoPor(protocolo.MtrEmitidoPor));
            html = html.Replace("@mtrDataEmissao", protocolo.MtrDataEmissao?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "Não informado");
            html = html.Replace("@mtrValidade", protocolo.MtrValidade?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "Não informado");
            html = html.Replace("@mtrEmpresaTransportadora", protocolo.MtrEmpresaTransportadora ?? "Não informado");
            html = html.Replace("@mtrCnpjTransportadora", protocolo.MtrCnpjTransportadora ?? "Não informado");
            html = html.Replace("@mtrPlacaVeiculo", protocolo.MtrPlacaVeiculo ?? "Não informado");
            html = html.Replace("@mtrMotorista", protocolo.MtrMotorista ?? "Não informado");
            html = html.Replace("@mtrCpfMotorista", protocolo.MtrCpfMotorista ?? "Não informado");
            
            // ✅ NOVO: Dados da transportadora aparecem apenas quando MTR foi emitido pelo transportador
            string dadosTransportadora = "";
            if (protocolo.MtrEmitidoPor == "TRANSPORTADOR" && 
                (!string.IsNullOrEmpty(protocolo.MtrEmpresaTransportadora) ||
                 !string.IsNullOrEmpty(protocolo.MtrPlacaVeiculo) ||
                 !string.IsNullOrEmpty(protocolo.MtrMotorista)))
            {
                dadosTransportadora = $@"
                <table class='info-table' style='margin-top: 15px; padding-top: 15px; border-top: 1px solid #f0ad4e;'>
                    <tr>
                        <td width='50%'><div class='info-item'><span class='label'>Empresa Transportadora:</span> <span class='value'>{protocolo.MtrEmpresaTransportadora ?? "Não informado"}</span></div></td>
                        <td width='50%'><div class='info-item'><span class='label'>CNPJ Transportadora:</span> <span class='value'>{protocolo.MtrCnpjTransportadora ?? "Não informado"}</span></div></td>
                    </tr>
                    <tr>
                        <td><div class='info-item'><span class='label'>Placa do Veículo:</span> <span class='value'>{protocolo.MtrPlacaVeiculo ?? "Não informado"}</span></div></td>
                        <td><div class='info-item'><span class='label'>Nome do Motorista:</span> <span class='value'>{protocolo.MtrMotorista ?? "Não informado"}</span></div></td>
                    </tr>
                    <tr>
                        <td colspan='2'><div class='info-item'><span class='label'>CPF do Motorista:</span> <span class='value'>{protocolo.MtrCpfMotorista ?? "Não informado"}</span></div></td>
                    </tr>
                </table>";
            }
            html = html.Replace("@mtrDadosTransportadora", dadosTransportadora);

            // Gerar lista detalhada de equipamentos
            var listaEquipamentos = new StringBuilder();
            int contador = 1;

            foreach (var item in itens)
            {
                listaEquipamentos.Append($@"
                <div style='border: 2px solid #e9ecef; padding: 20px; margin-bottom: 20px; page-break-inside: avoid;'>
                    <h3 style='color: #FF3A0F; margin-top: 0; border-bottom: 2px solid #FF3A0F; padding-bottom: 10px;'>
                        Equipamento #{contador} - {item.EquipamentoNavigation?.Numeroserie ?? ""}
                    </h3>
                    
                    <table style='width: 100%; margin-bottom: 15px; border-collapse: collapse;'>
                        <tr>
                            <td style='width: 50%; padding: 5px; vertical-align: top;'>
                                <p style='margin: 5px 0;'><strong>Número de Série:</strong> {item.EquipamentoNavigation?.Numeroserie ?? "N/A"}</p>
                                <p style='margin: 5px 0;'><strong>Patrimônio:</strong> {item.EquipamentoNavigation?.Patrimonio ?? "N/A"}</p>
                                <p style='margin: 5px 0;'><strong>Fabricante:</strong> {item.EquipamentoNavigation?.FabricanteNavigation?.Descricao ?? "N/A"}</p>
                                <p style='margin: 5px 0;'><strong>Tipo de Recurso:</strong> {item.EquipamentoNavigation?.TipoequipamentoNavigation?.Descricao ?? "N/A"}</p>
                            </td>
                            <td style='width: 50%; padding: 5px; vertical-align: top;'>
                                <p style='margin: 5px 0;'><strong>Modelo:</strong> {item.EquipamentoNavigation?.ModeloNavigation?.Descricao ?? "N/A"}</p>
                                <p style='margin: 5px 0;'><strong>Tipo de Aquisição:</strong> {item.EquipamentoNavigation?.TipoaquisicaoNavigation?.Nome ?? "N/A"}</p>
                                <p style='margin: 5px 0;'><strong>Valor Estimado:</strong> {(item.ValorEstimado.HasValue ? item.ValorEstimado.Value.ToString("C2", CultureInfo.GetCultureInfo("pt-BR")) : "N/A")}</p>
                                <p style='margin: 5px 0;'><strong>Status:</strong> <span style='color: #28a745; font-weight: bold;'>{ObterDescricaoStatusItem(item.StatusItem)}</span></p>
                            </td>
                        </tr>
                    </table>");

                // Processos Executados (se houver)
                if (item.ProcessosObrigatorios)
                {
                    listaEquipamentos.Append(@"
                    <div style='background: #f8f9fa; padding: 15px; margin-top: 15px; border: 1px solid #dee2e6;'>
                        <h4 style='margin-top: 0; color: #2c3e50; font-size: 14px;'>
                            <span style='background: #FF3A0F; color: white; padding: 4px 10px;'>⚠️ CARGO DE CONFIANÇA</span>
                            Processos Obrigatórios Executados
                        </h4>
                        <ul style='margin: 10px 0; padding-left: 20px;'>");

                    if (item.ObrigarSanitizacao)
                    {
                        var statusSanitizacao = item.ProcessoSanitizacao ? "✅" : "❌";
                        listaEquipamentos.Append($@"
                            <li style='margin: 8px 0;'>
                                <strong>{statusSanitizacao} Sanitização de Dados:</strong> {(item.ProcessoSanitizacao ? "Executado" : "Não executado")}
                                {(item.ProcessoSanitizacao && !string.IsNullOrEmpty(item.MetodoSanitizacao) ? $"<br/>&nbsp;&nbsp;&nbsp;&nbsp;• Método: {ObterDescricaoMetodoSanitizacao(item.MetodoSanitizacao)}" : "")}
                                {(item.ProcessoSanitizacao && !string.IsNullOrEmpty(item.FerramentaUtilizada) ? $"<br/>&nbsp;&nbsp;&nbsp;&nbsp;• Ferramenta: {item.FerramentaUtilizada}" : "")}
                                {(item.ProcessoSanitizacao && !string.IsNullOrEmpty(item.ObservacoesSanitizacao) ? $"<br/>&nbsp;&nbsp;&nbsp;&nbsp;• Observações: {item.ObservacoesSanitizacao}" : "")}
                            </li>");
                    }

                    if (item.ObrigarDescaracterizacao)
                    {
                        var statusDescarac = item.ProcessoDescaracterizacao ? "✅" : "❌";
                        listaEquipamentos.Append($@"
                            <li style='margin: 8px 0;'>
                                <strong>{statusDescarac} Descaracterização Física:</strong> {(item.ProcessoDescaracterizacao ? "Executado" : "Não executado")}
                            </li>");
                    }

                    if (item.ObrigarPerfuracaoDisco)
                    {
                        var statusPerfuracao = item.ProcessoPerfuracaoDisco ? "✅" : "❌";
                        listaEquipamentos.Append($@"
                            <li style='margin: 8px 0;'>
                                <strong>{statusPerfuracao} Perfuração de Disco:</strong> {(item.ProcessoPerfuracaoDisco ? "Executado" : "Não executado")}
                            </li>");
                    }

                    if (item.EvidenciasObrigatorias)
                    {
                        var quantidadeEvidencias = _evidenciaRepository
                            .Buscar(e => e.Equipamento == item.Equipamento && 
                                   (e.ProtocoloId == protocoloId || e.ProtocoloId == null || e.ProtocoloId == 0) && 
                                   e.Ativo)
                            .Count();
                        var statusEvidencias = item.EvidenciasExecutadas ? "✅" : "❌";
                        listaEquipamentos.Append($@"
                            <li style='margin: 8px 0;'>
                                <strong>{statusEvidencias} Evidências Fotográficas:</strong> {(item.EvidenciasExecutadas ? $"{quantidadeEvidencias} evidência(s) anexada(s)" : "Não executado")}
                            </li>");
                    }

                    listaEquipamentos.Append("</ul>");

                    // Datas do processo
                    listaEquipamentos.Append("<div style='margin-top: 10px; font-size: 12px; color: #6c757d;'>");
                    if (item.DataProcessoIniciado.HasValue)
                    {
                        listaEquipamentos.Append($"<p style='margin: 3px 0;'><strong>Processo Iniciado:</strong> {item.DataProcessoIniciado.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}</p>");
                    }
                    if (item.DataProcessoConcluido.HasValue)
                    {
                        listaEquipamentos.Append($"<p style='margin: 3px 0;'><strong>Processo Concluído:</strong> {item.DataProcessoConcluido.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}</p>");
                    }
                    listaEquipamentos.Append("</div>");

                    listaEquipamentos.Append("</div>");

                    // 📸 SEÇÃO DE EVIDÊNCIAS (se houver)
                    var evidencias = _evidenciaRepository
                        .Buscar(e => e.Equipamento == item.Equipamento && 
                               (e.ProtocoloId == protocoloId || e.ProtocoloId == null || e.ProtocoloId == 0) && 
                               e.Ativo)
                        .OrderBy(e => e.Tipoprocesso)
                        .ThenBy(e => e.Dataupload)
                        .ToList();

                    if (evidencias.Any())
                    {
                        listaEquipamentos.Append(@"
                        <div style='background: #fff8e1; padding: 15px; margin-top: 15px; border: 1px solid #ffc107;'>
                            <h4 style='margin-top: 0; margin-bottom: 10px; color: #f57c00; font-size: 14px;'>
                                📸 Evidências Anexadas (" + evidencias.Count + @" arquivo(s))
                            </h4>
                            <table style='width: 100%; border-collapse: collapse; font-size: 12px;'>
                                <thead>
                                    <tr style='background: #ffe0b2;'>
                                        <th style='padding: 8px; border: 1px solid #ffb74d; text-align: left;'>Tipo de Processo</th>
                                        <th style='padding: 8px; border: 1px solid #ffb74d; text-align: left;'>Descrição</th>
                                        <th style='padding: 8px; border: 1px solid #ffb74d; text-align: left;'>Arquivo</th>
                                        <th style='padding: 8px; border: 1px solid #ffb74d; text-align: left;'>Data</th>
                                    </tr>
                                </thead>
                                <tbody>");

                        foreach (var evidencia in evidencias)
                        {
                            var tipoProcessoLabel = evidencia.Tipoprocesso switch
                            {
                                "SANITIZACAO" => "Sanitização",
                                "DESCARACTERIZACAO" => "Descaracterização",
                                "PERFURACAO_DISCO" => "Perfuração de Disco",
                                "EVIDENCIAS_GERAIS" => "Evidências Gerais",
                                _ => evidencia.Tipoprocesso
                            };

                            // 🖼️ Gerar imagem base64 se for arquivo de imagem
                            var imagemBase64 = "";
                            var extensaoArquivo = Path.GetExtension(evidencia.Nomearquivo)?.ToLower();
                            var ehImagem = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }.Contains(extensaoArquivo);

                            if (ehImagem && !string.IsNullOrEmpty(evidencia.Caminhoarquivo))
                            {
                                try
                                {
                                    // 🔧 CORREÇÃO: As evidências estão em /evidencias, não em /wwwroot/evidencias
                                    var caminhoCompleto = Path.Combine(Directory.GetCurrentDirectory(), evidencia.Caminhoarquivo.TrimStart('/'));
                                    if (File.Exists(caminhoCompleto))
                                    {
                                        var bytesImagem = File.ReadAllBytes(caminhoCompleto);
                                        var mimeType = extensaoArquivo switch
                                        {
                                            ".jpg" or ".jpeg" => "image/jpeg",
                                            ".png" => "image/png",
                                            ".gif" => "image/gif",
                                            ".bmp" => "image/bmp",
                                            _ => "image/jpeg"
                                        };
                                        imagemBase64 = $"data:{mimeType};base64,{Convert.ToBase64String(bytesImagem)}";
                                        Console.WriteLine($"🖼️ Imagem carregada: {evidencia.Nomearquivo} ({bytesImagem.Length} bytes)");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"⚠️ Arquivo não encontrado: {caminhoCompleto}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"❌ Erro ao carregar imagem {evidencia.Nomearquivo}: {ex.Message}");
                                }
                            }

                            // 📋 Linha da tabela com ou sem imagem
                            if (!string.IsNullOrEmpty(imagemBase64))
                            {
                                listaEquipamentos.Append($@"
                                    <tr>
                                        <td style='padding: 6px; border: 1px solid #ffb74d; vertical-align: top;'>{tipoProcessoLabel}</td>
                                        <td style='padding: 6px; border: 1px solid #ffb74d; vertical-align: top;'>{evidencia.Descricao ?? "N/A"}</td>
                                        <td style='padding: 6px; border: 1px solid #ffb74d; vertical-align: top;'>
                                            <div style='font-size: 11px; margin-bottom: 5px;'>{evidencia.Nomearquivo}</div>
                                            <img src='{imagemBase64}' style='max-width: 200px; max-height: 150px; border: 1px solid #ddd;' />
                                        </td>
                                        <td style='padding: 6px; border: 1px solid #ffb74d; vertical-align: top;'>{evidencia.Dataupload.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}</td>
                                    </tr>");
                            }
                            else
                            {
                                listaEquipamentos.Append($@"
                                    <tr>
                                        <td style='padding: 6px; border: 1px solid #ffb74d;'>{tipoProcessoLabel}</td>
                                        <td style='padding: 6px; border: 1px solid #ffb74d;'>{evidencia.Descricao ?? "N/A"}</td>
                                        <td style='padding: 6px; border: 1px solid #ffb74d;'>{evidencia.Nomearquivo}</td>
                                        <td style='padding: 6px; border: 1px solid #ffb74d;'>{evidencia.Dataupload.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}</td>
                                    </tr>");
                            }
                        }

                        listaEquipamentos.Append(@"
                                </tbody>
                            </table>
                        </div>");
                    }
                }
                else
                {
                    listaEquipamentos.Append(@"
                    <div style='background: #e7f3ff; padding: 10px; margin-top: 10px; font-size: 13px; border: 1px solid #bee5eb;'>
                        <p style='margin: 0; color: #0c5460;'>
                            <strong>ℹ️ Informação:</strong> Este equipamento não requer processos especiais de sanitização.
                        </p>
                    </div>");
                }

                // Observações do item (se houver)
                if (!string.IsNullOrEmpty(item.ObservacoesItem))
                {
                    listaEquipamentos.Append($@"
                    <div style='margin-top: 10px; padding: 10px; background: #fff3cd; border-left: 4px solid #ffc107;'>
                        <p style='margin: 0; font-size: 13px;'><strong>📝 Observações:</strong> {item.ObservacoesItem}</p>
                    </div>");
                }

                listaEquipamentos.Append("</div>");
                contador++;
            }

            var listaEquipamentosHtml = listaEquipamentos.ToString();
            Console.WriteLine($"📦 Lista de equipamentos gerada: {listaEquipamentosHtml.Length} caracteres");
            Console.WriteLine($"🔍 Primeiros 200 caracteres da lista: {listaEquipamentosHtml.Substring(0, Math.Min(200, listaEquipamentosHtml.Length))}");
            
            html = html.Replace("@listaEquipamentos", listaEquipamentosHtml);
            html = html.Replace("@tabelaEquipamentos", listaEquipamentosHtml); // Manter compatibilidade

            Console.WriteLine($"✅ HTML final tem {html.Length} caracteres");
            Console.WriteLine($"🔍 HTML final contém LISTA_EQUIPAMENTOS: {html.Contains("{{LISTA_EQUIPAMENTOS}}")}");
            Console.WriteLine($"🔍 HTML final contém dados do equipamento: {html.Contains("Equipamento #1")}");

            // 🐛 DEBUG: Salvar HTML em arquivo temporário para análise
            try
            {
                var tempHtmlPath = Path.Combine(Path.GetTempPath(), $"DEBUG_DESCARTE_{protocoloId}_{DateTime.Now:yyyyMMddHHmmss}.html");
                File.WriteAllText(tempHtmlPath, html);
                Console.WriteLine($"🐛 DEBUG: HTML salvo em: {tempHtmlPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro ao salvar HTML debug: {ex.Message}");
            }

            // ✅ Incluir CSS do CKEditor (mesmo comportamento dos templates de colaboradores)
            try
            {
                var cssPath = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "ckeditor.css");
                if (File.Exists(cssPath))
                {
                    var css = File.ReadAllText(cssPath);
                    // Regras adicionais para garantir largura e layout das tabelas geradas pelo CKEditor
                    var extras = "table{width:100%;border-collapse:collapse} figure.table>table{width:100%} figure.table{width:100%} td,th{vertical-align:top;padding:6px;border:1px solid #ddd}";
                    html += $"<style>{css}{extras}</style>";
                    Console.WriteLine($"🎨 CSS do CKEditor aplicado ({css.Length} chars)");
                }
                else
                {
                    // Fallback mínimo para tabelas do cabeçalho do CKEditor
                    html += "<style>table{width:100%;border-collapse:collapse} td,th{vertical-align:top;padding:6px;border:1px solid #ddd} figure.table>table{width:100%}</style>";
                    Console.WriteLine("🎨 CSS do CKEditor não encontrado. Aplicado fallback mínimo para tabelas.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro ao aplicar CSS do CKEditor: {ex.Message}");
            }

            // Gerar PDF
            Console.WriteLine($"📄 Iniciando conversão HTML -> PDF...");
            var pdfBytes = HtmlToPdfConverter.ConvertHtmlToPdf(html);
            Console.WriteLine($"✅ PDF gerado com sucesso: {pdfBytes.Length} bytes");

            // Salvar caminho do documento no protocolo
            var nomeArquivo = $"DESCARTE_{protocolo.Protocolo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var caminhoRelativo = $"/documentos/descartes/{nomeArquivo}";
            
            protocolo.DocumentoGerado = true;
            protocolo.CaminhoDocumento = caminhoRelativo;
            _protocoloRepository.Atualizar(protocolo);
            _protocoloRepository.SalvarAlteracoes();

            return pdfBytes;
        }

        /// <summary>
        /// Obter descrição do método de sanitização
        /// </summary>
        private string ObterDescricaoMetodoSanitizacao(string metodo)
        {
            return metodo switch
            {
                "FORMATACAO_SIMPLES" => "Formatação Simples",
                "SOBREGRAVAR_MIDIA" => "Sobregravar Mídia (DoD 5220.22-M)",
                "DESTRUICAO_FISICA" => "Destruição Física",
                "DESMAGNETIZACAO" => "Desmagnetização",
                "RESTAURACAO_FABRICA" => "Restauração de Fábrica",
                _ => metodo ?? "Não informado"
            };
        }

        /// <summary>
        /// Obter descrição de quem emitiu o MTR
        /// </summary>
        private string ObterDescricaoMtrEmitidoPor(string emitidoPor)
        {
            return emitidoPor switch
            {
                "GERADOR" => "Gerador dos Resíduos",
                "TRANSPORTADOR" => "Transportador",
                "DESTINADOR" => "Destinador Final",
                _ => emitidoPor ?? "Não informado"
            };
        }

        /// <summary>
        /// Obter template padrão embutido no código (fallback se template do BD não estiver configurado)
        /// </summary>
        private string ObterTemplatePadrao()
        {
            return @"<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <title>Termo de Descarte de Equipamentos</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 40px; color: #2c3e50; line-height: 1.6; }
        .header { text-align: center; border-bottom: 4px solid #FF3A0F; padding-bottom: 20px; margin-bottom: 30px; }
        .header h1 { color: #FF3A0F; font-size: 28px; margin: 0 0 10px 0; text-transform: uppercase; }
        .header .protocolo { font-size: 18px; font-weight: bold; color: #34495e; }
        .section { margin: 30px 0; padding: 20px; background: #f8f9fa; border-left: 4px solid #FF3A0F; }
        .section h2 { color: #FF3A0F; font-size: 18px; margin: 0 0 15px 0; border-bottom: 2px solid #e9ecef; padding-bottom: 10px; }
        .info-table { width: 100%; margin: 15px 0; border-collapse: collapse; }
        .info-table td { padding: 8px 10px; vertical-align: top; }
        .info-item { margin: 8px 0; }
        .label { font-weight: bold; color: #495057; }
        .value { color: #2c3e50; }
        .logistica-reversa { background: #d4edda; border-left-color: #28a745; }
        .mtr-section { background: #fff3cd; border-left-color: #ffc107; }
        .mtr-section h2 { color: #856404; }
        .footer { margin-top: 60px; text-align: center; padding-top: 20px; border-top: 2px solid #e9ecef; }
        .assinatura { margin-top: 50px; text-align: center; }
        .assinatura-linha { border-top: 2px solid #2c3e50; width: 300px; margin: 0 auto 10px auto; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>TERMO DE DESCARTE DE EQUIPAMENTOS</h1>
        <div class='protocolo'>Protocolo: @numeroProtocolo</div>
    </div>

    <div class='section'>
        <h2>Informações do Protocolo</h2>
        <table class='info-table'>
            <tr>
                <td width='50%'><div class='info-item'><span class='label'>Cliente:</span> <span class='value'>@cliente</span></div></td>
                <td width='50%'><div class='info-item'><span class='label'>Tipo de Descarte:</span> <span class='value'>@tipoDescarte</span></div></td>
            </tr>
            <tr>
                <td><div class='info-item'><span class='label'>Responsável:</span> <span class='value'>@responsavel</span></div></td>
                <td><div class='info-item'><span class='label'>Data de Criação:</span> <span class='value'>@dataCriacao</span></div></td>
            </tr>
            <tr>
                <td><div class='info-item'><span class='label'>Data de Conclusão:</span> <span class='value'>@dataConclusao</span></div></td>
                <td><div class='info-item'><span class='label'>Total de Equipamentos:</span> <span class='value'>@quantidadeEquipamentos</span></div></td>
            </tr>
        </table>
        <div class='info-item'><span class='label'>Motivo:</span> <span class='value'>@motivoDescarte</span></div>
        <div class='info-item'><span class='label'>Destino:</span> <span class='value'>@destinoFinal</span></div>
    </div>

    <div class='section logistica-reversa'>
        <h2>Logística Reversa (Lei 12.305/2010)</h2>
        <div class='info-item'><span class='label'>Empresa Destino:</span> <span class='value'>@empresaDestino</span></div>
        <div class='info-item'><span class='label'>CNPJ:</span> <span class='value'>@cnpjDestino</span></div>
        <div class='info-item'><span class='label'>Certificado:</span> <span class='value'>@certificado</span></div>
        <p style='font-size: 12px; color: #155724; margin-top: 15px; font-style: italic;'>
            Em conformidade com a Política Nacional de Resíduos Sólidos
        </p>
    </div>

    <div class='section mtr-section'>
        <h2>📋 MTR - Manifesto de Transporte de Resíduos</h2>
        <table class='info-table'>
            <tr>
                <td width='50%'><div class='info-item'><span class='label'>MTR Obrigatório:</span> <span class='value'>@mtrObrigatorio</span></div></td>
                <td width='50%'><div class='info-item'><span class='label'>Número do MTR:</span> <span class='value'>@mtrNumero</span></div></td>
            </tr>
            <tr>
                <td><div class='info-item'><span class='label'>Emitido Por:</span> <span class='value'>@mtrEmitidoPor</span></div></td>
                <td><div class='info-item'><span class='label'>Data de Emissão:</span> <span class='value'>@mtrDataEmissao</span></div></td>
            </tr>
            <tr>
                <td colspan='2'><div class='info-item'><span class='label'>Validade do MTR:</span> <span class='value'>@mtrValidade</span></div></td>
            </tr>
        </table>
        @mtrDadosTransportadora
        <p style='font-size: 12px; color: #856404; margin-top: 15px; font-style: italic;'>
            O MTR (Manifesto de Transporte de Resíduos) é obrigatório conforme Resolução CONAMA nº 313/2002
        </p>
    </div>

    <div class='section'>
        <h2>Equipamentos Descartados</h2>
        @listaEquipamentos
    </div>

    <div class='section'>
        <h2>Observações Gerais</h2>
        <p>@observacoes</p>
    </div>

    <div class='assinatura'>
        <div class='assinatura-linha'></div>
        <p style='margin: 5px 0; font-weight: bold;'>@responsavel</p>
        <p style='margin: 5px 0; font-size: 14px; color: #6c757d;'>Responsável pelo Processo de Descarte</p>
    </div>

    <div class='footer'>
        <p style='font-size: 11px; color: #6c757d;'>Documento gerado pelo Sistema SingleOne</p>
    </div>
</body>
</html>";
        }

        #endregion
    }
}
