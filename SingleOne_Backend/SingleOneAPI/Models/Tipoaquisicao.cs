using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Tipoaquisicao
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
    }
}