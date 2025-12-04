using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SingleOneAPI.Services.Interface
{
    public interface INotaFiscalService
    {
        Task<string> UploadArquivoNotaFiscal(int notaFiscalId, IFormFile arquivo, int? usuarioId);
        Task<(byte[] fileBytes, string fileName)> DownloadArquivoNotaFiscal(int notaFiscalId);
        Task RemoverArquivoNotaFiscal(int notaFiscalId, int? usuarioId);
    }
}

