using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SingleOne.Models;
using SingleOne.Negocios;
using SingleOne.Util;
using SingleOneAPI.Models;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TelefoniaController : ControllerBase
    {
        private ITelefoniaNegocio _negocio;
        public TelefoniaController(ITelefoniaNegocio negocio)
        {
            _negocio = negocio;
        }

        /***************************************************************************************************/
        /******************************************** OPERADORAS *******************************************/
        /***************************************************************************************************/
        [HttpGet("[action]", Name ="ListarOperadoras")]
        public List<Telefoniaoperadora> ListarOperadoras()
        {
            return _negocio.ListarOperadoras();
        }
        [HttpPost("[action]", Name ="SalvarOperadora")]
        public ActionResult<Telefoniaoperadora> SalvarOperadora(Telefoniaoperadora to)
        {
            try
            {
                var operadoraSalva = _negocio.SalvarOperadora(to);
                return Ok(operadoraSalva);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirOperadora")]
        public void ExcluirOperadora(int id)
        {
            _negocio.ExcluirOperadora(id);
        }


        /***************************************************************************************************/
        /******************************************** CONTRATOS ********************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{operadora}/{cliente}", Name ="ListarContratos")]
        public List<Telefoniacontrato> ListarContratos(string pesquisa, int operadora, int cliente)
        {
            return _negocio.ListarContratos(pesquisa, operadora, cliente);
        }
        [HttpPost("[action]", Name ="SalvarContrato")]
        public void SalvarContrato(Telefoniacontrato tc)
        {
            _negocio.SalvarContrato(tc);
        }
        [HttpDelete("[action]/{id}", Name = "ExcluirContrato")]
        public void ExcluirContrato(int id)
        {
            _negocio.ExcluirContrato(id);
        }


        /***************************************************************************************************/
        /******************************************** PLANOS ***********************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{contrato}/{cliente}", Name = "ListarPlanos")]
        public List<PlanosVM> ListarPlanos(string pesquisa, int contrato, int cliente)
        {
            return _negocio.ListarPlanos(pesquisa, contrato, cliente);
        }
        
        // 🆕 ENDPOINT SIMPLES PARA LISTAR TODOS OS PLANOS
        [HttpGet("[action]", Name = "ListarTodosPlanos")]
        public List<PlanosVM> ListarTodosPlanos()
        {
            return _negocio.ListarPlanos("", 0, 1); // Lista todos os planos
        }
        [HttpPost("[action]", Name = "SalvarPlano")]
        public void SalvarPlano(PlanosVM tp)
        {
            _negocio.SalvarPlano(tp);
        }
        [HttpDelete("[action]/{id}", Name = "ExcluirPlano")]
        public void ExcluirPlano(int id)
        {
            _negocio.ExcluirPlano(id);
        }


        /***************************************************************************************************/
        /******************************************** LINHAS ***********************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}/{pagina}", Name = "ListarLinhas")]
        public List<Telefonialinha> ListarLinhas(string pesquisa, int cliente, int pagina)
        {
            return _negocio.ListarLinhas(pesquisa, cliente, pagina);
        }

        // 🆕 NOVOS ENDPOINTS PARA FILTROS ESPECÍFICOS
        [HttpGet("[action]/{contaId}/{cliente}/{pagina}", Name = "ListarLinhasPorConta")]
        public PagedResult<Telefonialinha> ListarLinhasPorConta(int contaId, int cliente, int pagina)
        {
            return _negocio.ListarLinhasPorConta(contaId, cliente, pagina);
        }

        [HttpGet("[action]/{planoId}/{cliente}/{pagina}", Name = "ListarLinhasPorPlano")]
        public PagedResult<Telefonialinha> ListarLinhasPorPlano(int planoId, int cliente, int pagina)
        {
            return _negocio.ListarLinhasPorPlano(planoId, cliente, pagina);
        }

        [HttpGet("[action]/{contaId}/{tipo}/{cliente}/{pagina}", Name = "ListarLinhasPorTipo")]
        public PagedResult<Telefonialinha> ListarLinhasPorTipo(int contaId, string tipo, int cliente, int pagina)
        {
            return _negocio.ListarLinhasPorTipo(contaId, tipo, cliente, pagina);
        }

        [HttpGet("[action]/{planoId}/{tipo}/{cliente}/{pagina}", Name = "ListarLinhasPorPlanoETipo")]
        public PagedResult<Telefonialinha> ListarLinhasPorPlanoETipo(int planoId, string tipo, int cliente, int pagina)
        {
            return _negocio.ListarLinhasPorPlanoETipo(planoId, tipo, cliente, pagina);
        }
        //[HttpGet("[action]/{pesquisa}/{cliente}", Name = "ListarLinhas")]
        //public List<Telefonialinha> ListarLinhas(string pesquisa, int cliente)
        //{
        //    return negocio.ListarLinhas(pesquisa, cliente);
        //}
        [HttpGet("[action]/{pesquisa}/{cliente}", Name = "LinhasDisponiveisParaRequisicao")]
        public List<Telefonialinha> LinhasDisponiveisParaRequisicao(string pesquisa, int cliente)
        {
            return _negocio.LinhasDisponiveisParaRequisicao(pesquisa, cliente);
        }

        // 🆕 NOVO ENDPOINT PARA EXPORTAÇÃO COM DADOS COMPLETOS
        [HttpGet("[action]/{pesquisa}/{cliente}", Name = "ListarLinhasParaExportacao")]
        public List<dynamic> ListarLinhasParaExportacao(string pesquisa, int cliente)
        {
            return _negocio.ListarLinhasParaExportacao(pesquisa, cliente);
        }

        [HttpPost("[action]", Name = "SalvarLinha")]
        public ActionResult SalvarLinha(Telefonialinha tl)
        {
            try
            {
                _negocio.SalvarLinha(tl);
                return Ok();
            }
            catch (EntidadeJaExisteEx ex)
            {
                return StatusCode(409, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpGet("[action]/{id}", Name = "BuscarLinhaPorId")]
        public Telefonialinha BuscarLinhaPorId(int id)
        {
            return _negocio.BuscarLinhaPorId(id);
        }
        
        [HttpDelete("[action]/{id}", Name = "ExcluirLinha")]
        public void ExcluirLinha(int id)
        {
            _negocio.ExcluirLinha(id);
        }


        [HttpGet("[action]/{cliente}", Name = "ExportarLinhasParaExcel")]
        public byte[]  ExportarLinhasParaExcel(int cliente)
        {
            var dados = _negocio.ExportarParaExcel(cliente);

            using(var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Telefonia");
                var currentRow = 1;
                ws.Cell(currentRow, 1).Value = "Operadora";
                ws.Cell(currentRow, 2).Value = "Contrato";
                ws.Cell(currentRow, 3).Value = "Plano";
                ws.Cell(currentRow, 4).Value = "Valor";
                ws.Cell(currentRow, 5).Value = "Número";
                ws.Cell(currentRow, 6).Value = "ICCID";
                ws.Cell(currentRow, 7).Value = "Em Uso";

                foreach(var row in dados)
                {
                    currentRow++;
                    ws.Cell(currentRow, 1).Value = row.Operadora;
                    ws.Cell(currentRow, 2).Value = row.Contrato;
                    ws.Cell(currentRow, 3).Value = row.Plano;
                    ws.Cell(currentRow, 4).Value = row.Valor.Value.ToString("C");
                    ws.Cell(currentRow, 5).Value = row.Numero;
                    ws.Cell(currentRow, 6).Value = row.Iccid;
                    ws.Cell(currentRow, 7).Value = row.Emuso;
                }

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    var content = stream.ToArray();

                    return content;

                    //return File(
                    //   content,
                    //   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    //   "Equipamentos.xlsx");

                }
            }
        }

        /***************************************************************************************************/
        /****************************************** CONTADORES *********************************************/
        /***************************************************************************************************/
        [HttpGet("[action]", Name = "ContarOperadoras")]
        public int ContarOperadoras()
        {
            return _negocio.ContarOperadoras();
        }

        [HttpGet("[action]", Name = "ContarContratos")]
        public int ContarContratos()
        {
            return _negocio.ContarContratos();
        }

        [HttpGet("[action]", Name = "ContarPlanos")]
        public int ContarPlanos()
        {
            return _negocio.ContarPlanos();
        }

        [HttpGet("[action]", Name = "ContarLinhas")]
        public int ContarLinhas()
        {
            return _negocio.ContarLinhas();
        }
    }
}
