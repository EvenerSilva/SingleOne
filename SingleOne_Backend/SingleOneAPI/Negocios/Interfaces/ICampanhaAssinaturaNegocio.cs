using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using System.Collections.Generic;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface ICampanhaAssinaturaNegocio
    {
        // CRUD Básico
        CampanhaAssinatura CriarCampanha(CampanhaAssinatura campanha, List<int> colaboradoresIds);
        CampanhaAssinatura ObterCampanhaPorId(int id);
        List<CampanhaAssinatura> ListarCampanhasPorCliente(int clienteId, char? status = null);
        void AtualizarCampanha(CampanhaAssinatura campanha);
        void InativarCampanha(int id);
        void ConcluirCampanha(int id);
        
        // Gerenciamento de Colaboradores
        void AdicionarColaboradoresNaCampanha(int campanhaId, List<int> colaboradoresIds);
        void RemoverColaboradorDaCampanha(int campanhaId, int colaboradorId);
        List<CampanhaColaborador> ObterColaboradoresDaCampanha(int campanhaId, char? statusAssinatura = null);
        
        // Envio de Termos
        bool EnviarTermoParaColaborador(int campanhaId, int colaboradorId, int usuarioEnvioId, string ip, string localizacao);
        bool EnviarTermosEmMassa(int campanhaId, List<int> colaboradoresIds, int usuarioEnvioId, string ip, string localizacao);
        
        // Atualização de Status
        void MarcarComoAssinado(int campanhaId, int colaboradorId);
        void AtualizarEstatisticasCampanha(int campanhaId);
        
        // Relatórios
        CampanhaResumoDTO ObterResumoCampanha(int campanhaId);
        List<CampanhaResumoDTO> ObterResumoCampanhasPorCliente(int clienteId);
        RelatorioAderenciaDTO ObterRelatorioAderencia(int campanhaId);
        List<ColaboradorPendenteDTO> ObterColaboradoresPendentes(int campanhaId);
    }
}

