using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOne.Util;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EquipamentoController : ControllerBase
    {
        private readonly IEquipamentoNegocio _negocio;
        public EquipamentoController(IConfiguration config, IEquipamentoNegocio negocio)
        {
            _negocio = negocio;
        }

        [HttpGet("[action]/{pesquisa}/{cliente}/{contrato}/{pagina}/{paginaTamanho}", Name ="ListarEquipamentos")]
        public PagedResult<Equipamentovm> ListarEquipamentos(string pesquisa, int cliente, int? contrato, int pagina, int paginaTamanho, [FromQuery] int? modeloId = null, [FromQuery] int? localidadeId = null)
        {
            return _negocio.ListarEquipamentos(pesquisa, cliente, contrato, pagina, paginaTamanho, modeloId, localidadeId);
        }

        [HttpGet("[action]/{cliente}", Name = "ListarTodosEquipamentosParaResumo")]
        [AllowAnonymous] // ✅ TEMPORÁRIO: Remover autenticação para teste
        public List<Equipamentovm> ListarTodosEquipamentosParaResumo(int cliente)
        {
            return _negocio.ListarTodosEquipamentosParaResumo(cliente);
        }

        [HttpGet("[action]/{pesquisa}/{cliente}", Name = "ListarEquipamentosDisponiveis")]
        public List<Equipamentovm> ListarEquipamentosDisponiveis(string pesquisa, int cliente)
        {
            return _negocio.ListarEquipamentosDisponiveis(pesquisa, cliente);
        }
        [HttpGet("[action]/{pesquisa}/{cliente}", Name = "ListarEquipamentoDisponivelParaLaudos")]
        public List<Equipamentovm> ListarEquipamentoDisponivelParaLaudos(string pesquisa, int cliente)
        {
            return _negocio.ListarEquipamentoDisponivelParaLaudos(pesquisa, cliente);
        }
        [HttpGet("[action]/{cliente}", Name = "ListarEquipamentosDisponiveisParaEstoque")]
        public List<Equipamentovm> ListarEquipamentosDisponiveisParaEstoque(int cliente)
        {
            return _negocio.ListarEquipamentosDisponiveisParaEstoque(cliente);
        }
        [HttpGet("[action]/{idEquipamento}", Name = "ListarAnexosDoEquipamento")]
        public List<Equipamentoanexo> ListarAnexosDoEquipamento(int idEquipamento)
        {
            return _negocio.AnexosDoEquipamento(idEquipamento);
        }
        [HttpGet("[action]", Name ="ListarStatusEquipamentos")]
        public List<Equipamentosstatus> ListarStatusEquipamentos()
        {
            return _negocio.ListarStatusEquipamentos();
        }
        [HttpGet("[action]/{cliente}/{colaborador}", Name = "EquipamentosDoTermoDeEntrega")]
        public List<Termoentregavm> EquipamentosDoTermoDeEntrega(int cliente, int colaborador)
        {
            return _negocio.EquipamentosDoTermoDeEntrega(cliente, colaborador);
        }
        [HttpGet("[action]/{cliente}", Name = "ExportarParaExcel")]
        public byte[] ExportarParaExcel(int cliente)
        {
            //return negocio.ExportarParaExcel(cliente);
            var eqpts = _negocio.ExportarParaExcel(cliente);

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Equipamentos");
                var currentRow = 1;
                ws.Cell(currentRow, 1).Value = "Id";
                ws.Cell(currentRow, 2).Value = "TipoEquipamento";
                ws.Cell(currentRow, 3).Value = "Fabricante";
                ws.Cell(currentRow, 4).Value = "Modelo";
                ws.Cell(currentRow, 5).Value = "NotaFiscal";
                ws.Cell(currentRow, 6).Value = "StatusEquipamento";
                ws.Cell(currentRow, 7).Value = "UsuarioCadastro";
                ws.Cell(currentRow, 8).Value = "Local";
                ws.Cell(currentRow, 9).Value = "PossuiBO";
                ws.Cell(currentRow, 10).Value = "DescricaoBO";
                ws.Cell(currentRow, 11).Value = "NumeroSerie";
                ws.Cell(currentRow, 12).Value = "Patrimônio";
                ws.Cell(currentRow, 13).Value = "DtCadastro";
                ws.Cell(currentRow, 14).Value = "Alugado";
                ws.Cell(currentRow, 15).Value = "Colaborador";
                ws.Cell(currentRow, 16).Value = "Empresa";
                ws.Cell(currentRow, 17).Value = "CentroCusto";
                //ws.Cell(currentRow, 16).Value = "Ativo";

                foreach (var eqp in eqpts)
                {
                    currentRow++;
                    ws.Cell(currentRow, 1).Value = eqp.Id;
                    ws.Cell(currentRow, 2).Value = eqp.Tipoequipamento;
                    ws.Cell(currentRow, 3).Value = eqp.Fabricante;
                    ws.Cell(currentRow, 4).Value = eqp.Modelo;
                    ws.Cell(currentRow, 5).Value = eqp.Notafiscal;
                    ws.Cell(currentRow, 6).Value = eqp.Equipamentostatus;
                    ws.Cell(currentRow, 7).Value = eqp.Usuariocadastro;
                                         ws.Cell(currentRow, 8).Value = eqp.Localizacao;
                    ws.Cell(currentRow, 9).Value = eqp.Possuibo;
                    ws.Cell(currentRow, 10).Value = eqp.Descricaobo;
                    ws.Cell(currentRow, 11).Value = eqp.Numeroserie;
                    ws.Cell(currentRow, 12).Value = eqp.Patrimonio;
                    ws.Cell(currentRow, 13).Value = eqp.Dtcadastro.Value.ToString("dd/MM/yyyy");
                    ws.Cell(currentRow, 14).Value = eqp.TipoAquisicao;
                    ws.Cell(currentRow, 15).Value = eqp.Colaborador;
                    ws.Cell(currentRow, 16).Value = eqp.Empresa;
                    ws.Cell(currentRow, 17).Value = eqp.Centrocusto;
                    //ws.Cell(currentRow, 16).Value = (eqp.Ativo == true) ? "Sim" : "Não";
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


        [HttpGet("[action]/{id}", Name ="BuscarEquipamentoPorId")]
        [AllowAnonymous] // ✅ TEMPORÁRIO: Remover autenticação para teste
        public Equipamento BuscarEquipamentoPorId(int id)
        {
            var resultado = _negocio.BuscarEquipamentoPorId(id);
            
            return resultado;
        }

        [HttpGet("[action]/{id}", Name = "VisualizarRecurso")]
        public Equipamento VisualizarRecurso(int id)
        {
            return _negocio.VisualizarRecurso(id);
        }

        [HttpGet("[action]/{cliente}/{numeroSerie}", Name = "BuscarEquipamentoPorNumeroSeriePatrimonio")]
        public PagedResult<Equipamentovm> BuscarEquipamentoPorNumeroSeriePatrimonio(int cliente, string numeroSerie)
        {
            return _negocio.BuscarEquipamentoPorNumeroSeriePatrimonio(cliente, numeroSerie);
        }
        [HttpGet("[action]/{usuario}/{equipamento}", Name = "LiberarEquipamentoParaEstoque")]
        public int LiberarEquipamentoParaEstoque(int usuario, int equipamento)
        {
            return _negocio.LiberarParaEstoque(usuario, equipamento);
        }


        [HttpPost("[action]", Name ="SalvarEquipamento")]
        public string SalvarEquipamento(Equipamento eqp)
        {
            return _negocio.SalvarEquipamento(eqp);
        }
        [HttpPost("[action]", Name ="IncluirAnexo")]
        public void IncluirAnexo(Equipamentoanexo eqp)
        {
            _negocio.IncluirAnexo(eqp);
        }
        [HttpPost("[action]", Name ="RegistrarBO")]
        public void RegistrarBO(Equipamento eqp)
        {
            _negocio.RegistrarBO(eqp);
        }

        [HttpDelete("[action]/{id}", Name = "ExcluirEquipamento")]
        public void ExcluirEquipamento(int id)
        {
            _negocio.ExcluirEquipamento(id);
        }
        [HttpDelete("[action]/{id}", Name = "ExcluirAnexo")]
        public void ExcluirAnexo(int id)
        {
            _negocio.ExcluirAnexo(id);
        }


        //Descarte
        [HttpGet("[action]/{cliente}/{pesquisa}", Name ="ListarEquipamentosDisponiveisParaDescarte")]
        public List<DescarteVM> ListarEquipamentosDisponiveisParaDescarte(int cliente, string pesquisa)
        {
            return _negocio.ListarEquipamentosDisponiveisParaDescarte(cliente, pesquisa);
        }
        [HttpPost("[action]", Name ="RealizarDescarte")]
        public void RealizarDescarte(List<DescarteVM> descartes)
        {
            _negocio.RealizarDescarte(descartes);
        }


        //Reativação
        [HttpGet("[action]/{id}", Name ="ReativarEquipamento")]
        public void ReativarEquipamento(int id)
        {
            _negocio.ReativarEquipamento(id);
        }
    }
}
