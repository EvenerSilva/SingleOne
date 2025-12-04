using Microsoft.AspNetCore.Http;

namespace SingleOneAPI.Models.DTO
{
    public class UploadEvidenciaDTO
    {
        public int Equipamento { get; set; }
        public string Descricao { get; set; }
        public string TipoProcesso { get; set; }
        public IFormFile Arquivo { get; set; }
    }
}

