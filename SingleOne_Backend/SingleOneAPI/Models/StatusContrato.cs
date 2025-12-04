using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public class StatusContrato
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public ICollection<Contrato> Contratos { get; set; }
    }

}
