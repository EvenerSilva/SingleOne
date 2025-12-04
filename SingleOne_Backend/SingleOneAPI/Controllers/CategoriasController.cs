using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SingleOneAPI.DTOs;
using SingleOneAPI.Services;
using System;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Lista todas as categorias com filtro opcional
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CategoriaListResponseDTO>> ListarCategorias([FromQuery] string filtro = null)
        {
            try
            {
                var resultado = await _categoriaService.ListarCategoriasAsync(filtro);
                
                if (resultado.Sucesso)
                {
                    return Ok(resultado);
                }
                
                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CategoriaListResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno do servidor: {ex.Message}",
                    Dados = new System.Collections.Generic.List<CategoriaDTO>(),
                    Status = 500,
                    Total = 0
                });
            }
        }

        /// <summary>
        /// Busca uma categoria específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaResponseDTO>> BuscarCategoria(int id)
        {
            try
            {
                var resultado = await _categoriaService.BuscarCategoriaPorIdAsync(id);
                
                if (resultado.Sucesso)
                {
                    return Ok(resultado);
                }
                
                if (resultado.Status == 404)
                {
                    return NotFound(resultado);
                }
                
                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno do servidor: {ex.Message}",
                    Dados = null,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// Cria uma nova categoria
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategoriaResponseDTO>> CriarCategoria([FromBody] CategoriaCreateDTO categoriaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Dados inválidos",
                        Dados = null,
                        Status = 400
                    });
                }

                var resultado = await _categoriaService.CriarCategoriaAsync(categoriaDto);
                
                if (resultado.Sucesso)
                {
                    return CreatedAtAction(nameof(BuscarCategoria), new { id = resultado.Dados.Id }, resultado);
                }
                
                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno do servidor: {ex.Message}",
                    Dados = null,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// Atualiza uma categoria existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoriaResponseDTO>> AtualizarCategoria(int id, [FromBody] CategoriaUpdateDTO categoriaDto)
        {
            try
            {
                if (id != categoriaDto.Id)
                {
                    return BadRequest(new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "ID da URL não confere com o ID do corpo da requisição",
                        Dados = null,
                        Status = 400
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new CategoriaResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Dados inválidos",
                        Dados = null,
                        Status = 400
                    });
                }

                var resultado = await _categoriaService.AtualizarCategoriaAsync(categoriaDto);
                
                if (resultado.Sucesso)
                {
                    return Ok(resultado);
                }
                
                if (resultado.Status == 404)
                {
                    return NotFound(resultado);
                }
                
                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno do servidor: {ex.Message}",
                    Dados = null,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// Desativa uma categoria
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<CategoriaResponseDTO>> DesativarCategoria(int id)
        {
            try
            {
                var resultado = await _categoriaService.DesativarCategoriaAsync(id);
                
                if (resultado.Sucesso)
                {
                    return Ok(resultado);
                }
                
                if (resultado.Status == 404)
                {
                    return NotFound(resultado);
                }
                
                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno do servidor: {ex.Message}",
                    Dados = null,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// Reativa uma categoria desativada
        /// </summary>
        [HttpPatch("{id}/reativar")]
        public async Task<ActionResult<CategoriaResponseDTO>> ReativarCategoria(int id)
        {
            try
            {
                var resultado = await _categoriaService.ReativarCategoriaAsync(id);
                
                if (resultado.Sucesso)
                {
                    return Ok(resultado);
                }
                
                if (resultado.Status == 404)
                {
                    return NotFound(resultado);
                }
                
                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CategoriaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno do servidor: {ex.Message}",
                    Dados = null,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// Verifica se um nome de categoria já existe
        /// </summary>
        [HttpGet("verificar-nome")]
        public async Task<ActionResult<object>> VerificarNomeExistente([FromQuery] string nome, [FromQuery] int? idExcluir = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                {
                    return BadRequest(new { Sucesso = false, Mensagem = "Nome é obrigatório" });
                }

                var existe = await _categoriaService.VerificarNomeExistenteAsync(nome, idExcluir);
                
                return Ok(new { 
                    Sucesso = true, 
                    Nome = nome,
                    Existe = existe,
                    Mensagem = existe ? "Nome já existe" : "Nome disponível"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Sucesso = false, 
                    Mensagem = $"Erro interno do servidor: {ex.Message}" 
                });
            }
        }
    }
}
