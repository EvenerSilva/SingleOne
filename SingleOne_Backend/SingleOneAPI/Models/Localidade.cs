using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Localidade
    {
        public Localidade()
        {
            Colaboradores = new HashSet<Colaboradore>();
            Equipamentos = new HashSet<Equipamento>();
        }

        public int Id { get; set; }
        public int Cliente { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public int? Migrateid { get; set; }
        public string Cidade { get; set; } = string.Empty;        // Campo novo para cidade
        public string Estado { get; set; } = string.Empty;        // Campo novo para estado

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual ICollection<Colaboradore> Colaboradores { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
    }
}
