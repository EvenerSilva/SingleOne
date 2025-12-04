using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    public class ReordenarEvidenciasRequest
    {
        public int LaudoId { get; set; }
        public List<int> OrdemEvidencias { get; set; } = new List<int>();
    }
}
