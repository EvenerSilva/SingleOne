using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingleOne.Util;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DescarteEvidenciaController : ControllerBase
    {
        private readonly IRepository<DescarteEvidencia> _repository;
        private readonly string _pastaEvidencias = "evidencias"; // Pasta relativa para salvar arquivos

        public DescarteEvidenciaController(IRepository<DescarteEvidencia> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Upload de evidência de descarte
        /// </summary>
        [HttpPost("[action]")]
        public IActionResult UploadEvidencia([FromForm] int equipamento, [FromForm] string tipoProcesso, [FromForm] string descricao, IFormFile arquivo)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                    return BadRequest("Nenhum arquivo foi enviado");

                // Validar tipo de arquivo (aceitar imagens e PDFs)
                var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".webp" };
                var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
                
                if (!extensoesPermitidas.Contains(extensao))
                    return BadRequest("Tipo de arquivo não permitido. Envie apenas imagens (JPG, PNG, GIF, WEBP) ou PDF");

                // Criar pasta se não existir
                var caminhoCompleto = Path.Combine(Directory.GetCurrentDirectory(), _pastaEvidencias);
                if (!Directory.Exists(caminhoCompleto))
                    Directory.CreateDirectory(caminhoCompleto);

                // Gerar nome único para o arquivo
                var nomeUnico = $"{equipamento}_{tipoProcesso}_{DateTime.Now:yyyyMMddHHmmss}{extensao}";
                var caminhoArquivo = Path.Combine(caminhoCompleto, nomeUnico);

                // Salvar arquivo
                using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                {
                    arquivo.CopyTo(stream);
                }

                // Obter ID do usuário do token
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Criar registro no banco
                var evidencia = new DescarteEvidencia
                {
                    Equipamento = equipamento,
                    Descricao = descricao,
                    Tipoprocesso = tipoProcesso,
                    Nomearquivo = arquivo.FileName,
                    Caminhoarquivo = Path.Combine(_pastaEvidencias, nomeUnico),
                    Tipoarquivo = arquivo.ContentType,
                    Tamanhoarquivo = arquivo.Length,
                    Usuarioupload = usuarioId,
                    Dataupload = TimeZoneMapper.GetDateTimeNow(),
                    Ativo = true
                };

                _repository.Adicionar(evidencia);

                return Ok(new { 
                    mensagem = "Evidência enviada com sucesso", 
                    evidencia = evidencia 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao fazer upload da evidência: {ex.Message}");
            }
        }

        /// <summary>
        /// Listar evidências de um equipamento
        /// </summary>
        [HttpGet("[action]/{equipamento}")]
        public IActionResult ListarEvidencias(int equipamento)
        {
            try
            {
                var evidencias = _repository
                    .Buscar(e => e.Equipamento == equipamento && e.Ativo)
                    .OrderByDescending(e => e.Dataupload)
                    .ToList();

                return Ok(evidencias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar evidências: {ex.Message}");
            }
        }

        /// <summary>
        /// Download de evidência
        /// </summary>
        [HttpGet("[action]/{id}")]
        public IActionResult DownloadEvidencia(int id)
        {
            try
            {
                var evidencia = _repository.ObterPorId(id);
                
                if (evidencia == null || !evidencia.Ativo)
                    return NotFound("Evidência não encontrada");

                var caminhoCompleto = Path.Combine(Directory.GetCurrentDirectory(), evidencia.Caminhoarquivo);
                
                if (!System.IO.File.Exists(caminhoCompleto))
                    return NotFound("Arquivo não encontrado no servidor");

                var bytes = System.IO.File.ReadAllBytes(caminhoCompleto);
                return File(bytes, evidencia.Tipoarquivo ?? "application/octet-stream", evidencia.Nomearquivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao fazer download da evidência: {ex.Message}");
            }
        }

        /// <summary>
        /// Excluir evidência (soft delete)
        /// </summary>
        [HttpDelete("[action]/{id}")]
        public IActionResult ExcluirEvidencia(int id)
        {
            try
            {
                var evidencia = _repository.ObterPorId(id);
                
                if (evidencia == null)
                    return NotFound("Evidência não encontrada");

                evidencia.Ativo = false;
                _repository.Atualizar(evidencia);

                return Ok(new { mensagem = "Evidência excluída com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir evidência: {ex.Message}");
            }
        }

        /// <summary>
        /// Contar evidências por tipo de processo de um equipamento
        /// </summary>
        [HttpGet("[action]/{equipamento}")]
        public IActionResult ContarEvidenciasPorTipo(int equipamento)
        {
            try
            {
                var evidencias = _repository
                    .Buscar(e => e.Equipamento == equipamento && e.Ativo)
                    .GroupBy(e => e.Tipoprocesso)
                    .Select(g => new { tipoProcesso = g.Key, quantidade = g.Count() })
                    .ToList();

                return Ok(evidencias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao contar evidências: {ex.Message}");
            }
        }
    }
}

