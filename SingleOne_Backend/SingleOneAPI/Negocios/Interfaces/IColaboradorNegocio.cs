using SingleOne.Models;
using SingleOne.Util;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface IColaboradorNegocio
    {
        PagedResult<ColaboradoresVM> ListarColaboradores(string pesquisa, int cliente, int pagina);
        List<Colaboradore> ListarColaboradores(string pesquisa, int cliente);
        List<Colaboradore> ListarColaboradoresAtivos(string pesquisa, int cliente);
        ColaboradorCompletoDTO ObterColaboradorPorID(int id);
        string SalvarColaborador(Colaboradore colaborador);
        void ExcluirColaborador(int id);
        byte[] TermoCompromisso(int cliente, int colaborador, int usuarioLogado, bool byod = false);
        Task<string> TermoPorEmail(int cliente, int colaborador, int usuarioLogado, bool byod);
        List<Vwnadaconstum> NadaConsta(int colaborador, int cliente);
        List<string> ListarCargos(int cliente, string pesquisa);
        void SalvarCargoDescarte(Descartecargo cargo);
        void ExcluirCargoDescarte(int idCargo);
        List<Descartecargo> ListarCargosDeDescarte(int cliente);
        void ExportarTermosEmPDF(int cliente);
        void TermoCompromissoExport(int cliente, int colaborador, int usuarioLogado, bool assinado);
        byte[] TermoNadaConsta(int colaborador, int cliente, int usuarioLogado);
        List<Termoscolaboradoresvm> ColaboradoresComTermoPorAssinar(string pesquisa, int cliente, string filtro);
        void RegistrarLocalizacaoAssinatura(LocalizacaoAssinaturaDTO dados);
        ColaboradorEstatisticasDTO ObterEstatisticas(int cliente);
        
        // Cargos de Confiança
        List<string> ListarCargosUnicos(int cliente);
        List<CargoConfianca> ListarCargosConfianca(int cliente);
        CargoConfianca SalvarCargoConfianca(CargoConfianca cargo);
        CargoConfianca AtualizarCargoConfianca(int id, CargoConfianca cargo);
        void ExcluirCargoConfianca(int id);
        CargoConfianca VerificarCargoConfianca(string cargo, int cliente);
    }
}
