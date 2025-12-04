using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Equipamentosstatus
    {
        public Equipamentosstatus()
        {
            Equipamentohistoricos = new HashSet<Equipamentohistorico>();
            Equipamentos = new HashSet<Equipamento>();
        }

        public int Id { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }

        public virtual ICollection<Equipamentohistorico> Equipamentohistoricos { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
    }
}
