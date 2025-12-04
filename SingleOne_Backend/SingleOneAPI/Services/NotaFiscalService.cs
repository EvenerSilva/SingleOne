using Microsoft.AspNetCore.Http;
using SingleOne;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Services.Interface;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SingleOneAPI.Services
{
    public class NotaFiscalService : INotaFiscalService
    {
        private readonly IRepository<Notasfiscai> _repository;
        private readonly IFileUploadService _fileUploadService;

        public NotaFiscalService(IRepository<Notasfiscai> repository, IFileUploadService fileUploadService)
        {
            _repository = repository;
            _fileUploadService = fileUploadService;
        }

        public async Task<string> UploadArquivoNotaFiscal(int notaFiscalId, IFormFile arquivo, int? usuarioId)
        {
            try
            {
                Console.WriteLine($"[NOTAFISCAL-SERVICE] Upload de arquivo para nota fiscal ID: {notaFiscalId}");
                
                var notaFiscal = _repository.ObterPorId(notaFiscalId);
                if (notaFiscal == null)
                {
                    throw new EntidadeNaoEncontradaEx($"Nota Fiscal ID {notaFiscalId} não encontrada.");
                }

                // Validar tipo de arquivo
                var allowedExtensions = new[] { ".pdf", ".xml", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
                
                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    throw new ArgumentException("Tipo de arquivo não permitido. Use apenas PDF, XML, JPG ou PNG");
                }

                // Validar tamanho (máximo 10MB)
                if (arquivo.Length > 10 * 1024 * 1024)
                {
                    throw new ArgumentException("Arquivo muito grande. Tamanho máximo: 10MB");
                }

                // Remover arquivo antigo se existir
                if (!string.IsNullOrEmpty(notaFiscal.ArquivoNotaFiscal))
                {
                    _fileUploadService.DeleteFile(notaFiscal.ArquivoNotaFiscal, "notasfiscais");
                }

                // Fazer upload do novo arquivo
                var nomeArquivo = await _fileUploadService.UploadFileAsync(arquivo, "notasfiscais");
                
                // Atualizar informações no banco
                notaFiscal.ArquivoNotaFiscal = nomeArquivo;
                notaFiscal.NomeArquivoOriginal = arquivo.FileName;
                notaFiscal.DataUploadArquivo = DateTime.Now;
                notaFiscal.UsuarioUploadArquivo = usuarioId;
                
                _repository.Atualizar(notaFiscal);
                
                Console.WriteLine($"[NOTAFISCAL-SERVICE] Arquivo enviado com sucesso: {nomeArquivo}");
                return nomeArquivo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTAFISCAL-SERVICE] Erro ao fazer upload: {ex.Message}");
                throw;
            }
        }

        public async Task<(byte[] fileBytes, string fileName)> DownloadArquivoNotaFiscal(int notaFiscalId)
        {
            try
            {
                Console.WriteLine($"[NOTAFISCAL-SERVICE] Download de arquivo para nota fiscal ID: {notaFiscalId}");
                
                var notaFiscal = _repository.ObterPorId(notaFiscalId);
                if (notaFiscal == null)
                {
                    throw new EntidadeNaoEncontradaEx($"Nota Fiscal ID {notaFiscalId} não encontrada.");
                }

                if (string.IsNullOrEmpty(notaFiscal.ArquivoNotaFiscal))
                {
                    throw new EntidadeNaoEncontradaEx($"Nota Fiscal não possui arquivo anexado.");
                }

                var fileBytes = await _fileUploadService.DownloadFileAsync(notaFiscal.ArquivoNotaFiscal, "notasfiscais");
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    throw new EntidadeNaoEncontradaEx($"Arquivo não encontrado no servidor.");
                }

                Console.WriteLine($"[NOTAFISCAL-SERVICE] Arquivo baixado: {notaFiscal.NomeArquivoOriginal}");
                return (fileBytes, notaFiscal.NomeArquivoOriginal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTAFISCAL-SERVICE] Erro ao fazer download: {ex.Message}");
                throw;
            }
        }

        public Task RemoverArquivoNotaFiscal(int notaFiscalId, int? usuarioId)
        {
            try
            {
                Console.WriteLine($"[NOTAFISCAL-SERVICE] Remover arquivo da nota fiscal ID: {notaFiscalId}, Usuario: {usuarioId?.ToString() ?? "NULL"}");
                
                var notaFiscal = _repository.ObterPorId(notaFiscalId);
                if (notaFiscal == null)
                {
                    throw new EntidadeNaoEncontradaEx($"Nota Fiscal ID {notaFiscalId} não encontrada.");
                }

                if (string.IsNullOrEmpty(notaFiscal.ArquivoNotaFiscal))
                {
                    throw new EntidadeNaoEncontradaEx($"Nota Fiscal não possui arquivo anexado.");
                }

                // Remover arquivo físico
                _fileUploadService.DeleteFile(notaFiscal.ArquivoNotaFiscal, "notasfiscais");

                // Registrar remoção antes de limpar os dados
                notaFiscal.UsuarioRemocaoArquivo = usuarioId;
                notaFiscal.DataRemocaoArquivo = DateTime.Now;
                
                // Limpar informações do arquivo
                notaFiscal.ArquivoNotaFiscal = null;
                notaFiscal.NomeArquivoOriginal = null;
                notaFiscal.DataUploadArquivo = null;
                notaFiscal.UsuarioUploadArquivo = null;
                
                _repository.Atualizar(notaFiscal);
                
                Console.WriteLine($"[NOTAFISCAL-SERVICE] Arquivo removido com sucesso. Remoção registrada por usuário: {usuarioId?.ToString() ?? "NULL"}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTAFISCAL-SERVICE] Erro ao remover arquivo: {ex.Message}");
                throw;
            }
        }
    }
}

