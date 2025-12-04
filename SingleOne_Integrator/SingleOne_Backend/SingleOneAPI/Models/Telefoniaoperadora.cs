using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Telefoniaoperadora
    {
        public Telefoniaoperadora()
        {
            Telefoniacontratos = new HashSet<Telefoniacontrato>();
        }

        public int Id { get; set; }
        public string Nome { get; set; }
        public bool Ativo { get; set; }

        public virtual ICollection<Telefoniacontrato> Telefoniacontratos { get; set; }
    }
}
