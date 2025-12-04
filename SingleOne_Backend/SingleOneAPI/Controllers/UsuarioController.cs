using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SingleOne.Jwt;
using SingleOne.Models;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioNegocio _negocio;
        private readonly ITwoFactorService _twoFactorService;
        
        // Armazenamento temporário dos códigos 2FA com timestamp (em memória)
        private static readonly Dictionary<int, (string Code, DateTime Timestamp)> _twoFactorCodes = new Dictionary<int, (string, DateTime)>();
        
        // Tempo de expiração do código 2FA (5 minutos)
        private static readonly TimeSpan CODE_EXPIRATION_TIME = TimeSpan.FromMinutes(5);
        
        // Método para limpar códigos expirados
        private void CleanupExpiredCodes()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _twoFactorCodes
                .Where(kvp => (now - kvp.Value.Timestamp) > CODE_EXPIRATION_TIME)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in expiredKeys)
            {
                _twoFactorCodes.Remove(key);
            }
        }

        public UsuarioController(IUsuarioNegocio negocio, ITwoFactorService twoFactorService)
        {
            _negocio = negocio;
            _twoFactorService = twoFactorService;
        }

        [AllowAnonymous]
        [HttpGet("[action]/{email}", Name = "RecuperarPalavraPasse")]
        public async Task<IActionResult> RecuperarPalavraChave(string email)
        {
            try
            {
                await _negocio.RecuperarPalavraChave(email);
                return Ok(new { Mensagem = "E-mail de recuperação enviado com sucesso." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] Erro ao recuperar palavra-chave: {ex.Message}");
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("[action]/{token}", Name = "RedirectValidarToken")]
        public IActionResult RedirectValidarToken(string token)
        {
            // Redirecionamento direto para evitar tracking do Brevo
            var frontendUrl = _negocio.GetFrontendUrl();
            var redirectUrl = $"{frontendUrl}/validar-token/{token}";
            return Redirect(redirectUrl);
        }

        [AllowAnonymous]
        [HttpPost("[action]", Name = "RecuperarSenha")]
        public void RecuperarSenha([FromBody] Usuario usuario)
        {
            _negocio.RecuperarSenha(usuario);
        }

        // POST: api/Usuario
        [HttpPost]
        public JsonResult Post([FromBody] Usuario usuario)
        {
            try
            {
                var resultado = _negocio.Salvar(usuario);
                var objetoResultado = JsonConvert.DeserializeObject<dynamic>(resultado);
                return new JsonResult(objetoResultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERRO ao salvar usuário: {ex.Message}");
                return new JsonResult(new { Mensagem = ex.Message, Status = "500" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<dynamic>> Login(Usuario usr)
        {
            try
            {
                var usuario = _negocio.Logar(usr);
                
                if (usuario != null)
                {
                    var twoFactorRequired = await _twoFactorService.IsTwoFactorRequiredAsync(usuario.Id);
                    
                    if (twoFactorRequired)
                    {
                        var response = new
                        {
                            usuario = new
                            {
                                id = usuario.Id,
                                nome = usuario.Nome,
                                email = usuario.Email,
                                su = usuario.Su,
                                adm = usuario.Adm,
                                operador = usuario.Operador,
                                consulta = usuario.Consulta,
                                cliente = usuario.Cliente
                            },
                            twoFactorRequired = true,
                            message = "Verificação de duplo fator necessária"
                        };
                        
                        return Ok(response);
                    }
                    else
                    {
                        var token = JwtTokenService.GenerateToken(usuario);
                        var response = new
                        {
                            usuario = new
                            {
                                id = usuario.Id,
                                nome = usuario.Nome,
                                email = usuario.Email,
                                su = usuario.Su,
                                adm = usuario.Adm,
                                operador = usuario.Operador,
                                consulta = usuario.Consulta,
                                cliente = usuario.Cliente
                            },
                            token = token,
                            twoFactorRequired = false
                        };
                        
                        return Ok(response);
                    }
                }
                else
                {
                    return Unauthorized(new { Mensagem = "Usuário/Senha não encontrado.", Status = "401" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERRO Login: {ex.Message}");
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<dynamic>> VerifyTwoFactor(TwoFactorVerificationRequest request)
        {
            try
            {
                bool isValid = false;
                
                if (request.VerificationType == "totp")
                {
                    // Verificar código TOTP
                    var usuario = _negocio.BuscarPorId(request.UserId);
                    if (usuario?.TwoFactorSecret != null)
                    {
                        isValid = await _twoFactorService.ValidateTOTPCodeAsync(usuario.TwoFactorSecret, request.Code);
                    }
                }
                else if (request.VerificationType == "backup")
                {
                    // Verificar código de backup
                    isValid = await _twoFactorService.ValidateBackupCodeAsync(request.UserId, request.Code);
                }
                else if (request.VerificationType == "email")
                {
                    CleanupExpiredCodes();
                    
                    if (_twoFactorCodes.TryGetValue(request.UserId, out var storedData))
                    {
                        var (storedCode, timestamp) = storedData;
                        var now = DateTime.UtcNow;
                        var timeElapsed = now - timestamp;
                        
                        // Verificar se o código expirou (5 minutos)
                        if (timeElapsed > CODE_EXPIRATION_TIME)
                        {
                            _twoFactorCodes.Remove(request.UserId);
                            return BadRequest(new { 
                                success = false, 
                                message = "Código de verificação expirado. Por favor, solicite um novo código." 
                            });
                        }
                        
                        // Verificar se o código está correto
                        isValid = storedCode == request.Code;
                        
                        // Remover o código após verificação (usado apenas uma vez)
                        if (isValid)
                        {
                            _twoFactorCodes.Remove(request.UserId);
                        }
                    }
                    else
                    {
                        isValid = false;
                    }
                }
                
                if (isValid)
                {
                    var usuario = _negocio.BuscarPorId(request.UserId);
                    var token = JwtTokenService.GenerateToken(usuario);
                    
                    // Atualizar último uso do 2FA
                    usuario.TwoFactorLastUsed = DateTime.Now;
                    
                    try
                    {
                        _negocio.Salvar(usuario);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[2FA] Erro ao atualizar TwoFactorLastUsed: {ex.Message}");
                    }
                    
                    var response = new
                    {
                        success = true,
                        message = "Verificação realizada com sucesso",
                        usuario = new
                        {
                            id = usuario.Id,
                            nome = usuario.Nome,
                            email = usuario.Email,
                            su = usuario.Su,
                            adm = usuario.Adm,
                            operador = usuario.Operador,
                            consulta = usuario.Consulta,
                            cliente = usuario.Cliente
                        },
                        token = token
                    };
                    
                    return Ok(response);
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Código de verificação inválido" 
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[2FA] Erro na verificação: {ex.Message}");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Erro interno ao verificar código" 
                });
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<dynamic>> EnableTwoFactor(TwoFactorSetupRequest request)
        {
            try
            {
                // VALIDAÇÃO 2FA ANTES DE TENTAR HABILITAR
                var usuario = _negocio.BuscarPorId(request.UserId);
                if (usuario == null)
                {
                    return BadRequest(new { Mensagem = "Usuário não encontrado" });
                }

                // Verificar se 2FA global está habilitado para o cliente
                Console.WriteLine($"[CONTROLLER] === VALIDAÇÃO 2FA GLOBAL ===");
                Console.WriteLine($"[CONTROLLER] Usuário ID: {usuario.Id}, Cliente: {usuario.Cliente}");
                
                var configuracaoGlobal = _negocio.IsTwoFactorEnabledGlobally(usuario.Cliente);
                Console.WriteLine($"[CONTROLLER] Configuração global 2FA para cliente {usuario.Cliente}: {configuracaoGlobal}");
                
                if (!configuracaoGlobal)
                {
                    Console.WriteLine($"[CONTROLLER] ✗ BLOQUEADO: 2FA global desabilitado para cliente {usuario.Cliente}");
                    return BadRequest(new { 
                        Mensagem = "Para habilitar 2FA para usuários, é necessário ativar primeiro a funcionalidade nas configurações globais do cliente.",
                        Status = "400",
                        CodigoErro = "2FA_GLOBAL_DESABILITADO",
                        Detalhes = "2FA global está desabilitado para este cliente"
                    });
                }
                
                Console.WriteLine($"[CONTROLLER] ✓ PERMITIDO: 2FA global habilitado para cliente {usuario.Cliente}");
                Console.WriteLine($"[CONTROLLER] === FIM VALIDAÇÃO 2FA GLOBAL ===");

                var secret = await _twoFactorService.GenerateTOTPSecretAsync();
                var backupCodes = await _twoFactorService.GenerateBackupCodesAsync();
                
                var success = await _twoFactorService.EnableTwoFactorAsync(request.UserId, secret, backupCodes);
                
                if (success)
                {
                    var response = new
                    {
                        secret = secret,
                        backupCodes = backupCodes,
                        message = "2FA habilitado com sucesso"
                    };
                    
                    return Ok(response);
                }
                else
                {
                    return BadRequest(new { Mensagem = "Erro ao habilitar 2FA" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<dynamic>> DisableTwoFactor([FromBody] TwoFactorSetupRequest request)
        {
            try
            {
                var success = await _twoFactorService.DisableTwoFactorAsync(request.UserId);
                
                if (success)
                {
                    return Ok(new { message = "2FA desabilitado com sucesso" });
                }
                else
                {
                    return BadRequest(new { Mensagem = "Erro ao desabilitar 2FA" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<dynamic>> SendTwoFactorCode([FromBody] TwoFactorCodeRequest request)
        {
            try
            {
                CleanupExpiredCodes();
                
                var random = new Random();
                var code = random.Next(100000, 999999).ToString();
                
                var timestamp = DateTime.UtcNow;
                _twoFactorCodes[request.UserId] = (code, timestamp);
                
                var resultado = await _negocio.EnviarCodigoTwoFactor(request.Email, code);
                
                if (resultado)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Código de verificação enviado para seu e-mail",
                        email = request.Email
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Falha ao enviar código de verificação" 
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[2FA] Erro ao enviar código: {ex.Message}");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Erro interno ao enviar código de verificação" 
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<dynamic>> VerifyTwoFactorCode([FromBody] TwoFactorVerificationRequest request)
        {
            try
            {
                Console.WriteLine($"[CONTROLLER] === INÍCIO VERIFICAÇÃO CÓDIGO 2FA ===");
                Console.WriteLine($"[CONTROLLER] Usuário ID: {request.UserId}");
                Console.WriteLine($"[CONTROLLER] Código recebido: {request.Code}");
                Console.WriteLine($"[CONTROLLER] Tipo verificação: {request.VerificationType}");
                
                // Verificar código 2FA
                var resultado = await _negocio.VerificarCodigoTwoFactor(request.UserId, request.Code);
                
                if (resultado.Success)
                {
                    Console.WriteLine($"[CONTROLLER] ✅ Código 2FA verificado com sucesso para usuário ID: {request.UserId}");
                    Console.WriteLine($"[CONTROLLER] === FIM VERIFICAÇÃO CÓDIGO 2FA (SUCESSO) ===");
                    
                    return Ok(new { 
                        success = true, 
                        message = "Verificação realizada com sucesso",
                        usuario = resultado.Usuario,
                        token = resultado.Token
                    });
                }
                else
                {
                    Console.WriteLine($"[CONTROLLER] ❌ Código 2FA inválido para usuário ID: {request.UserId}");
                    Console.WriteLine($"[CONTROLLER] === FIM VERIFICAÇÃO CÓDIGO 2FA (FALHA) ===");
                    
                    return BadRequest(new { 
                        success = false, 
                        message = resultado.Message ?? "Código de verificação inválido" 
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERRO ao verificar código 2FA: {ex.Message}");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Erro interno ao verificar código" 
                });
            }
        }

        [HttpGet("[action]/{token}", Name ="Revoke")]
        public void Revoke(string token)
        {
            JwtTokenService.RevokeToken(token);
        }

        [HttpGet]
        public List<Usuario> Get()
        {
            return _negocio.ListarUsuarios();
        }

        [HttpGet("{id}")]
        public Usuario Get(int id)
        {
            return _negocio.BuscarPorId(id);
        }

        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarUsuarios")]
        public List<Usuario> ListarUsuarios(string pesquisa, int cliente)
        {
            try
            {
                Console.WriteLine($"[CONTROLLER] === LISTAR USUÁRIOS ===");
                Console.WriteLine($"[CONTROLLER] Pesquisa: {pesquisa}");
                Console.WriteLine($"[CONTROLLER] Cliente: {cliente}");
                Console.WriteLine($"[CONTROLLER] User.Identity.Name: {User.Identity?.Name}");
                Console.WriteLine($"[CONTROLLER] User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"[CONTROLLER] Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
                
                // Verificar se o usuário logado é um Super Usuário
                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                bool usuarioLogadoEhSuper = false;
                
                if (!string.IsNullOrEmpty(emailClaim))
                {
                    // Buscar o usuário logado para verificar se é Super Usuário
                    var todosUsuarios = _negocio.ListarUsuarios(); // Buscar todos os usuários
                    var usuarioLogado = todosUsuarios.FirstOrDefault(u => u.Email?.ToLower() == emailClaim.ToLower());
                    usuarioLogadoEhSuper = usuarioLogado?.Su == true;
                    
                    Console.WriteLine($"[CONTROLLER] Email do usuário logado: {emailClaim}");
                    Console.WriteLine($"[CONTROLLER] Usuário logado é Super Usuário: {usuarioLogadoEhSuper}");
                }
                
                var usuarios = _negocio.ListarUsuarios(pesquisa, cliente, usuarioLogadoEhSuper);
                Console.WriteLine($"[CONTROLLER] Usuários encontrados: {usuarios?.Count ?? 0}");
                Console.WriteLine($"[CONTROLLER] === FIM LISTAR USUÁRIOS ===");
                
                return usuarios;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERRO ao listar usuários: {ex.Message}");
                Console.WriteLine($"[CONTROLLER] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [HttpDelete("[action]/{id}", Name ="ExcluirUsuario")]
        public ActionResult ExcluirUsuario(int id)
        {
            try
            {
                _negocio.ExcluirUsuario(id);
                
                return Ok(new { 
                    Success = true,
                    Message = "Usuário desativado com sucesso. Dados preservados para auditoria.",
                    UsuarioId = id,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    Success = false,
                    Message = ex.Message,
                    UsuarioId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] Erro ao desativar usuário {id}: {ex.Message}");
                return StatusCode(500, new { 
                    Success = false,
                    Message = "Erro interno ao desativar usuário",
                    UsuarioId = id
                });
            }
        }

        /// <summary>
        /// Verifica o status de 2FA de um usuário específico
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <returns>Status de 2FA do usuário</returns>
        [HttpGet("[action]/{id}", Name = "GetTwoFactorStatus")]
        public ActionResult<dynamic> GetTwoFactorStatus(int id)
        {
            try
            {
                Console.WriteLine($"[CONTROLLER] === VERIFICAR STATUS 2FA ===");
                Console.WriteLine($"[CONTROLLER] Usuário ID: {id}");
                
                var status = _negocio.GetUserTwoFactorStatus(id);
                
                if (status.Success)
                {
                    Console.WriteLine($"[CONTROLLER] ✓ Status 2FA obtido com sucesso");
                    Console.WriteLine($"[CONTROLLER] 2FA Global: {status.TwoFactorEnabledGlobally}");
                    Console.WriteLine($"[CONTROLLER] 2FA Individual: {status.TwoFactorEnabledIndividually}");
                    Console.WriteLine($"[CONTROLLER] Pode ativar: {status.CanEnableTwoFactor}");
                    Console.WriteLine($"[CONTROLLER] === FIM VERIFICAR STATUS 2FA ===");
                    
                    return Ok(status);
                }
                else
                {
                    Console.WriteLine($"[CONTROLLER] ✗ Erro ao obter status 2FA: {status.Message}");
                    Console.WriteLine($"[CONTROLLER] === FIM VERIFICAR STATUS 2FA ===");
                    
                    return NotFound(status);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERRO ao verificar status 2FA: {ex.Message}");
                Console.WriteLine($"[CONTROLLER] === FIM VERIFICAR STATUS 2FA ===");
                
                return BadRequest(new { 
                    Success = false, 
                    Message = $"Erro interno: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Verifica se o 2FA está habilitado globalmente para um cliente
        /// </summary>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Status global de 2FA do cliente</returns>
        [HttpGet("[action]/{clienteId}", Name = "GetGlobalTwoFactorStatus")]
        public ActionResult<dynamic> GetGlobalTwoFactorStatus(int clienteId)
        {
            try
            {
                Console.WriteLine($"[CONTROLLER] === VERIFICAR STATUS 2FA GLOBAL ===");
                Console.WriteLine($"[CONTROLLER] Cliente ID: {clienteId}");
                
                var twoFactorEnabled = _negocio.IsTwoFactorEnabledGlobally(clienteId);
                
                var resultado = new
                {
                    ClienteId = clienteId,
                    TwoFactorEnabledGlobally = twoFactorEnabled,
                    Message = twoFactorEnabled 
                        ? "2FA está habilitado globalmente para este cliente" 
                        : "2FA não está habilitado globalmente para este cliente",
                    Timestamp = DateTime.UtcNow
                };
                
                Console.WriteLine($"[CONTROLLER] ✓ Status global 2FA obtido: {twoFactorEnabled}");
                Console.WriteLine($"[CONTROLLER] === FIM VERIFICAR STATUS 2FA GLOBAL ===");
                
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERRO ao verificar status global 2FA: {ex.Message}");
                Console.WriteLine($"[CONTROLLER] === FIM VERIFICAR STATUS 2FA GLOBAL ===");
                
                return BadRequest(new { 
                    Success = false, 
                    Message = $"Erro interno: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Lista usuários ativos para atribuição de investigador
        /// </summary>
        /// <returns>Lista de usuários ativos</returns>
        [AllowAnonymous]
        [HttpGet("[action]", Name = "GetUsuariosAtivos")]
        public ActionResult<List<dynamic>> GetUsuariosAtivos()
        {
            try
            {
                Console.WriteLine($"[CONTROLLER] === BUSCAR USUÁRIOS ATIVOS ===");
                
                var usuarios = _negocio.ListarUsuarios();
                var usuariosAtivos = usuarios.Where(u => u.Ativo == true)
                    .Select(u => new
                    {
                        id = u.Id,
                        nome = u.Nome,
                        email = u.Email,
                        ativo = u.Ativo,
                        su = u.Su,
                        adm = u.Adm,
                        operador = u.Operador,
                        consulta = u.Consulta
                    })
                    .ToList();

                Console.WriteLine($"[CONTROLLER] Usuários ativos encontrados: {usuariosAtivos.Count}");
                Console.WriteLine($"[CONTROLLER] === FIM BUSCAR USUÁRIOS ATIVOS ===");
                
                return Ok(usuariosAtivos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERRO ao buscar usuários ativos: {ex.Message}");
                Console.WriteLine($"[CONTROLLER] === FIM BUSCAR USUÁRIOS ATIVOS ===");
                
                return BadRequest(new { 
                    Success = false, 
                    Message = $"Erro interno: {ex.Message}" 
                });
            }
        }
    }

    public class TwoFactorVerificationRequest
    {
        public int UserId { get; set; }
        public string Code { get; set; }
        public string VerificationType { get; set; } // "totp", "backup", "email"
        public string StoredCode { get; set; } // Para verificação por email
    }

    public class TwoFactorSetupRequest
    {
        public int UserId { get; set; }
    }

    public class TwoFactorCodeRequest
    {
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}
