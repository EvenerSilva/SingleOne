using SingleOne.Models;
using SingleOne.Util;
using SingleOneAPI.Models;
using System.Collections.Generic;

namespace SingleOneAPI.Negocios.Interfaces
{
    public interface ITelefoniaNegocio
    {
        List<Telefoniaoperadora> ListarOperadoras();
        Telefoniaoperadora SalvarOperadora(Telefoniaoperadora to);
        void ExcluirOperadora(int id);

        List<Telefoniacontrato> ListarContratos(string pesquisa, int operadora, int cliente);
        void SalvarContrato(Telefoniacontrato tc);
        void ExcluirContrato(int id);

        List<PlanosVM> ListarPlanos(string pesquisa, int contrato, int cliente);
        void SalvarPlano(PlanosVM tp);
        void ExcluirPlano(int id);

        List<Telefonialinha> ListarLinhas(string pesquisa, int cliente, int pagina);
        List<Telefonialinha> LinhasDisponiveisParaRequisicao(string pesquisa, int cliente);
        List<dynamic> ListarLinhasParaExportacao(string pesquisa, int cliente);
        Telefonialinha BuscarLinhaPorId(int id);
        void SalvarLinha(Telefonialinha tl);
        void ExcluirLinha(int id);

        // 🆕 NOVOS MÉTODOS PARA FILTROS ESPECÍFICOS
        PagedResult<Telefonialinha> ListarLinhasPorConta(int contaId, int cliente, int pagina);
        PagedResult<Telefonialinha> ListarLinhasPorPlano(int planoId, int cliente, int pagina);
        PagedResult<Telefonialinha> ListarLinhasPorTipo(int contaId, string tipo, int cliente, int pagina);
        PagedResult<Telefonialinha> ListarLinhasPorPlanoETipo(int planoId, string tipo, int cliente, int pagina);

        List<Vwtelefonium> ExportarParaExcel(int cliente);

        // Métodos de contagem para dashboard
        int ContarOperadoras();
        int ContarContratos();
        int ContarPlanos();
        int ContarLinhas();
    }

}
