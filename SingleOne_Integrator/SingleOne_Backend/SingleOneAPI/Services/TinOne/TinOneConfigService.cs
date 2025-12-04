using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.Models.TinOne;
using SingleOneAPI.DTOs.TinOne;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleOneAPI.Services.TinOne
{
    /// <summary>
    /// Serviço para gerenciar configurações do TinOne através dos parâmetros do sistema
    /// </summary>
    public class TinOneConfigService : ITinOneConfigService
    {
        private readonly SingleOneDbContext _context;
        private readonly ILogger<TinOneConfigService> _logger;

        public TinOneConfigService(SingleOneDbContext context, ILogger<TinOneConfigService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public TinOneConfig GetConfig(int? clienteId = null)
        {
            try
            {
                // Busca configurações do TinOne (primeiro específicas do cliente, depois globais)
                var configs = _context.Set<TinOneConfigEntity>()
                    .Where(c => c.Ativo == true &&
                               c.Chave.StartsWith("TINONE_") &&
                               (c.Cliente == clienteId || c.Cliente == null))
                    .OrderBy(c => c.Cliente == null ? 1 : 0) // Prioriza configs do cliente
                    .ToList();

                // Remove duplicatas (mantém apenas a primeira ocorrência de cada chave)
                var configsUnicas = configs
                    .GroupBy(c => c.Chave)
                    .Select(g => g.First())
                    .ToList();

                var config = new TinOneConfig
                {
                    Habilitado = GetBoolConfig(configsUnicas, "TINONE_HABILITADO", true),
                    ChatHabilitado = GetBoolConfig(configsUnicas, "TINONE_CHAT_HABILITADO", true),
                    TooltipsHabilitado = GetBoolConfig(configsUnicas, "TINONE_TOOLTIPS_HABILITADO", true),
                    GuiasHabilitado = GetBoolConfig(configsUnicas, "TINONE_GUIAS_HABILITADO", false),
                    SugestoesProativas = GetBoolConfig(configsUnicas, "TINONE_SUGESTOES_PROATIVAS", false),
                    IaHabilitada = GetBoolConfig(configsUnicas, "TINONE_IA_HABILITADA", false),
                    Analytics = GetBoolConfig(configsUnicas, "TINONE_ANALYTICS", true),
                    DebugMode = GetBoolConfig(configsUnicas, "TINONE_DEBUG_MODE", false),
                    Posicao = GetStringConfig(configsUnicas, "TINONE_POSICAO", "bottom-right"),
                    CorPrimaria = GetStringConfig(configsUnicas, "TINONE_COR_PRIMARIA", "#4a90e2")
                };

                if (config.DebugMode)
                {
                    _logger.LogInformation($"[TinOne] Config carregada - Habilitado: {config.Habilitado}, Cliente: {clienteId?.ToString() ?? "Global"}");
                }

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao carregar configuração");
                
                // Retorna configuração padrão em caso de erro
                return new TinOneConfig
                {
                    Habilitado = false // Desabilita em caso de erro para não quebrar o sistema
                };
            }
        }

        public bool IsEnabled(int? clienteId = null)
        {
            try
            {
                var config = GetConfig(clienteId);
                return config.Habilitado;
            }
            catch
            {
                return false; // Seguro: desabilita em caso de erro
            }
        }

        public bool IsFeatureEnabled(string featureName, int? clienteId = null)
        {
            try
            {
                var config = GetConfig(clienteId);
                
                if (!config.Habilitado)
                    return false;

                return featureName.ToUpper() switch
                {
                    "CHAT" => config.ChatHabilitado,
                    "TOOLTIPS" => config.TooltipsHabilitado,
                    "GUIAS" => config.GuiasHabilitado,
                    "SUGESTOES" => config.SugestoesProativas,
                    "IA" => config.IaHabilitada,
                    "ANALYTICS" => config.Analytics,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private bool GetBoolConfig(List<TinOneConfigEntity> configs, string chave, bool valorPadrao)
        {
            var config = configs.FirstOrDefault(c => c.Chave == chave);
            if (config == null || string.IsNullOrWhiteSpace(config.Valor))
                return valorPadrao;

            return config.Valor.Trim().ToLower() == "true";
        }

        private string GetStringConfig(List<TinOneConfigEntity> configs, string chave, string valorPadrao)
        {
            var config = configs.FirstOrDefault(c => c.Chave == chave);
            if (config == null || string.IsNullOrWhiteSpace(config.Valor))
                return valorPadrao;

            return config.Valor.Trim();
        }

        public List<TinOneConfigItemDTO> GetAllConfigs(int? clienteId = null)
        {
            try
            {
                var configs = _context.Set<TinOneConfigEntity>()
                    .Where(c => c.Chave.StartsWith("TINONE_") &&
                               (c.Cliente == clienteId || c.Cliente == null))
                    .OrderBy(c => c.Cliente == null ? 1 : 0)
                    .ToList();

                // Remove duplicatas (mantém apenas a primeira ocorrência de cada chave)
                var configsUnicas = configs
                    .GroupBy(c => c.Chave)
                    .Select(g => g.First())
                    .ToList();

                return configsUnicas.Select(c => new TinOneConfigItemDTO
                {
                    Id = c.Id,
                    Cliente = c.Cliente,
                    Chave = c.Chave,
                    Valor = c.Valor,
                    Descricao = c.Descricao,
                    Ativo = c.Ativo
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao buscar todas as configurações");
                return new List<TinOneConfigItemDTO>();
            }
        }

        public void SaveConfigs(List<TinOneConfigItemDTO> configuracoes)
        {
            try
            {
                _logger.LogInformation($"[TinOne] Iniciando salvamento de {configuracoes.Count} configurações");
                
                foreach (var configDTO in configuracoes)
                {
                    _logger.LogInformation($"[TinOne] Processando config: {configDTO.Chave} - ID: {configDTO.Id} - Valor: '{configDTO.Valor}' - Ativo: {configDTO.Ativo}");
                    
                    // Se tem ID, atualiza
                    if (configDTO.Id.HasValue && configDTO.Id.Value > 0)
                    {
                        // Busca a entidade para atualizar COM TRACKING
                        var configExistente = _context.Set<TinOneConfigEntity>()
                            .AsTracking() // 🔥 FORÇA O TRACKING DESTA QUERY
                            .FirstOrDefault(c => c.Id == configDTO.Id.Value);

                        if (configExistente != null)
                        {
                            _logger.LogInformation($"[TinOne] Config encontrada - Valor Atual: '{configExistente.Valor}' -> Novo Valor: '{configDTO.Valor}'");
                            
                            // Atualiza os valores
                            configExistente.Valor = configDTO.Valor;
                            configExistente.Descricao = configDTO.Descricao;
                            configExistente.Ativo = configDTO.Ativo;
                            configExistente.UpdatedAt = DateTime.UtcNow;
                            
                            // Marca explicitamente como modificado
                            _context.Entry(configExistente).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            
                            _logger.LogInformation($"[TinOne] Config marcada como modificada: {configExistente.Chave} = '{configExistente.Valor}'");
                        }
                        else
                        {
                            _logger.LogWarning($"[TinOne] Configuração com ID {configDTO.Id} não encontrada para atualizar");
                        }
                    }
                    else
                    {
                        // Se não tem ID, insere
                        var novaConfig = new TinOneConfigEntity
                        {
                            Cliente = configDTO.Cliente,
                            Chave = configDTO.Chave,
                            Valor = configDTO.Valor,
                            Descricao = configDTO.Descricao,
                            Ativo = configDTO.Ativo,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Add(novaConfig);
                        _logger.LogInformation($"[TinOne] Inserindo nova configuração: {novaConfig.Chave} = '{novaConfig.Valor}'");
                    }
                }

                _logger.LogInformation("[TinOne] Chamando SaveChanges()...");
                var changesSaved = _context.SaveChanges();
                _logger.LogInformation($"[TinOne] ✅ {changesSaved} alterações salvas no banco de dados com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] ❌ Erro ao salvar configurações no banco de dados");
                _logger.LogError($"[TinOne] Detalhes do erro: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"[TinOne] Erro interno: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}

