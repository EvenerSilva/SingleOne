using Microsoft.AspNetCore.Http;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneAPI.Services.Interface
{
    public interface IContratoService : IService<Contrato>
    {
        Task<List<ContratoDTO>> Listar(int cliente);
        Task<List<ContratoDTO>> Listar(int cliente, int fornecedor);
        Task<List<StatusContrato>> ListarStatus();
        Task<ContratoDTO> ObterPorId(int id);
        Task CriarContrato(CriarNovoContrato novoContrato);
        Task AtualizarContrato(AtualizarContrato atualizarContrato);
        Task<string> UploadArquivoContrato(int contratoId, IFormFile arquivo, int? usuarioId);
        Task<(byte[] fileBytes, string fileName)> DownloadArquivoContrato(int contratoId);
        Task RemoverArquivoContrato(int contratoId, int? usuarioId);
    }
}
