using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SingleOne.Models;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SingleOneAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmpresasController : ControllerBase
    {
        private readonly IConfiguracoesNegocio _negocio;

        public EmpresasController(IConfiguracoesNegocio negocio)
        {
            _negocio = negocio;
        }

        #region EMPRESAS
        /***************************************************************************************************/
        /************************************************* EMPRESAS ****************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarEmpresas")]
        public List<Empresa> ListarEmpresas(string pesquisa, int cliente)
        {
            return _negocio.ListarEmpresas(pesquisa, cliente);
        }

        [HttpGet("[action]/{id}", Name ="BuscarEmpresaPeloID")]
        public Empresa BuscarEmpresaPeloID(int id)
        {
            return _negocio.BuscarEmpresaPeloID(id);
        }

        [HttpPost("[action]", Name ="SalvarEmpresa")]
        public ActionResult SalvarEmpresa([FromBody] Empresa empresa)
        {
            try
            {
                if (empresa == null)
                {
                    return BadRequest("Empresa não pode ser nula");
                }

                var resultado = _negocio.SalvarEmpresa(empresa);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                var status = resultadoObj.Status?.ToString();
                if (status == "200")
                {
                    return Ok(resultadoObj);
                }
                else if (status == "400" || status == "400.1")
                {
                    return BadRequest(resultadoObj);
                }
                else
                {
                    return BadRequest(resultadoObj);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("[action]/{id}", Name ="ExcluirEmpresa")]
        public string ExcluirEmpresa(int id)
        {
            return _negocio.ExcluirEmpresa(id);
        }
        #endregion
    }
}

