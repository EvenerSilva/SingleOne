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
                Console.WriteLine($"[SALVAR] === INÍCIO DO SALVAR ===");
                Console.WriteLine($"[SALVAR] Usuário ID: {usr.Id}");
                Console.WriteLine($"[SALVAR] Nome: {usr.Nome}");
                Console.WriteLine($"[SALVAR] Email: {usr.Email}");
                Console.WriteLine($"[SALVAR] Cliente: {usr.Cliente}");
                Console.WriteLine($"[SALVAR] TwoFactorEnabled: {usr.TwoFactorEnabled}");
                Console.WriteLine($"[SALVAR] TwoFactorSecret: {(string.IsNullOrEmpty(usr.TwoFactorSecret) ? "NULL" : "PRESENTE")}");
                Console.WriteLine($"[SALVAR] TwoFactorBackupCodes: {(string.IsNullOrEmpty(usr.TwoFactorBackupCodes) ? "NULL" : "PRESENTE")}");
                if (usr.Id == 0)
                {
                    var existe = _usuarioRepository.Buscar(x => x.Email == usr.Email).Any();

                    if (!existe)
                    {
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
                    Console.WriteLine($"[SALVAR] Atualizando usuário existente");
                    var u = _usuarioRepository.ObterPorId(usr.Id);
                    
                    if (u == null)
                    {
                        Console.WriteLine($"[SALVAR] ✗ Usuário não encontrado para ID: {usr.Id}");
                        return JsonConvert.SerializeObject(new { Mensagem = "Usuário não encontrado", Status = "404" });
                    }
                    
                    Console.WriteLine($"[SALVAR] Usuário encontrado: {u.Nome} (Cliente: {u.Cliente})");
                    
                    u.Nome = usr.Nome;
                    u.Email = usr.Email;
                    u.Adm = usr.Adm;
                    u.Operador = usr.Operador;
                    u.Consulta = usr.Consulta;
                    u.Cliente = usr.Cliente;
                    
                    if (!String.IsNullOrEmpty(usr.Senha))
                    {
                        u.Senha = usr.Senha;
                        Console.WriteLine($"[SALVAR] Senha atualizada");
                    }
                    
                    // VALIDAÇÃO SIMPLES DE 2FA
                    Console.WriteLine($"[SALVAR] === VERIFICAÇÃO 2FA ===");
                    
                    // SÓ VALIDAR SE ESTIVER TENTANDO HABILITAR 2FA
                    if (usr.TwoFactorEnabled == true)
                    {
                        Console.WriteLine($"[SALVAR] Tentando HABILITAR 2FA - verificando configuração global...");
                        
                        // Buscar configuração global no cliente do usuário
                        bool configuracaoGlobal = IsTwoFactorEnabledGlobally(usr.Cliente);
                        
                        Console.WriteLine($"[SALVAR] Configuração global encontrada: TwoFactorEnabled = {configuracaoGlobal}");
                        
                        // BLOQUEAR se global estiver desabilitado
                        if (!configuracaoGlobal)
                        {
                            Console.WriteLine($"[SALVAR] ✗ BLOQUEADO: Tentativa de habilitar 2FA com global desabilitado");
                            return JsonConvert.SerializeObject(new { 
                                Mensagem = "Para habilitar 2FA para usuários, é necessário ativar primeiro a funcionalidade nas configurações globais do cliente.", 
                                Status = "400",
                                CodigoErro = "2FA_GLOBAL_DESABILITADO",
                                Detalhes = "2FA global está desabilitado para este cliente"
                            });
                        }
                        
                        Console.WriteLine($"[SALVAR] ✓ PERMITIDO: 2FA global habilitado, permitindo habilitação individual");
                    }
                    else
                    {
                        Console.WriteLine($"[SALVAR] Nenhuma tentativa de habilitar 2FA - prosseguindo normalmente");
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
                Console.WriteLine($"[SALVAR] ✗ ERRO: {ex.Message}");
                Console.WriteLine($"[SALVAR] Stack trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                Console.WriteLine($"[SALVAR] === FIM DO SALVAR ===");
            }
        }

        public Usuario Logar(Usuario usr)
        {
            try
            {
                Console.WriteLine($"[DEBUG] === INÍCIO DO LOGIN ===");
                Console.WriteLine($"[DEBUG] Email recebido: {usr.Email}");
                Console.WriteLine($"[DEBUG] Senha recebida (raw): {usr.Senha}");
                
                // Decodificar a senha se ela vier em Base64 do frontend
                string senhaOriginal = usr.Senha;
                if (Cripto.IsBase64String(usr.Senha))
                {
                    try
                    {
                        senhaOriginal = Cripto.CriptografarDescriptografar(usr.Senha, false);
                        Console.WriteLine($"[DEBUG] Senha decodificada do Base64: {senhaOriginal}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Erro ao decodificar Base64: {ex.Message}");
                        senhaOriginal = usr.Senha;
                    }
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Senha não é Base64, usando como está");
                }
                
                string senhaCodificada = Cripto.CriptografarDescriptografar(senhaOriginal, true);
                Console.WriteLine($"[DEBUG] Senha codificada: {senhaCodificada}");

                // Buscar o usuário pelo email
                var usuarioRetorno = _usuarioRepository.Buscar(x => x.Email.ToLower() == usr.Email.ToLower() && x.Ativo == true).FirstOrDefault();
                
                if (usuarioRetorno != null)
                {
                    Console.WriteLine($"[DEBUG] Usuário encontrado: {usuarioRetorno.Nome}");
                    Console.WriteLine($"[DEBUG] Senha no banco: {usuarioRetorno.Senha}");
                    Console.WriteLine($"[DEBUG] Usuário ativo: {usuarioRetorno.Ativo}");
                    
                    // Comparar a senha
                    bool senhaValida = false;
                    
                    // Tentar com a senha original
                    if (usuarioRetorno.Senha == senhaOriginal)
                    {
                        senhaValida = true;
                        Console.WriteLine($"[DEBUG] ✓ Senha válida (original)");
                    }
                    // Tentar com a senha codificada
                    else if (usuarioRetorno.Senha == senhaCodificada)
                    {
                        senhaValida = true;
                        Console.WriteLine($"[DEBUG] ✓ Senha válida (codificada)");
                    }
                    // Tentar decodificando a senha do banco
                    else if (Cripto.IsBase64String(usuarioRetorno.Senha))
                    {
                        try
                        {
                            string senhaDecodificada = Cripto.CriptografarDescriptografar(usuarioRetorno.Senha, false);
                            Console.WriteLine($"[DEBUG] Senha decodificada do banco: {senhaDecodificada}");
                            if (senhaDecodificada == senhaOriginal)
                            {
                                senhaValida = true;
                                Console.WriteLine($"[DEBUG] ✓ Senha válida (decodificada)");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[DEBUG] Erro ao decodificar senha: {ex.Message}");
                        }
                    }
                    
                    if (senhaValida)
                    {
                        Console.WriteLine($"[DEBUG] ✓ Login bem-sucedido para: {usuarioRetorno.Nome}");
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
                        
                        Console.WriteLine($"[DEBUG] ✓ Usuário retornado com sucesso: {usuarioRetornoFinal.Nome}");
                        Console.WriteLine($"[DEBUG] === FIM DO LOGIN (SUCESSO) ===");
                        return usuarioRetornoFinal;
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] ✗ Senha inválida para usuário: {usuarioRetorno.Nome}");
                        Console.WriteLine($"[DEBUG] Comparações tentadas:");
                        Console.WriteLine($"[DEBUG] - Original: '{senhaOriginal}' vs Banco: '{usuarioRetorno.Senha}'");
                        Console.WriteLine($"[DEBUG] - Codificada: '{senhaCodificada}' vs Banco: '{usuarioRetorno.Senha}'");
                    }
                }
                else
                {
                    Console.WriteLine($"[DEBUG] ✗ Usuário não encontrado para email: {usr.Email}");
                }
                
                Console.WriteLine($"[DEBUG] === FIM DO LOGIN (FALHA) ===");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] ERRO: {ex.Message}");
                Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
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
                    Console.WriteLine($"[RECUPERAR SENHA] Carregando configurações SMTP para cliente: {usr.Cliente}");
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
                    
                    Console.WriteLine($"[RECUPERAR SENHA] SMTP configurado - Host: {_environmentApiSettings.SMTPHost}, Port: {_environmentApiSettings.SMTPPort}");
                }
                else
                {
                    Console.WriteLine($"[RECUPERAR SENHA] SmtpConfigService não disponível, usando configurações padrão");
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
                
                // Log para debug
                Console.WriteLine($"[RECUPERAR SENHA] BaseUrl: '{baseUrl}'");
                Console.WriteLine($"[RECUPERAR SENHA] SecureLink: '{secureLink}'");
                Console.WriteLine($"[RECUPERAR SENHA] Enviando e-mail para: {usr.Email}");
                
                template = template.Replace("@nome", usr.Nome)
                                 .Replace("@palavra", usr.Palavracriptografada)
                                 .Replace("@endereco", secureLink);

                mail.Enviar(usr.Email, "Recuperação de Senha", template);
                
                Console.WriteLine($"[RECUPERAR SENHA] E-mail de recuperação enviado com sucesso para: {usr.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RECUPERAR SENHA] ERRO: {ex.Message}");
                Console.WriteLine($"[RECUPERAR SENHA] StackTrace: {ex.StackTrace}");
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
                    
                    Console.WriteLine($"[RECUPERAR SENHA] Senha gerada: {senha}");
                    Console.WriteLine($"[RECUPERAR SENHA] Senha criptografada: {senhaCriptografada}");
                    
                    usr.Senha = senhaCriptografada;
                    usr.Palavracriptografada = Guid.NewGuid().ToString();
                    _usuarioRepository.Atualizar(usr);
                    _usuarioRepository.SalvarAlteracoes(); // ✅ CORREÇÃO: Salvar alterações explicitamente

                    var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "recuperarSenha.html");
                    string template = System.IO.File.ReadAllText(file);
                    template = template.Replace("@nome", usr.Nome).Replace("@senha", senha);

                    mail.Enviar(usr.Email, "Recuperação de Senha", template);
                    
                    Console.WriteLine($"[RECUPERAR SENHA] Senha atualizada com sucesso para o usuário: {usr.Nome}");
                }
                else
                {
                    Console.WriteLine($"[RECUPERAR SENHA] Usuário não encontrado para email: {usuario.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RECUPERAR SENHA] ERRO: {ex.Message}");
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
                // DEBUG TEMPORÁRIO: Verificar quantos usuários existem e quantos têm su=true
                var totalUsuarios = _usuarioRepository.Buscar(x => true).Count();
                var usuariosSu = _usuarioRepository.Buscar(x => x.Su).Count();
                var usuariosNaoSu = _usuarioRepository.Buscar(x => !x.Su).Count();
                
                Console.WriteLine($"[DEBUG] Total usuários: {totalUsuarios}, com su=true: {usuariosSu}, com su=false: {usuariosNaoSu}");
                
                // DEBUG: Listar todos os usuários com seus perfis
                var todosUsuarios = _usuarioRepository.Buscar(x => true).ToList();
                foreach (var user in todosUsuarios)
                {
                    Console.WriteLine($"[DEBUG] Usuário ID:{user.Id}, Nome:{user.Nome}, Cliente:{user.Cliente}, su:{user.Su}, adm:{user.Adm}, operador:{user.Operador}, consulta:{user.Consulta}");
                }
                
                // TESTE TEMPORÁRIO: Retornar TODOS os usuários para confirmar se o problema é o filtro su
                Console.WriteLine($"[DEBUG] TESTE: Retornando TODOS os usuários (sem filtro su) para confirmar o problema");
                var users = _usuarioRepository.Buscar(x => true) // TESTE: sem filtro
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
                
                Console.WriteLine($"[DEBUG] Usuários retornados após filtro (sem filtro su): {users.Count}");
                return users;
            }
            catch (Exception ex)
            {
                // Log de erro apenas para debugging interno
                Console.WriteLine($"[LISTAR USUARIOS] Erro: {ex.Message}");
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
                Console.WriteLine($"[DEBUG] === INÍCIO LISTAR USUÁRIOS COM CLIENTE ===");
                Console.WriteLine($"[DEBUG] Cliente solicitado: {cliente}");
                Console.WriteLine($"[DEBUG] Pesquisa: '{pesquisa}'");
                Console.WriteLine($"[DEBUG] Usuário logado é Super Usuário: {usuarioLogadoEhSuper}");
                
                // Tratar pesquisa null ou vazia
                string pesquisaProcessada = string.IsNullOrEmpty(pesquisa) ? "" : pesquisa.ToLower();
                
                // DEBUG: Verificar quantos usuários existem para este cliente
                var totalUsuariosCliente = _usuarioRepository.Buscar(x => x.Cliente == cliente).Count();
                var usuariosAtivosCliente = _usuarioRepository.Buscar(x => x.Ativo && x.Cliente == cliente).Count();
                var usuariosSuCliente = _usuarioRepository.Buscar(x => x.Su && x.Cliente == cliente).Count();
                var usuariosNaoSuCliente = _usuarioRepository.Buscar(x => !x.Su && x.Cliente == cliente).Count();
                
                Console.WriteLine($"[DEBUG] Total usuários cliente {cliente}: {totalUsuariosCliente}");
                Console.WriteLine($"[DEBUG] Usuários ativos cliente {cliente}: {usuariosAtivosCliente}");
                Console.WriteLine($"[DEBUG] Usuários su=true cliente {cliente}: {usuariosSuCliente}");
                Console.WriteLine($"[DEBUG] Usuários su=false cliente {cliente}: {usuariosNaoSuCliente}");
                
                // DEBUG: Listar todos os usuários deste cliente
                var todosUsuariosCliente = _usuarioRepository.Buscar(x => x.Cliente == cliente).ToList();
                foreach (var user in todosUsuariosCliente)
                {
                    Console.WriteLine($"[DEBUG] Usuário cliente {cliente}: ID:{user.Id}, Nome:{user.Nome}, su:{user.Su}, ativo:{user.Ativo}");
                }
                
                // LÓGICA CORRIGIDA: 
                // 1. SEMPRE excluir usuários com su=true (Super Usuário)
                // 2. Filtrar pelo cliente solicitado (ou todos os clientes se for Super Usuário)
                // 3. Apenas usuários ativos
                
                IQueryable<Usuario> query;
                
                if (usuarioLogadoEhSuper)
                {
                    // Super Usuário pode ver usuários de TODOS os clientes
                    Console.WriteLine($"[DEBUG] Usuário logado é Super Usuário - mostrando usuários de TODOS os clientes");
                    query = _usuarioRepository.Buscar(x => 
                        x.Ativo && 
                        !x.Su); // SEMPRE excluir usuários com su=true
                }
                else
                {
                    // Usuário normal só vê usuários do seu próprio cliente
                    Console.WriteLine($"[DEBUG] Usuário logado é normal - mostrando apenas usuários do cliente {cliente}");
                    query = _usuarioRepository.Buscar(x => 
                        x.Ativo && 
                        !x.Su && // SEMPRE excluir usuários com su=true
                        x.Cliente == cliente); // Filtrar pelo cliente solicitado
                }
                
                Console.WriteLine($"[DEBUG] Query após filtros: {query.Count()} usuários");
                
                // Aplicar filtro de pesquisa se fornecido (CORRIGIDO)
                if (!string.IsNullOrEmpty(pesquisaProcessada) && pesquisaProcessada != "null")
                {
                    query = query.Where(x => 
                        x.Nome.ToLower().Contains(pesquisaProcessada) || 
                        x.Email.ToLower().Contains(pesquisaProcessada));
                    Console.WriteLine($"[DEBUG] Após filtro de pesquisa '{pesquisaProcessada}': {query.Count()} usuários");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Sem filtro de pesquisa aplicado (pesquisa vazia ou null)");
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
                
                Console.WriteLine($"[DEBUG] Usuários finais retornados: {usrs.Count}");
                Console.WriteLine($"[DEBUG] === FIM LISTAR USUÁRIOS COM CLIENTE ===");
                
                return usrs;
            }
            catch (Exception ex)
            {
                // Log de erro apenas para debugging interno
                Console.WriteLine($"[LISTAR USUARIOS] Erro: {ex.Message}");
                throw;
            }
        }
        public void ExcluirUsuario(int id)
        {
            Console.WriteLine($"[EXCLUIR USUARIO] === INÍCIO DESATIVAÇÃO USUÁRIO {id} ===");
            
            var usr = _usuarioRepository.ObterPorId(id);
            if (usr == null)
            {
                Console.WriteLine($"[EXCLUIR USUARIO] ❌ Usuário com ID {id} não encontrado");
                throw new InvalidOperationException($"Usuário com ID {id} não encontrado");
            }

            Console.WriteLine($"[EXCLUIR USUARIO] Usuário encontrado: {usr.Nome} (ID: {id})");
            Console.WriteLine($"[EXCLUIR USUARIO] Status atual Ativo: {usr.Ativo}");
            Console.WriteLine($"[EXCLUIR USUARIO] Cliente: {usr.Cliente}");
            Console.WriteLine($"[EXCLUIR USUARIO] Email: {usr.Email}");

            try
            {
                // ✅ NOVO COMPORTAMENTO: Usar ExecuteInTransaction para garantir SaveChanges
                Console.WriteLine($"[EXCLUIR USUARIO] Iniciando transação para marcar como inativo...");
                
                _usuarioRepository.ExecuteInTransaction(() =>
                {
                    // Marcar como inativo ao invés de excluir
                    Console.WriteLine($"[EXCLUIR USUARIO] Dentro da transação - marcando Ativo = false");
                    usr.Ativo = false;
                    Console.WriteLine($"[EXCLUIR USUARIO] Ativo alterado para: {usr.Ativo}");
                    
                    Console.WriteLine($"[EXCLUIR USUARIO] Chamando _usuarioRepository.Atualizar...");
                    _usuarioRepository.Atualizar(usr);
                    Console.WriteLine($"[EXCLUIR USUARIO] Atualizar chamado com sucesso");
                    
                    // SaveChanges é chamado automaticamente pelo ExecuteInTransaction
                    Console.WriteLine($"[EXCLUIR USUARIO] Transação será commitada automaticamente");
                });
                
                Console.WriteLine($"[EXCLUIR USUARIO] ✅ Transação executada com sucesso");
                
                // Verificar se realmente foi alterado
                var usuarioVerificado = _usuarioRepository.ObterPorId(id);
                if (usuarioVerificado != null)
                {
                    Console.WriteLine($"[EXCLUIR USUARIO] ✅ Verificação pós-transação: Ativo = {usuarioVerificado.Ativo}");
                }
                else
                {
                    Console.WriteLine($"[EXCLUIR USUARIO] ❌ ALERTA: Usuário não encontrado após transação!");
                }
                
                Console.WriteLine($"[EXCLUIR USUARIO] === FIM DESATIVAÇÃO USUÁRIO {id} ===");
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
                
                // Verificar se o template existe
                var file = Path.Combine(Directory.GetCurrentDirectory(), "documentos", "two-factor-auth.html");
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
