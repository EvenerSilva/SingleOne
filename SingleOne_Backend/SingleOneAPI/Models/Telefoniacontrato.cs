using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Telefoniacontrato
    {
        public Telefoniacontrato()
        {
            Telefoniaplanos = new HashSet<Telefoniaplano>();
        }

        public int Id { get; set; }
        public int Cliente { get; set; }
        public int Operadora { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Telefoniaoperadora OperadoraNavigation { get; set; }
        public virtual ICollection<Telefoniaplano> Telefoniaplanos { get; set; }
    }
}
