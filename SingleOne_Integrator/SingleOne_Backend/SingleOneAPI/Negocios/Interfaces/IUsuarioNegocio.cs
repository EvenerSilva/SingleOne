using SingleOne.Models;
using SingleOne.Negocios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IUsuarioNegocio
    {
        string Salvar(Usuario usr);
        Usuario Logar(Usuario usr);
        Task RecuperarPalavraChave(string email);
        void RecuperarSenha(Usuario usuario);
        List<Usuario> ListarUsuarios();
        Usuario BuscarPorId(int id);
        List<Usuario> ListarUsuarios(string pesquisa, int cliente, bool usuarioLogadoEhSuper = false);
        void ExcluirUsuario(int id);
        string GetFrontendUrl();
        
        // Métodos de 2FA
        bool IsTwoFactorEnabledGlobally(int clienteId);
        dynamic GetUserTwoFactorStatus(int usuarioId);
        Task<bool> EnviarCodigoTwoFactor(string email, string codigo);
        Task<TwoFactorVerificationResult> VerificarCodigoTwoFactor(int userId, string codigo);
    }

}
