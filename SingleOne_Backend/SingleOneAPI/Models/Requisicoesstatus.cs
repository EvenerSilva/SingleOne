using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Requisicoesstatus
    {
        public Requisicoesstatus()
        {
            Requisicos = new HashSet<Requisico>();
        }

        public int Id { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }

        public virtual ICollection<Requisico> Requisicos { get; set; }
    }
}
