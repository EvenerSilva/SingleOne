using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SingleOneAPI.DTOs.TinOne;
using SingleOneAPI.Services.TinOne;
using System;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    /// <summary>
    /// Controller do assistente TinOne
    /// Totalmente isolado - não afeta funcionalidades existentes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TinOneController : ControllerBase
    {
        private readonly ITinOneService _tinOneService;
        private readonly ITinOneConfigService _configService;
        private readonly ILogger<TinOneController> _logger;

        public TinOneController(
            ITinOneService tinOneService,
            ITinOneConfigService configService,
            ILogger<TinOneController> logger)
        {
            _tinOneService = tinOneService;
            _configService = configService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém a configuração do TinOne
        /// </summary>
        [HttpGet("config")]
        [AllowAnonymous] // Pode ser acessado antes do login
        public IActionResult GetConfig([FromQuery] int? clienteId = null)
        {
            try
            {
                var config = _configService.GetConfig(clienteId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao buscar configuração");
                return StatusCode(500, new { erro = "Erro ao buscar configuração do TinOne" });
            }
        }

        /// <summary>
        /// Processa uma pergunta do usuário
        /// </summary>
        [HttpPost("ask")]
        [Authorize]
        public async Task<IActionResult> Ask([FromBody] TinOnePerguntaDTO pergunta)
        {
            try
            {
                // Verifica se TinOne está habilitado
                if (!_configService.IsEnabled(pergunta.ClienteId))
                {
                    return BadRequest(new { erro = "TinOne não está habilitado" });
                }

                if (string.IsNullOrWhiteSpace(pergunta.Pergunta))
                {
                    return BadRequest(new { erro = "Pergunta não pode ser vazia" });
                }

                var resposta = await _tinOneService.ProcessarPerguntaAsync(pergunta);
                return Ok(resposta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao processar pergunta");
                return StatusCode(500, new 
                { 
                    erro = "Erro ao processar pergunta",
                    resposta = "Desculpe, tive um problema ao processar sua pergunta. Tente novamente." 
                });
            }
        }

        /// <summary>
        /// Obtém informações sobre um campo específico
        /// </summary>
        [HttpGet("field/{fieldId}")]
        [Authorize]
        public async Task<IActionResult> GetFieldInfo(string fieldId)
        {
            try
            {
                var fieldInfo = await _tinOneService.GetCampoInfoAsync(fieldId);
                
                if (fieldInfo == null)
                {
                    return NotFound(new { erro = "Campo não encontrado na base de conhecimento" });
                }

                return Ok(fieldInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TinOne] Erro ao buscar info do campo: {fieldId}");
                return StatusCode(500, new { erro = "Erro ao buscar informações do campo" });
            }
        }

        /// <summary>
        /// Obtém um processo guiado
        /// </summary>
        [HttpGet("process/{processId}")]
        [Authorize]
        public async Task<IActionResult> GetProcess(string processId)
        {
            try
            {
                var processo = await _tinOneService.GetProcessoAsync(processId);
                
                if (processo == null)
                {
                    return NotFound(new { erro = "Processo não encontrado" });
                }

                return Ok(processo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TinOne] Erro ao buscar processo: {processId}");
                return StatusCode(500, new { erro = "Erro ao buscar processo" });
            }
        }

        /// <summary>
        /// Registra feedback do usuário sobre uma resposta
        /// </summary>
        [HttpPost("feedback")]
        [Authorize]
        public async Task<IActionResult> Feedback([FromBody] TinOneFeedbackDTO feedback)
        {
            try
            {
                await _tinOneService.RegistrarFeedbackAsync(feedback);
                return Ok(new { mensagem = "Feedback registrado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao registrar feedback");
                return StatusCode(500, new { erro = "Erro ao registrar feedback" });
            }
        }

        /// <summary>
        /// Verifica se o TinOne está habilitado (health check)
        /// </summary>
        [HttpGet("status")]
        [AllowAnonymous]
        public IActionResult GetStatus([FromQuery] int? clienteId = null)
        {
            try
            {
                var isEnabled = _configService.IsEnabled(clienteId);
                var config = _configService.GetConfig(clienteId);

                return Ok(new 
                { 
                    habilitado = isEnabled,
                    versao = "1.0.0",
                    features = new
                    {
                        chat = config.ChatHabilitado,
                        tooltips = config.TooltipsHabilitado,
                        guias = config.GuiasHabilitado,
                        ia = config.IaHabilitada
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao buscar status");
                return StatusCode(500, new { erro = "Erro ao buscar status" });
            }
        }

        /// <summary>
        /// Obtém todas as configurações do TinOne
        /// </summary>
        [HttpGet("configuracoes")]
        [Authorize]
        public IActionResult GetConfiguracoes([FromQuery] int? clienteId = null)
        {
            try
            {
                var configuracoes = _configService.GetAllConfigs(clienteId);
                return Ok(configuracoes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao buscar configurações");
                return StatusCode(500, new { erro = "Erro ao buscar configurações" });
            }
        }

        /// <summary>
        /// Salva as configurações do TinOne
        /// </summary>
        [HttpPost("configuracoes")]
        [Authorize]
        public IActionResult SaveConfiguracoes([FromBody] System.Collections.Generic.List<DTOs.TinOne.TinOneConfigItemDTO> configuracoes)
        {
            try
            {
                _configService.SaveConfigs(configuracoes);
                _logger.LogInformation("[TinOne] Configurações salvas com sucesso");
                return Ok(new { mensagem = "Configurações salvas com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao salvar configurações");
                return StatusCode(500, new { erro = "Erro ao salvar configurações" });
            }
        }
    }
}

