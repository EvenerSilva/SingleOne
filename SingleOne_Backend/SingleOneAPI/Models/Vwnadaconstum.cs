using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Vwnadaconstum
    {
        public int? Id { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Centrocusto { get; set; }
        public string Empresa { get; set; }
        public string Matricula { get; set; }
        public string Cargo { get; set; }
        public long? Maquinascomcolaborador { get; set; }
        public int? Cliente { get; set; }
    }
}
