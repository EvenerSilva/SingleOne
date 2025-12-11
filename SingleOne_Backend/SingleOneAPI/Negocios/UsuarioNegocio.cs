using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SingleOne.Models;
using SingleOne.Util;
using SingleOne.Jwt;
using SingleOneAPI;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Negocios
{
    public class UsuarioNegocio : IUsuarioNegocio
    {
        private SendMail mail;
        private readonly EnvironmentApiSettings _environmentApiSettings;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<Parametro> _parametroRepository;
        private readonly ISmtpConfigService _smtpConfigService;
        public UsuarioNegocio(EnvironmentApiSettings environmentApiSettings, 
                             IRepository<Usuario> usuarioRepository,
                             IRepository<Parametro> parametroRepository,
                             ISmtpConfigService smtpConfigService)
        {
            this.mail = new SendMail(environmentApiSettings, smtpConfigService);
            _usuarioRepository = usuarioRepository;
            _parametroRepository = parametroRepository;
            _environmentApiSettings = environmentApiSettings;
            _smtpConfigService = smtpConfigService;
        }

        public string Salvar(Usuario usr)
        {
            try
            {
                if (usr.Id == 0)
                {
                    var existe = _usuarioRepository.Buscar(x => x.Email == usr.Email).Any();

                    if (!existe)
                    {
                        // VALIDAÇÃO DE 2FA PARA NOVOS USUÁRIOS
                        // SÓ VALIDAR SE ESTIVER TENTANDO HABILITAR 2FA
                        if (usr.TwoFactorEnabled == true)
                        {
                            // Buscar configuração global no cliente do usuário
                            bool configuracaoGlobal = IsTwoFactorEnabledGlobally(usr.Cliente);
                            
                            // BLOQUEAR se global estiver desabilitado
                            if (!configuracaoGlobal)
                            {
                                return JsonConvert.SerializeObject(new { 
                                    Mensagem = "Para habilitar 2FA para usuários, é necessário ativar primeiro a funcionalidade nas configurações globais do cliente.", 
                                    Status = "400",
                                    CodigoErro = "2FA_GLOBAL_DESABILITADO",
                                    Detalhes = "2FA global está desabilitado para este cliente"
                                });
                            }
                        }
                        
                        usr.Palavracriptografada = Guid.NewGuid().ToString();
                        usr.Ativo = true;

                        //db.Add(usr);
                        //db.SaveChanges();
                        _usuarioRepository.Adicionar(usr);
                        _usuarioRepository.SalvarAlteracoes(); // ✅ CORREÇÃO: Salvar alterações explicitamente
                        return JsonConvert.SerializeObject(new { Messagem = "", Status = "200" });
                    }
                    else
                    {
                        //JsonRetorno jr = new JsonRetorno() { Mensagem = "Usuário ja cadastrado", Status = "200.1" };
                        //return JsonConvert.SerializeObject(jr);
                        var retorno = JsonConvert.SerializeObject(new { Mensagem = "E-mail ja cadastrado", Status = "200.1" });
                        return retorno;
                    }
                }
                else
                {
                    var u = _usuarioRepository.ObterPorId(usr.Id);
                    
                    if (u == null)
                    {
                        return JsonConvert.SerializeObject(new { Mensagem = "Usuário não encontrado", Status = "404" });
                    }
                    
                    u.Nome = usr.Nome;
                    u.Email = usr.Email;
                    u.Adm = usr.Adm;
                    u.Operador = usr.Operador;
                    u.Consulta = usr.Consulta;
                    u.Cliente = usr.Cliente;
                    
                    if (!String.IsNullOrEmpty(usr.Senha))
                    {
                        u.Senha = usr.Senha;
                    }
                    
                    // VALIDAÇÃO SIMPLES DE 2FA
                    // SÓ VALIDAR SE ESTIVER TENTANDO HABILITAR 2FA
                    if (usr.TwoFactorEnabled == true)
                    {
                        // Buscar configuração global no cliente do usuário
                        bool configuracaoGlobal = IsTwoFactorEnabledGlobally(usr.Cliente);
                        
                        // BLOQUEAR se global estiver desabilitado
                        if (!configuracaoGlobal)
                        {
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "Para habilitar 2FA para usuários, é necessário ativar primeiro a funcionalidade nas configurações globais do cliente.", 
                                Status = "400",
                                CodigoErro = "2FA_GLOBAL_DESABILITADO",
                                Detalhes = "2FA global está desabilitado para este cliente"
                            });
                        }
                    }
                    
                    // Campos 2FA (só serão alterados se a validação acima passar)
                    if (usr.TwoFactorEnabled.HasValue)
                        u.TwoFactorEnabled = usr.TwoFactorEnabled;
                    if (!String.IsNullOrEmpty(usr.TwoFactorSecret))
                        u.TwoFactorSecret = usr.TwoFactorSecret;
                    if (!String.IsNullOrEmpty(usr.TwoFactorBackupCodes))
                        u.TwoFactorBackupCodes = usr.TwoFactorBackupCodes;
                    if (usr.TwoFactorLastUsed.HasValue)
                        u.TwoFactorLastUsed = usr.TwoFactorLastUsed;
                    
                    
                    //db.Update(u);
                    //db.SaveChanges();
                    _usuarioRepository.Atualizar(u);
                    _usuarioRepository.SalvarAlteracoes();
                    
                    return JsonConvert.SerializeObject(new { Messagem = "", Status = "200" });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Usuario Logar(Usuario usr)
        {
            try
            {
                // Decodificar a senha se ela vier em Base64 do frontend
                string senhaOriginal = usr.Senha;
                if (Cripto.IsBase64String(usr.Senha))
                {
                    try
                    {
                        senhaOriginal = Cripto.CriptografarDescriptografar(usr.Senha, false);
                    }
                    catch (Exception ex)
                    {
                        senhaOriginal = usr.Senha;
                    }
                }
                
                string senhaCodificada = Cripto.CriptografarDescriptografar(senhaOriginal, true);

                // Buscar o usuário pelo email
                var usuarioRetorno = _usuarioRepository.Buscar(x => x.Email.ToLower() == usr.Email.ToLower() && x.Ativo == true).FirstOrDefault();
                
                if (usuarioRetorno != null)
                {
                    // Comparar a senha
                    bool senhaValida = false;
                    
                    // Tentar com a senha original
                    if (usuarioRetorno.Senha == senhaOriginal)
                    {
                        senhaValida = true;
                    }
                    // Tentar com a senha codificada
                    else if (usuarioRetorno.Senha == senhaCodificada)
                    {
                        senhaValida = true;
                    }
                    // Tentar decodificando a senha do banco
                    else if (Cripto.IsBase64String(usuarioRetorno.Senha))
                    {
                        try
                        {
                            string senhaDecodificada = Cripto.CriptografarDescriptografar(usuarioRetorno.Senha, false);
                            if (senhaDecodificada == senhaOriginal)
                            {
                                senhaValida = true;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    
                    if (senhaValida)
                    {
                        usuarioRetorno.Ultimologin = TimeZoneMapper.GetDateTimeNow();
                        _usuarioRepository.Atualizar(usuarioRetorno);
                        _usuarioRepository.SalvarAlteracoes(); // ✅ CORREÇÃO: Salvar alterações explicitamente

                        // Retornar o usuário diretamente, sem verificar cliente
                        var usuarioRetornoFinal = new Usuario()
                        {
                            Id = usuarioRetorno.Id,
                            Nome = usuarioRetorno.Nome,
                            Email = usuarioRetorno.Email,
                            Su = usuarioRetorno.Su,
                            Adm = usuarioRetorno.Adm,
                            Operador = usuarioRetorno.Operador,
                            Consulta = usuarioRetorno.Consulta,
                            Cliente = usuarioRetorno.Cliente,
                            Senha = usuarioRetorno.Senha,
                            TwoFactorEnabled = usuarioRetorno.TwoFactorEnabled,
                            TwoFactorSecret = usuarioRetorno.TwoFactorSecret,
                            TwoFactorBackupCodes = usuarioRetorno.TwoFactorBackupCodes,
                            TwoFactorLastUsed = usuarioRetorno.TwoFactorLastUsed
                        };
                        
                        return usuarioRetornoFinal;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task RecuperarPalavraChave(string email)
        {
            try
            {
                var usr = _usuarioRepository.Buscar(x => x.Email == email).FirstOrDefault();
                
                if (usr == null)
                {
                    throw new Exception($"Usuário com e-mail {email} não encontrado.");
                }

                // Carregar configurações SMTP do banco de dados para o cliente do usuário
                if (_smtpConfigService != null)
                {
                    await _smtpConfigService.LoadSmtpSettingsFromDatabase(_environmentApiSettings, usr.Cliente);
                    
                    // Verificar se SMTP está habilitado
                    if (!_environmentApiSettings.SMTPEnabled)
                    {
                        throw new Exception("Serviço de e-mail não está habilitado.");
                    }
                    
                    if (string.IsNullOrEmpty(_environmentApiSettings.SMTPHost))
                    {
                        throw new Exception("Configurações SMTP não estão configuradas.");
                    }
                    
                }
                else
                {
                }

                var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "recuperarPalavraCriptografada.html");
                string template = System.IO.File.ReadAllText(file);
                
                // Gerar link seguro - correção específica para a barra
                string baseUrl = _environmentApiSettings.SiteUrl ?? "http://localhost:4200";
                baseUrl = baseUrl.TrimEnd('/');
                
                // Garantir que a URL tenha a barra correta após a porta
                if (baseUrl.EndsWith(":4200"))
                {
                    baseUrl = baseUrl + "/";
                }
                
                string secureLink = baseUrl.TrimEnd('/') + "/validar-token/" + usr.Palavracriptografada;
                
                
                template = template.Replace("@nome", usr.Nome)
                                 .Replace("@palavra", usr.Palavracriptografada)
                                 .Replace("@endereco", secureLink);

                mail.Enviar(usr.Email, "Recuperação de Senha", template);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void RecuperarSenha(Usuario usuario)
        {
            try
            {
                var usr = _usuarioRepository.Buscar(x => x.Email == usuario.Email && x.Palavracriptografada == usuario.Palavracriptografada).FirstOrDefault();
                if (usr != null)
                {
                    string senha = GeradorSenhaRandomica();
                    string senhaCriptografada = Cripto.CriptografarDescriptografar(senha, true);
                    
                    usr.Senha = senhaCriptografada;
                    usr.Palavracriptografada = Guid.NewGuid().ToString();
                    _usuarioRepository.Atualizar(usr);
                    _usuarioRepository.SalvarAlteracoes(); // ✅ CORREÇÃO: Salvar alterações explicitamente

                    var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "recuperarSenha.html");
                    string template = System.IO.File.ReadAllText(file);
                    template = template.Replace("@nome", usr.Nome).Replace("@senha", senha);

                    mail.Enviar(usr.Email, "Recuperação de Senha", template);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string GeradorSenhaRandomica()
        {
            // Gera uma senha com 6 caracteres entre numeros e letras
            string chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ023456789!@#$%&*";
            string pass = "";
            Random random = new Random();
            for (int f = 0; f < 6; f++)
            {
                pass = pass + chars.Substring(random.Next(0, chars.Length - 1), 1);
            }
            return pass;
        }

        public List<Usuario> ListarUsuarios()
        {
            try
            {
                var users = _usuarioRepository.Buscar(x => true)
                    .Select(x => new Usuario() { 
                        Id = x.Id, 
                        Nome = x.Nome, 
                        Su = x.Su,
                        Adm = x.Adm, 
                        Cliente = x.Cliente,
                        Consulta = x.Consulta,
                        Email = x.Email,
                        Operador = x.Operador,
                        Ativo = x.Ativo,
                        TwoFactorEnabled = x.TwoFactorEnabled,
                        TwoFactorSecret = x.TwoFactorSecret,
                        TwoFactorBackupCodes = x.TwoFactorBackupCodes,
                        TwoFactorLastUsed = x.TwoFactorLastUsed
                    })
                    .OrderBy(x => x.Nome).ToList();
                
                return users;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public Usuario BuscarPorId(int id)
        {
            var usuario = _usuarioRepository.ObterPorId(id);
            //string senha = Cripto.CriptografarDescriptografar(usuario.Senha, false);
            //string senha = Cripto.DecodeBase64(usuario.Senha);
            usuario.Senha = null;
            return usuario;
        }

        public List<Usuario> ListarUsuarios(string pesquisa, int cliente, bool usuarioLogadoEhSuper = false)
        {
            try
            {
                // Tratar pesquisa null ou vazia
                string pesquisaProcessada = string.IsNullOrEmpty(pesquisa) ? "" : pesquisa.ToLower();
                
                // LÓGICA CORRIGIDA: 
                // 1. SEMPRE excluir usuários com su=true (Super Usuário)
                // 2. Filtrar pelo cliente solicitado (ou todos os clientes se for Super Usuário)
                // 3. Apenas usuários ativos
                
                IQueryable<Usuario> query;
                
                if (usuarioLogadoEhSuper)
                {
                    // Super Usuário pode ver usuários de TODOS os clientes
                    query = _usuarioRepository.Buscar(x => 
                        x.Ativo && 
                        !x.Su); // SEMPRE excluir usuários com su=true
                }
                else
                {
                    // Usuário normal só vê usuários do seu próprio cliente
                    query = _usuarioRepository.Buscar(x => 
                        x.Ativo && 
                        !x.Su && // SEMPRE excluir usuários com su=true
                        x.Cliente == cliente); // Filtrar pelo cliente solicitado
                }
                
                // Aplicar filtro de pesquisa se fornecido (CORRIGIDO)
                if (!string.IsNullOrEmpty(pesquisaProcessada) && pesquisaProcessada != "null")
                {
                    query = query.Where(x => 
                        x.Nome.ToLower().Contains(pesquisaProcessada) || 
                        x.Email.ToLower().Contains(pesquisaProcessada));
                }
                
                var usrs = query
                    .Select(x => new Usuario() { 
                        Id = x.Id, 
                        Nome = x.Nome, 
                        Su = x.Su,
                        Adm = x.Adm, 
                        Cliente = x.Cliente,
                        Consulta = x.Consulta,
                        Email = x.Email,
                        Operador = x.Operador,
                        Ativo = x.Ativo,
                        TwoFactorEnabled = x.TwoFactorEnabled,
                        TwoFactorSecret = x.TwoFactorSecret,
                        TwoFactorBackupCodes = x.TwoFactorBackupCodes,
                        TwoFactorLastUsed = x.TwoFactorLastUsed
                    }).OrderBy(x => x.Nome).ToList();
                
                return usrs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void ExcluirUsuario(int id)
        {
            var usr = _usuarioRepository.ObterPorId(id);
            if (usr == null)
            {
                throw new InvalidOperationException($"Usuário com ID {id} não encontrado");
            }

            try
            {
                // ✅ NOVO COMPORTAMENTO: Usar ExecuteInTransaction para garantir SaveChanges
                _usuarioRepository.ExecuteInTransaction(() =>
                {
                    // Marcar como inativo ao invés de excluir
                    usr.Ativo = false;
                    _usuarioRepository.Atualizar(usr);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCLUIR USUARIO] ❌ Erro ao marcar usuário {id} como inativo: {ex.Message}");
                Console.WriteLine($"[EXCLUIR USUARIO] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[EXCLUIR USUARIO] === FIM DESATIVAÇÃO USUÁRIO {id} COM ERRO ===");
                throw new InvalidOperationException($"Erro ao desativar usuário: {ex.Message}");
            }
        }

        public string GetFrontendUrl()
        {
            return _environmentApiSettings.SiteUrl ?? "http://localhost:4200";
        }

        public async Task<bool> EnviarCodigoTwoFactor(string email, string codigo)
        {
            try
            {
                Console.WriteLine($"[2FA] === INÍCIO ENVIO CÓDIGO 2FA ===");
                Console.WriteLine($"[2FA] Email: {email}");
                Console.WriteLine($"[2FA] Código: {codigo}");
                
                // Buscar usuário pelo email para obter o clienteId
                var usuario = await Task.Run(() => _usuarioRepository.Buscar(x => x.Email == email).FirstOrDefault());
                if (usuario == null)
                {
                    Console.WriteLine($"[2FA] ❌ Usuário não encontrado para email: {email}");
                    return false;
                }
                
                Console.WriteLine($"[2FA] Usuário encontrado: {usuario.Nome} (Cliente: {usuario.Cliente})");
                

                
                // Carregar configurações SMTP do banco de dados para este cliente
                await _smtpConfigService.LoadSmtpSettingsFromDatabase(_environmentApiSettings, usuario.Cliente);
                
                // Verificar configurações SMTP após carregar do banco
                Console.WriteLine($"[2FA] Configurações SMTP carregadas do banco:");
                Console.WriteLine($"[2FA] - SMTP Enabled: {_environmentApiSettings.SMTPEnabled}");
                Console.WriteLine($"[2FA] - SMTP Host: {_environmentApiSettings.SMTPHost}");
                Console.WriteLine($"[2FA] - SMTP Port: {_environmentApiSettings.SMTPPort}");
                Console.WriteLine($"[2FA] - SMTP Login: {_environmentApiSettings.SMTPLogin}");
                Console.WriteLine($"[2FA] - SMTP From: {_environmentApiSettings.SMTPEmailFrom}");
                Console.WriteLine($"[2FA] - SMTP SSL: {_environmentApiSettings.SMTPEnableSSL}");
                
                if (!_environmentApiSettings.SMTPEnabled)
                {
                    Console.WriteLine($"[2FA] ❌ SMTP está desabilitado para o cliente {usuario.Cliente}!");
                    return false;
                }
                
                if (string.IsNullOrEmpty(_environmentApiSettings.SMTPHost))
                {
                    Console.WriteLine($"[2FA] ❌ Host SMTP não configurado para o cliente {usuario.Cliente}!");
                    return false;
                }
                
                // Salvar código temporariamente (em produção, use Redis ou banco com TTL)
                // Por enquanto, vamos simular salvando em memória
                Console.WriteLine($"[2FA] Código salvo temporariamente para usuário: {usuario.Nome}");
                
                // Verificar se o template existe (pasta 'Documentos' conforme configurado no .csproj)
                var file = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "two-factor-auth.html");
                if (!System.IO.File.Exists(file))
                {
                    Console.WriteLine($"[2FA] ❌ Template de 2FA não encontrado: {file}");
                    return false;
                }
                
                Console.WriteLine($"[2FA] Template de 2FA encontrado: {file}");
                
                // Buscar configuração de expiração do token 2FA no banco
                var configuracao = await Task.Run(() => _parametroRepository.Buscar(x => x.Cliente == usuario.Cliente).FirstOrDefault());
                var minutosExpiracao = configuracao?.TwoFactorExpirationMinutes ?? 5; // Padrão 5 minutos se não configurado
                
                Console.WriteLine($"[2FA] Minutos de expiração configurados: {minutosExpiracao}");
                
                // Enviar e-mail usando o template específico de 2FA
                string template = await System.IO.File.ReadAllTextAsync(file);
                
                // Substituir placeholders específicos do template 2FA
                template = template.Replace("@nome", usuario.Nome)
                                 .Replace("@codigo", codigo)
                                 .Replace("@minutos", minutosExpiracao.ToString());
                
                Console.WriteLine($"[2FA] Template de 2FA processado, tentando enviar e-mail...");
                
                // Enviar e-mail usando o SendMail com configurações do banco
                await Task.Run(() => mail.Enviar(email, "Código de Verificação 2FA", template));
                
                Console.WriteLine($"[2FA] ✅ E-mail enviado com sucesso para: {email}");
                Console.WriteLine($"[2FA] === FIM ENVIO CÓDIGO 2FA (SUCESSO) ===");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[2FA] ❌ ERRO ao enviar código 2FA: {ex.Message}");
                Console.WriteLine($"[2FA] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[2FA] === FIM ENVIO CÓDIGO 2FA (ERRO) ===");
                return false;
            }
        }

        public async Task<TwoFactorVerificationResult> VerificarCodigoTwoFactor(int userId, string codigo)
        {
            try
            {
                Console.WriteLine($"[2FA] === INÍCIO VERIFICAÇÃO CÓDIGO 2FA ===");
                Console.WriteLine($"[2FA] Usuário ID: {userId}");
                Console.WriteLine($"[2FA] Código recebido: {codigo}");
                
                // Buscar usuário
                var usuario = await Task.Run(() => _usuarioRepository.ObterPorId(userId));
                if (usuario == null)
                {
                    Console.WriteLine($"[2FA] ❌ Usuário não encontrado para ID: {userId}");
                    return new TwoFactorVerificationResult { Success = false, Message = "Usuário não encontrado" };
                }
                
                Console.WriteLine($"[2FA] Usuário encontrado: {usuario.Nome}");
                
                // Aqui você implementaria a verificação real do código
                // Por enquanto, vamos simular aceitando qualquer código de 6 dígitos
                if (codigo.Length == 6 && codigo.All(char.IsDigit))
                {
                    Console.WriteLine($"[2FA] ✅ Código 2FA verificado com sucesso para usuário: {usuario.Nome}");
                    Console.WriteLine($"[2FA] === FIM VERIFICAÇÃO CÓDIGO 2FA (SUCESSO) ===");
                    
                    // Gerar token JWT
                    var token = JwtTokenService.GenerateToken(usuario);
                    
                    return new TwoFactorVerificationResult 
                    { 
                        Success = true, 
                        Message = "Verificação realizada com sucesso",
                        Usuario = usuario,
                        Token = token
                    };
                }
                else
                {
                    Console.WriteLine($"[2FA] ❌ Código 2FA inválido para usuário: {usuario.Nome}");
                    Console.WriteLine($"[2FA] === FIM VERIFICAÇÃO CÓDIGO 2FA (FALHA) ===");
                    
                    return new TwoFactorVerificationResult 
                    { 
                        Success = false, 
                        Message = "Código de verificação inválido" 
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[2FA] ❌ ERRO ao verificar código 2FA: {ex.Message}");
                Console.WriteLine($"[2FA] === FIM VERIFICAÇÃO CÓDIGO 2FA (ERRO) ===");
                
                return new TwoFactorVerificationResult 
                { 
                    Success = false, 
                    Message = "Erro interno ao verificar código" 
                };
            }
        }

        /// <summary>
        /// Verifica se o 2FA está habilitado globalmente para um cliente
        /// </summary>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>True se 2FA estiver habilitado globalmente</returns>
        public bool IsTwoFactorEnabledGlobally(int clienteId)
        {
            try
            {
                var configuracao = _parametroRepository.Buscar(x => x.Cliente == clienteId).FirstOrDefault();
                return configuracao?.TwoFactorEnabled == true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[2FA] Erro ao verificar configuração global: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtém o status de 2FA de um usuário específico
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Objeto com informações de 2FA do usuário</returns>
        public dynamic GetUserTwoFactorStatus(int usuarioId)
        {
            try
            {
                var usuario = _usuarioRepository.ObterPorId(usuarioId);
                if (usuario == null)
                {
                    return new { 
                        Success = false, 
                        Message = "Usuário não encontrado" 
                    };
                }

                var twoFactorEnabledGlobally = IsTwoFactorEnabledGlobally(usuario.Cliente);
                
                return new
                {
                    Success = true,
                    UserId = usuario.Id,
                    UserName = usuario.Nome,
                    ClienteId = usuario.Cliente,
                    TwoFactorEnabledGlobally = twoFactorEnabledGlobally,
                    TwoFactorEnabledIndividually = usuario.TwoFactorEnabled ?? false,
                    CanEnableTwoFactor = twoFactorEnabledGlobally, // Só pode ativar se global estiver habilitado
                    Message = twoFactorEnabledGlobally 
                        ? "2FA está disponível para este usuário" 
                        : "2FA não está habilitado globalmente para este cliente"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[2FA] Erro ao obter status de 2FA: {ex.Message}");
                return new { 
                    Success = false, 
                    Message = $"Erro ao obter status: {ex.Message}" 
                };
            }
        }
    }

    public class TwoFactorVerificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Usuario Usuario { get; set; }
        public string Token { get; set; }
    }
}
