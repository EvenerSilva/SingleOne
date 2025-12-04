using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Modelo
    {
        public Modelo()
        {
            Equipamentos = new HashSet<Equipamento>();
            Notasfiscaisitens = new HashSet<Notasfiscaisiten>();
        }

        public int Id { get; set; }
        public int Fabricante { get; set; }
        public int Cliente { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
        public int? Migrateid { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Fabricante FabricanteNavigation { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
        public virtual ICollection<Notasfiscaisiten> Notasfiscaisitens { get; set; }
    }
}
