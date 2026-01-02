using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Infra.Repositorio.Views;
using SingleOneAPI.Models;
using SingleOneAPI.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleOneAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermosPublicosController : ControllerBase
    {
        private readonly IRepository<Requisico> _requisicaoRepository;
        private readonly IRepository<Requisicoesiten> _requisicaoItensRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Tipoequipamento> _tipoEquipamentoRepository;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IReadOnlyRepository<SingleOne.Models.Requisicoesvm> _requisicoesvmRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<GeolocalizacaoAssinatura> _geolocalizacaoRepository;

        public TermosPublicosController(
            IRepository<Requisico> requisicaoRepository,
            IRepository<Requisicoesiten> requisicaoItensRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Usuario> usuarioRepository,
            IReadOnlyRepository<SingleOne.Models.Requisicoesvm> requisicoesvmRepository,
            IRepository<Tipoequipamento> tipoEquipamentoRepository,
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<GeolocalizacaoAssinatura> geolocalizacaoRepository)
        {
            _requisicaoRepository = requisicaoRepository;
            _requisicaoItensRepository = requisicaoItensRepository;
            _equipamentoRepository = equipamentoRepository;
            _usuarioRepository = usuarioRepository;
            _requisicoesvmRepository = requisicoesvmRepository;
            _tipoEquipamentoRepository = tipoEquipamentoRepository;
            _colaboradorRepository = colaboradorRepository;
            _geolocalizacaoRepository = geolocalizacaoRepository;
        }

        /// <summary>
        /// Valida publicamente um termo pelo hash (sem dados sensíveis)
        /// </summary>
        [HttpGet("validacao/{hash}")]
        [AllowAnonymous]
        public IActionResult Validar(string hash)
        {
            Console.WriteLine($"[TERMO] ========== INÍCIO VALIDAÇÃO TERMO ==========");
            Console.WriteLine($"[TERMO] Hash recebido: {hash}");
            
            try
            {
                if (string.IsNullOrWhiteSpace(hash))
                {
                    Console.WriteLine($"[TERMO] ❌ Hash inválido (vazio ou nulo)");
                    return BadRequest(new { sucesso = false, mensagem = "Hash inválido" });
                }

                // Saneamento: remover dois-pontos iniciais e espaços, e normalizar case
                try
                {
                    hash = Uri.UnescapeDataString(hash);
                }
                catch { }
                if (hash.StartsWith(":")) hash = hash.Substring(1);
                hash = (hash ?? string.Empty).Trim();
                
                Console.WriteLine($"[TERMO] Hash após saneamento: {hash}");

                // Buscar TODAS as requisições que compartilham o mesmo hash
                var hashLower = hash.ToLower();
                Console.WriteLine($"[TERMO] Buscando requisições com hash: {hashLower}");
                var requisicoesHash = _requisicaoRepository
                    .Buscar(x => x.Hashrequisicao != null && x.Hashrequisicao.ToLower() == hashLower)
                    .ToList();
                Console.WriteLine($"[TERMO] Requisições encontradas diretamente: {requisicoesHash?.Count ?? 0}");
                
                if (requisicoesHash == null || requisicoesHash.Count == 0)
                {
                    Console.WriteLine($"[TERMO] Nenhuma requisição encontrada diretamente, tentando via view...");
                    // Fallback: consultar a view consolidada
                    var rvm = _requisicoesvmRepository
                        .Buscar(x => x.Hashrequisicao != null && x.Hashrequisicao.ToLower() == hashLower)
                        .ToList();
                    
                    Console.WriteLine($"[TERMO] Requisições encontradas via view: {rvm?.Count ?? 0}");
                    
                    if (rvm == null || rvm.Count == 0)
                    {
                        Console.WriteLine($"[TERMO] ❌ Termo não encontrado para o hash informado");
                        return Ok(new { sucesso = true, assinado = false, mensagem = "Termo não encontrado para o hash informado." });
                    }

                    bool assinadoVm = rvm.Any(r => r.Assinaturaeletronica == true);
                    DateTime? dtAssinaturaVm = rvm
                        .Where(r => r.Dtassinaturaeletronica.HasValue)
                        .OrderByDescending(r => r.Dtassinaturaeletronica)
                        .Select(r => r.Dtassinaturaeletronica)
                        .FirstOrDefault();

                    // Tentar listar itens a partir dos IDs da view
                    var reqIdsVm = rvm.Where(r => r.Id.HasValue).Select(r => r.Id.Value).Distinct().ToList();
                    Console.WriteLine($"[TERMO] Buscando itens via view para requisições: {string.Join(", ", reqIdsVm)}");
                    
                    var itensVm = _requisicaoItensRepository
                        .Buscar(x => reqIdsVm.Contains(x.Requisicao) && x.Dtdevolucao == null)
                        .ToList();
                    
                    Console.WriteLine($"[TERMO] Itens encontrados via view (sem devolução): {itensVm.Count}");
                    
                    // ✅ CORREÇÃO: Filtrar apenas itens que foram entregues
                    var itensEntreguesVm = itensVm.Where(x => x.Dtentrega.HasValue).ToList();
                    Console.WriteLine($"[TERMO] Itens entregues via view (Dtentrega não nulo): {itensEntreguesVm.Count}");
                    
                    // ✅ FALLBACK: Se não encontrar itens entregues, buscar todos os itens da requisição
                    if (itensEntreguesVm.Count == 0)
                    {
                        Console.WriteLine($"[TERMO] Nenhum item entregue encontrado via view, buscando todos os itens...");
                        itensEntreguesVm = itensVm;
                    }

                    var recursosVm = new List<object>();
                    foreach (var it in itensEntreguesVm)
                    {
                        string tipo = "Corporativo";
                        string patrimonio = string.Empty;
                        string numeroSerie = string.Empty;
                        string tipoEquipamento = string.Empty;

                        if (it.Equipamento > 0)
                        {
                            var eq = _equipamentoRepository.Buscar(e => e.Id == it.Equipamento).FirstOrDefault();
                            if (eq != null)
                            {
                                patrimonio = eq.Patrimonio;
                                numeroSerie = eq.Numeroserie;
                            tipoEquipamento = _tipoEquipamentoRepository
                                .Buscar(t => t.Id == eq.Tipoequipamento)
                                .Select(t => t.Descricao)
                                .FirstOrDefault() ?? "Equipamento";
                                tipo = eq.Tipoaquisicao == 2 ? "BYOD" : "Corporativo";
                            }
                        }
                        else if (it.Linhatelefonica.HasValue && it.Linhatelefonica.Value > 0)
                        {
                            tipoEquipamento = "Linha Telefônica";
                            var linha = _requisicaoItensRepository
                                .Buscar(x => x.Linhatelefonica == it.Linhatelefonica.Value)
                                .Select(x => x.LinhatelefonicaNavigation)
                                .FirstOrDefault();
                            patrimonio = (linha != null ? linha.Numero.ToString() : it.Linhatelefonica.Value.ToString());
                            numeroSerie = linha?.Iccid ?? string.Empty;
                        }

                        recursosVm.Add(new {
                            tipo,
                            patrimonio,
                            numeroSerie,
                            tipoEquipamento
                        });
                    }

                    // Técnico responsável pela view (pegar o mais recente possível via data)
                    int tecnicoVm = rvm
                        .Where(r => r.Tecnicoresponsavelid.HasValue)
                        .OrderByDescending(r => r.Dtassinaturaeletronica ?? r.Dtprocessamento ?? r.Dtsolicitacao)
                        .Select(r => r.Tecnicoresponsavelid.Value)
                        .FirstOrDefault();
                    string entreguePorVm = string.Empty;
                    if (tecnicoVm > 0)
                    {
                        var usu = _usuarioRepository.Buscar(u => u.Id == tecnicoVm).FirstOrDefault();
                        entreguePorVm = usu?.Nome ?? string.Empty;
                    }

                    // Buscar colaborador destinatário
                    int? colaboradorIdVm = rvm
                        .Where(r => r.Colaboradorfinalid.HasValue)
                        .Select(r => r.Colaboradorfinalid)
                        .FirstOrDefault();
                    string colaboradorNomeVm = string.Empty;
                    if (colaboradorIdVm.HasValue && colaboradorIdVm.Value > 0)
                    {
                        var colab = _colaboradorRepository.Buscar(c => c.Id == colaboradorIdVm.Value).FirstOrDefault();
                        colaboradorNomeVm = colab?.Nome ?? string.Empty;
                    }

                    // ✅ CORREÇÃO: Buscar dados de geolocalização da ASSINATURA (mesma lógica do PDF)
                    object dadosAuditoriaVm = null;
                    if (colaboradorIdVm.HasValue && colaboradorIdVm.Value > 0 && assinadoVm && dtAssinaturaVm.HasValue)
                    {
                        Console.WriteLine($"[VALIDACAO_TERMO] Buscando geolocalização para colaborador ID: {colaboradorIdVm.Value}");
                        
                        // Buscar todos os registros do dia da assinatura e escolher o mais próximo do horário
                        var geosDoDia = _geolocalizacaoRepository.Buscar(g =>
                            g.ColaboradorId == colaboradorIdVm.Value &&
                            (g.Acao == "ASSINATURA_TERMO_ELETRONICO" || g.Acao == "ASSINATURA_TERMO_ELETRONICO_BYOD") &&
                            g.TimestampCaptura.Date == dtAssinaturaVm.Value.Date)
                            .ToList();

                        var geolocalizacao = geosDoDia
                            .OrderBy(g => Math.Abs((g.TimestampCaptura - dtAssinaturaVm.Value).TotalSeconds))
                            .ThenByDescending(g => g.TimestampCaptura)
                            .FirstOrDefault();

                        Console.WriteLine($"[VALIDACAO_TERMO] Geolocalização encontrada: {(geolocalizacao != null ? "SIM" : "NÃO")}");
                        
                        if (geolocalizacao != null)
                        {
                            Console.WriteLine($"[VALIDACAO_TERMO] IP: {geolocalizacao.IpAddress}, Local: {geolocalizacao.City}, {geolocalizacao.Country}");
                            
                            dadosAuditoriaVm = new {
                                ip = geolocalizacao.IpAddress,
                                pais = geolocalizacao.Country,
                                cidade = geolocalizacao.City,
                                regiao = geolocalizacao.Region,
                                latitude = geolocalizacao.Latitude,
                                longitude = geolocalizacao.Longitude,
                                precisao = geolocalizacao.AccuracyMeters,
                                dataCaptura = geolocalizacao.TimestampCaptura
                            };
                        }
                    }

                    Console.WriteLine($"[TERMO] ✅ Retornando termo via view - Recursos: {recursosVm.Count}, Colaborador: {colaboradorNomeVm}");
                    Console.WriteLine($"[TERMO] ========== FIM VALIDAÇÃO TERMO (VIEW) ==========");
                    
                    return Ok(new {
                        sucesso = true,
                        assinado = assinadoVm,
                        dataAssinatura = dtAssinaturaVm,
                        entreguePor = entreguePorVm,
                        colaborador = colaboradorNomeVm,
                        hash,
                        recursos = recursosVm,
                        auditoria = dadosAuditoriaVm
                    });
                }

                // Consolidar status: considera assinado se qualquer requisição com o hash estiver marcada como assinada
                bool assinado = requisicoesHash.Any(r => r.Assinaturaeletronica == true);
                DateTime? dtAssinatura = requisicoesHash
                    .Where(r => r.Dtassinaturaeletronica.HasValue)
                    .OrderByDescending(r => r.Dtassinaturaeletronica)
                    .Select(r => r.Dtassinaturaeletronica)
                    .FirstOrDefault();

                // Compilar itens ativos (sem devolução) das requisições do hash
                var reqIds = requisicoesHash.Select(r => r.Id).ToList();
                Console.WriteLine($"[TERMO] Buscando itens para requisições: {string.Join(", ", reqIds)}");
                
                var itens = _requisicaoItensRepository
                    .Buscar(x => reqIds.Contains(x.Requisicao) && x.Dtdevolucao == null)
                    .ToList();
                
                Console.WriteLine($"[TERMO] Itens encontrados (sem devolução): {itens.Count}");
                
                // ✅ CORREÇÃO: Filtrar apenas itens que foram entregues (Dtentrega não nulo)
                // O termo deve mostrar apenas recursos que foram entregues mas ainda não foram assinados
                var itensEntregues = itens.Where(x => x.Dtentrega.HasValue).ToList();
                Console.WriteLine($"[TERMO] Itens entregues (Dtentrega não nulo): {itensEntregues.Count}");
                
                // ✅ FALLBACK: Se não encontrar itens entregues, buscar todos os itens da requisição
                // (pode ser que o termo seja gerado antes da entrega)
                if (itensEntregues.Count == 0)
                {
                    Console.WriteLine($"[TERMO] Nenhum item entregue encontrado, buscando todos os itens da requisição...");
                    itensEntregues = itens;
                }

                var recursos = new List<object>();
                foreach (var it in itensEntregues)
                {
                    string tipo = "Corporativo";
                    string patrimonio = string.Empty;
                    string numeroSerie = string.Empty;
                    string tipoEquipamento = string.Empty;

                    if (it.Equipamento > 0)
                    {
                        var eq = _equipamentoRepository
                            .Buscar(e => e.Id == it.Equipamento)
                            .FirstOrDefault();
                        if (eq != null)
                        {
                            patrimonio = eq.Patrimonio;
                            numeroSerie = eq.Numeroserie;
                            // Buscar descrição do tipo via repositório
                            tipoEquipamento = _tipoEquipamentoRepository
                                .Buscar(t => t.Id == eq.Tipoequipamento)
                                .Select(t => t.Descricao)
                                .FirstOrDefault() ?? "Equipamento";
                            // Tipo aquisição 2 = BYOD (conforme uso existente)
                            tipo = eq.Tipoaquisicao == 2 ? "BYOD" : "Corporativo";
                        }
                    }
                    else if (it.Linhatelefonica.HasValue && it.Linhatelefonica.Value > 0)
                    {
                        tipoEquipamento = "Linha Telefônica";
                        var linha = _requisicaoItensRepository
                            .Buscar(x => x.Linhatelefonica == it.Linhatelefonica.Value)
                            .Select(x => x.LinhatelefonicaNavigation)
                            .FirstOrDefault();
                        patrimonio = (linha != null ? linha.Numero.ToString() : it.Linhatelefonica.Value.ToString());
                        numeroSerie = linha?.Iccid ?? string.Empty;
                    }

                    recursos.Add(new {
                        tipo,
                        patrimonio,
                        numeroSerie,
                        tipoEquipamento
                    });
                }

                // Determinar "Entregue por" priorizando o usuário da requisição (quem efetua a entrega)
                var usuarioReqId = requisicoesHash
                    .Where(r => r.Usuariorequisicao > 0)
                    .OrderByDescending(r => r.Dtassinaturaeletronica ?? r.Dtprocessamento ?? r.Dtsolicitacao)
                    .Select(r => r.Usuariorequisicao)
                    .FirstOrDefault();

                string entreguePor = string.Empty;
                if (usuarioReqId > 0)
                {
                    var usu = _usuarioRepository.Buscar(u => u.Id == usuarioReqId).FirstOrDefault();
                    entreguePor = usu?.Nome ?? string.Empty;
                }

                // Buscar colaborador destinatário
                int? colaboradorId = requisicoesHash
                    .Where(r => r.Colaboradorfinal.HasValue)
                    .OrderByDescending(r => r.Dtassinaturaeletronica ?? r.Dtprocessamento ?? r.Dtsolicitacao)
                    .Select(r => r.Colaboradorfinal)
                    .FirstOrDefault();
                string colaboradorNome = string.Empty;
                if (colaboradorId.HasValue && colaboradorId.Value > 0)
                {
                    var colab = _colaboradorRepository.Buscar(c => c.Id == colaboradorId.Value).FirstOrDefault();
                    colaboradorNome = colab?.Nome ?? string.Empty;
                }

                // ✅ CORREÇÃO: Buscar dados de geolocalização da ASSINATURA (mesma lógica do PDF)
                object dadosAuditoria = null;
                if (colaboradorId.HasValue && colaboradorId.Value > 0 && assinado && dtAssinatura.HasValue)
                {
                    Console.WriteLine($"[VALIDACAO_TERMO] Buscando geolocalização para colaborador ID: {colaboradorId.Value}");
                    
                    // Buscar todos os registros do dia da assinatura e escolher o mais próximo do horário
                    var geosDoDia = _geolocalizacaoRepository.Buscar(g =>
                        g.ColaboradorId == colaboradorId.Value &&
                        (g.Acao == "ASSINATURA_TERMO_ELETRONICO" || g.Acao == "ASSINATURA_TERMO_ELETRONICO_BYOD") &&
                        g.TimestampCaptura.Date == dtAssinatura.Value.Date)
                        .ToList();

                    var geolocalizacao = geosDoDia
                        .OrderBy(g => Math.Abs((g.TimestampCaptura - dtAssinatura.Value).TotalSeconds))
                        .ThenByDescending(g => g.TimestampCaptura)
                        .FirstOrDefault();

                    Console.WriteLine($"[VALIDACAO_TERMO] Geolocalização encontrada: {(geolocalizacao != null ? "SIM" : "NÃO")}");
                    
                    if (geolocalizacao != null)
                    {
                        Console.WriteLine($"[VALIDACAO_TERMO] IP: {geolocalizacao.IpAddress}, Local: {geolocalizacao.City}, {geolocalizacao.Country}");
                        
                        dadosAuditoria = new {
                            ip = geolocalizacao.IpAddress,
                            pais = geolocalizacao.Country,
                            cidade = geolocalizacao.City,
                            regiao = geolocalizacao.Region,
                            latitude = geolocalizacao.Latitude,
                            longitude = geolocalizacao.Longitude,
                            precisao = geolocalizacao.AccuracyMeters,
                            dataCaptura = geolocalizacao.TimestampCaptura
                        };
                    }
                }

                Console.WriteLine($"[TERMO] ✅ Retornando termo - Recursos: {recursos.Count}, Colaborador: {colaboradorNome}");
                Console.WriteLine($"[TERMO] ========== FIM VALIDAÇÃO TERMO ==========");
                
                return Ok(new {
                    sucesso = true,
                    assinado,
                    dataAssinatura = dtAssinatura,
                    entreguePor,
                    colaborador = colaboradorNome,
                    hash,
                    recursos,
                    auditoria = dadosAuditoria
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
        }
    }
}


