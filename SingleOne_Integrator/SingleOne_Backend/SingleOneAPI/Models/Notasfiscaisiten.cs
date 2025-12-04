using SingleOneAPI.Models;
using System;

namespace SingleOneAPI.Models
{
    public partial class Notasfiscaisiten
    {
        public int Id { get; set; }
        public int Notafiscal { get; set; }
        public int Tipoequipamento { get; set; }
        public int Fabricante { get; set; }
        public int Modelo { get; set; }
        public int Quantidade { get; set; }
        public decimal Valorunitario { get; set; }
        public int TipoAquisicao { get; set; }
        public DateTime? Dtlimitegarantia { get; set; }
        public int? Contrato { get; set; }

        public virtual Fabricante FabricanteNavigation { get; set; }
        public virtual Modelo ModeloNavigation { get; set; }
        public virtual Notasfiscai NotafiscalNavigation { get; set; }
        public virtual Tipoequipamento TipoequipamentoNavigation { get; set; }
        public virtual Contrato ContratoNavigation { get; set; }
    }
}
