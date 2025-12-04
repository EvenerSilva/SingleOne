using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SingleOne;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Services.Interface;
using SingleOneAPI.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;

namespace SingleOneAPI.Services
{
    public class ContratoService : Service<Contrato>, IContratoService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Contrato> _repository;
        private readonly IRepository<StatusContrato> _statusRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IFileUploadService _fileUploadService;
        
        public ContratoService(IRepository<Contrato> repository, IRepository<StatusContrato> statusRepository, IRepository<Equipamento> equipamentoRepository, IFileUploadService fileUploadService, IMapper mapper) 
            : base(repository, mapper)
        {
            _repository = repository;
            _statusRepository = statusRepository;
            _equipamentoRepository = equipamentoRepository;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
        }

        public Task AtualizarContrato(AtualizarContrato atualizarContrato)
        {
            try
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Atualizando contrato ID: {atualizarContrato.Id}");
                
                Contrato contratoDb = _repository.ObterPorId(atualizarContrato.Id);
                if (contratoDb == null)
                {
                    Console.WriteLine($"[CONTRATO-SERVICE] Contrato ID {atualizarContrato.Id} não encontrado");
                    throw new EntidadeNaoEncontradaEx($"Contrato de ID {atualizarContrato.Id} não encontrado.");
                }
                
                Console.WriteLine($"[CONTRATO-SERVICE] Contrato encontrado: {contratoDb.Numero}/{contratoDb.Aditivo}");
                
                // Atualizar apenas campos que não são nulos
                if (atualizarContrato.Fornecedor > 0)
                    contratoDb.Fornecedor = atualizarContrato.Fornecedor;
                    
                if (atualizarContrato.Numero.HasValue)
                    contratoDb.Numero = atualizarContrato.Numero;
                    
                if (atualizarContrato.Aditivo.HasValue)
                    contratoDb.Aditivo = atualizarContrato.Aditivo;
                    
                if (!string.IsNullOrEmpty(atualizarContrato.Descricao))
                    contratoDb.Descricao = atualizarContrato.Descricao;
                    
                contratoDb.DTInicioVigencia = atualizarContrato.DTInicioVigencia;
                contratoDb.DTFinalVigencia = atualizarContrato.DTFinalVigencia;
                contratoDb.Valor = atualizarContrato.Valor;
                contratoDb.GeraNF = atualizarContrato.GeraNF;
                contratoDb.Renovavel = atualizarContrato.Renovavel;
                
                if (atualizarContrato.DTFinalVigencia.HasValue)
                {
                    contratoDb.Status = Contrato.VerificarStatusContrato(atualizarContrato.DTInicioVigencia, atualizarContrato.DTFinalVigencia.Value);
                }
                
                Console.WriteLine($"[CONTRATO-SERVICE] Salvando alterações...");
                Update<Contrato, ContratoValidator>(contratoDb);
                Console.WriteLine($"[CONTRATO-SERVICE] Contrato atualizado com sucesso");
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Erro ao atualizar contrato: {ex.Message}");
                Console.WriteLine($"[CONTRATO-SERVICE] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public Task CriarContrato(CriarNovoContrato novoContrato)
        {
            var contratoDb = _repository
                .Buscar(x => x.Numero == novoContrato.Numero && 
                        x.Aditivo == novoContrato.Aditivo)
                .FirstOrDefault();

            if (contratoDb != null) 
                throw new EntidadeJaExisteEx($"Contrato {contratoDb.Numero}/{contratoDb.Aditivo} já está cadastrado no sistema.");

            // Converter DateTimes UTC para Local se necessário
            var dtInicioVigencia = novoContrato.DTInicioVigencia;
            if (dtInicioVigencia.Kind == DateTimeKind.Utc)
            {
                dtInicioVigencia = DateTime.SpecifyKind(dtInicioVigencia, DateTimeKind.Local);
            }
            
            DateTime? dtFinalVigencia = novoContrato.DTFinalVigencia;
            if (dtFinalVigencia.HasValue && dtFinalVigencia.Value.Kind == DateTimeKind.Utc)
            {
                dtFinalVigencia = DateTime.SpecifyKind(dtFinalVigencia.Value, DateTimeKind.Local);
            }

            // ✅ CORREÇÃO: Mapeamento manual para evitar problemas do AutoMapper
            var contrato = new Contrato
            {
                Cliente = novoContrato.Cliente,
                Fornecedor = novoContrato.FornecedorId, // Usar o fornecedor selecionado
                Numero = novoContrato.Numero,
                Aditivo = novoContrato.Aditivo,
                Descricao = novoContrato.Descricao,
                DTInicioVigencia = dtInicioVigencia,
                DTFinalVigencia = dtFinalVigencia,
                Valor = novoContrato.Valor,
                GeraNF = novoContrato.GeraNF,
                Renovavel = novoContrato.Renovavel,
                Status = 2, // Status 2 = Vigente
                DTCriacao = DateTime.Now,
                UsuarioCriacao = novoContrato.UsuarioCriacao,
                DTExclusao = null,
                UsuarioExclusao = null
            };

            _repository.Adicionar(contrato);
            return Task.CompletedTask;
        }

        public Task<List<ContratoDTO>> Listar(int cliente)
        {
            try
            {
                var contratos = _repository
                    .Include(i => i.StatusContratoNavigation)
                    .Include(i => i.FornecedorNavigation)
                    .Where(x => x.Cliente == cliente && x.DTExclusao == null)
                    .ToList();

                // DEBUG: Ver se os dados estão no banco ANTES do mapper
                Console.WriteLine($"[CONTRATO-SERVICE] DEBUG - Contratos carregados do banco:");
                foreach (var c in contratos.Take(3))
                {
                    Console.WriteLine($"[CONTRATO-SERVICE] DEBUG - Contrato {c.Id}: ArquivoContrato={c.ArquivoContrato ?? "NULL"}, NomeOriginal={c.NomeArquivoOriginal ?? "NULL"}");
                }

                // Usar AutoMapper para mapear os contratos
                var result = _mapper.Map<List<ContratoDTO>>(contratos);
                
                // Atualizar quantidade de recursos para cada contrato
                foreach (var contrato in result)
                {
                    var contratoOriginal = contratos.First(c => c.Id == contrato.Id);
                    contrato.QtdeRecursos = _equipamentoRepository
                        .Buscar(e => e.Contrato == contratoOriginal.Id && e.Ativo == true)
                        .Count();
                }

                Console.WriteLine($"[CONTRATO-SERVICE] Listando {result.Count} contratos para cliente {cliente}");
                foreach (var contrato in result)
                {
                    Console.WriteLine($"[CONTRATO-SERVICE] Contrato {contrato.Numero}/{contrato.Aditivo}: {contrato.QtdeRecursos} recursos, Renovavel: {contrato.Renovavel}, DiasParaVencimento: {contrato.DiasParaVencimento}, TemArquivo: {contrato.TemArquivo}, NomeArquivo: {contrato.NomeArquivoOriginal ?? "NULL"}");
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                // Log do erro para debug
                Console.WriteLine($"[CONTRATO-SERVICE] Erro ao listar contratos: {ex.Message}");
                Console.WriteLine($"[CONTRATO-SERVICE] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public Task<List<ContratoDTO>> Listar(int cliente, int fornecedor)
        {
            try
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Iniciando listagem para cliente {cliente} e fornecedor {fornecedor}");
                
                var contratos = _repository
                    .Include(i => i.StatusContratoNavigation)
                    .Include(i => i.FornecedorNavigation)
                    .Where(x => x.DTExclusao == null && x.Cliente == cliente && x.Fornecedor == fornecedor)
                    .ToList();
                
                Console.WriteLine($"[CONTRATO-SERVICE] Contratos encontrados no banco: {contratos.Count}");
                foreach (var contrato in contratos)
                {
                    Console.WriteLine($"[CONTRATO-SERVICE] Contrato encontrado: ID={contrato.Id}, Numero={contrato.Numero}, Fornecedor={contrato.Fornecedor}");
                }

                // Usar AutoMapper para mapear os contratos
                var result = _mapper.Map<List<ContratoDTO>>(contratos);
                
                // Atualizar quantidade de recursos para cada contrato
                foreach (var contrato in result)
                {
                    var contratoOriginal = contratos.First(c => c.Id == contrato.Id);
                    contrato.QtdeRecursos = _equipamentoRepository
                        .Buscar(e => e.Contrato == contratoOriginal.Id && e.Ativo == true)
                        .Count();
                }

                Console.WriteLine($"[CONTRATO-SERVICE] Listando {result.Count} contratos para cliente {cliente} e fornecedor {fornecedor}");
                foreach (var contrato in result)
                {
                    Console.WriteLine($"[CONTRATO-SERVICE] Contrato {contrato.Numero}/{contrato.Aditivo}: {contrato.QtdeRecursos} recursos");
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Erro ao listar contratos por fornecedor: {ex.Message}");
                throw;
            }
        }

        public Task<List<StatusContrato>> ListarStatus()
        {
            var result = _statusRepository.ObterTodos().ToList();
            return Task.FromResult(result);
        }

        public Task<ContratoDTO> ObterPorId(int id)
        {
            var colaboradorDb = _repository
                .Include(i => i.StatusContratoNavigation)
                .Include(i => i.FornecedorNavigation)
                .Where(x => x.DTExclusao == null && x.Id == id)
                .Select(s => _mapper.Map<ContratoDTO>(s))
                .FirstOrDefault();

            if (colaboradorDb == null)
                throw new EntidadeNaoEncontradaEx($"Contrato de id {id} não encontrado.");

            return Task.FromResult(colaboradorDb);
        }

        public async Task<string> UploadArquivoContrato(int contratoId, IFormFile arquivo, int? usuarioId)
        {
            try
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Upload de arquivo para contrato ID: {contratoId}");
                
                var contrato = _repository.ObterPorId(contratoId);
                if (contrato == null)
                {
                    throw new EntidadeNaoEncontradaEx($"Contrato ID {contratoId} não encontrado.");
                }

                // Validar tipo de arquivo
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
                
                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    throw new ArgumentException("Tipo de arquivo não permitido. Use apenas PDF, DOC ou DOCX");
                }

                // Validar tamanho (máximo 10MB)
                if (arquivo.Length > 10 * 1024 * 1024)
                {
                    throw new ArgumentException("Arquivo muito grande. Tamanho máximo: 10MB");
                }

                // Remover arquivo antigo se existir
                if (!string.IsNullOrEmpty(contrato.ArquivoContrato))
                {
                    _fileUploadService.DeleteFile(contrato.ArquivoContrato, "contratos");
                }

                // Fazer upload do novo arquivo
                var nomeArquivo = await _fileUploadService.UploadFileAsync(arquivo, "contratos");
                
                // Atualizar informações no banco
                contrato.ArquivoContrato = nomeArquivo;
                contrato.NomeArquivoOriginal = arquivo.FileName;
                contrato.DataUploadArquivo = DateTime.Now;
                contrato.UsuarioUploadArquivo = usuarioId;
                
                _repository.Atualizar(contrato);
                
                Console.WriteLine($"[CONTRATO-SERVICE] Arquivo enviado com sucesso: {nomeArquivo}");
                return nomeArquivo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Erro ao fazer upload: {ex.Message}");
                throw;
            }
        }

        public async Task<(byte[] fileBytes, string fileName)> DownloadArquivoContrato(int contratoId)
        {
            try
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Download de arquivo para contrato ID: {contratoId}");
                
                var contrato = _repository.ObterPorId(contratoId);
                if (contrato == null)
                {
                    throw new EntidadeNaoEncontradaEx($"Contrato ID {contratoId} não encontrado.");
                }

                if (string.IsNullOrEmpty(contrato.ArquivoContrato))
                {
                    throw new EntidadeNaoEncontradaEx($"Contrato não possui arquivo anexado.");
                }

                var fileBytes = await _fileUploadService.DownloadFileAsync(contrato.ArquivoContrato, "contratos");
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    throw new EntidadeNaoEncontradaEx($"Arquivo não encontrado no servidor.");
                }

                Console.WriteLine($"[CONTRATO-SERVICE] Arquivo baixado: {contrato.NomeArquivoOriginal}");
                return (fileBytes, contrato.NomeArquivoOriginal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Erro ao fazer download: {ex.Message}");
                throw;
            }
        }

        public Task RemoverArquivoContrato(int contratoId, int? usuarioId)
        {
            try
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Remover arquivo do contrato ID: {contratoId}, Usuario: {usuarioId?.ToString() ?? "NULL"}");
                
                var contrato = _repository.ObterPorId(contratoId);
                if (contrato == null)
                {
                    throw new EntidadeNaoEncontradaEx($"Contrato ID {contratoId} não encontrado.");
                }

                if (string.IsNullOrEmpty(contrato.ArquivoContrato))
                {
                    throw new EntidadeNaoEncontradaEx($"Contrato não possui arquivo anexado.");
                }

                // Remover arquivo físico
                _fileUploadService.DeleteFile(contrato.ArquivoContrato, "contratos");

                // Registrar remoção antes de limpar os dados
                contrato.UsuarioRemocaoArquivo = usuarioId;
                contrato.DataRemocaoArquivo = DateTime.Now;
                
                // Limpar informações do arquivo
                contrato.ArquivoContrato = null;
                contrato.NomeArquivoOriginal = null;
                contrato.DataUploadArquivo = null;
                contrato.UsuarioUploadArquivo = null;
                
                _repository.Atualizar(contrato);
                
                Console.WriteLine($"[CONTRATO-SERVICE] Arquivo removido com sucesso. Remoção registrada por usuário: {usuarioId?.ToString() ?? "NULL"}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTRATO-SERVICE] Erro ao remover arquivo: {ex.Message}");
                throw;
            }
        }
    }
}
