using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Telefoniaplano
    {
        public Telefoniaplano()
        {
            Telefonialinhas = new HashSet<Telefonialinha>();
        }

        public int Id { get; set; }
        public int Contrato { get; set; }
        public string Nome { get; set; }
        public decimal Valor { get; set; }
        public bool Ativo { get; set; }

        public virtual Telefoniacontrato ContratoNavigation { get; set; }
        public virtual ICollection<Telefonialinha> Telefonialinhas { get; set; }
    }
}
