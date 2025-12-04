using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Telefonialinha
    {
        public int Id { get; set; }
        public int Plano { get; set; }
        public decimal Numero { get; set; }
        public string Iccid { get; set; }
        public bool Emuso { get; set; }
        public bool Ativo { get; set; }

        public virtual Telefoniaplano PlanoNavigation { get; set; }
    }
}
