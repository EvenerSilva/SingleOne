using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Tipoequipamentoscliente
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public int Tipo { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Tipoequipamento TipoNavigation { get; set; }
    }
}
