using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SingleOneAPI.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadLogoAsync(IFormFile file, int clienteId);
        Task<string> UploadFileAsync(IFormFile file, string pasta);
        Task<byte[]> DownloadFileAsync(string fileName, string pasta);
        bool DeleteLogo(string fileName);
        bool DeleteFile(string fileName, string pasta);
        string GetLogoPath(string fileName);
        string GetFilePath(string fileName, string pasta);
        bool LogoExists(string fileName);
    }
}
