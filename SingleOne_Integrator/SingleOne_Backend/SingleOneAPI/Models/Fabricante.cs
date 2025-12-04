using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Fabricante
    {
        public Fabricante()
        {
            Equipamentos = new HashSet<Equipamento>();
            Modelos = new HashSet<Modelo>();
            Notasfiscaisitens = new HashSet<Notasfiscaisiten>();
        }

        public int Id { get; set; }
        public int Tipoequipamento { get; set; }
        public int Cliente { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
        public int? Migrateid { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Tipoequipamento TipoequipamentoNavigation { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
        public virtual ICollection<Modelo> Modelos { get; set; }
        public virtual ICollection<Notasfiscaisiten> Notasfiscaisitens { get; set; }
    }
}
