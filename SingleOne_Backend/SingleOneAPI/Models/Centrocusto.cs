using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Centrocusto
    {
        public Centrocusto()
        {
            Colaboradores = new HashSet<Colaboradore>();
            Equipamentos = new HashSet<Equipamento>();
        }

        public int Id { get; set; }
        public int Empresa { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public int? FilialId { get; set; }
        public int? Migrateid { get; set; }
        public bool Ativo { get; set; } = true; // ✅ ADICIONADO: Campo Ativo do banco
        public DateTime? CreatedAt { get; set; } // ✅ ADICIONADO: Campo CreatedAt do banco
        public DateTime? UpdatedAt { get; set; } // ✅ ADICIONADO: Campo UpdatedAt do banco

        public virtual Empresa EmpresaNavigation { get; set; }
        // Comentando temporariamente para resolver conflito de mapeamento
        // public virtual Filial Filial { get; set; }
        public virtual ICollection<Colaboradore> Colaboradores { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
    }
}
