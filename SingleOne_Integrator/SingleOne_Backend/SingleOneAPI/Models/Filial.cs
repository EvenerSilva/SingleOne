using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    public partial class Filial
    {
        public Filial()
        {
            Colaboradores = new HashSet<Colaboradore>();
            Equipamentos = new HashSet<Equipamento>();
            // Comentando temporariamente para resolver conflito de mapeamento
            // Centrocustos = new HashSet<Centrocusto>();
        }

        public int Id { get; set; }
        public string Nome { get; set; }
        [Column("empresa_id")]
        public int EmpresaId { get; set; }
        
        [Column("localidade_id")]
        public int LocalidadeId { get; set; }
        
        public string Cnpj { get; set; }
        public string Endereco { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public bool? Ativo { get; set; }
        
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
        
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public virtual Empresa Empresa { get; set; }
        public virtual Localidade Localidade { get; set; }
        public virtual ICollection<Colaboradore> Colaboradores { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
        // Comentando temporariamente para resolver conflito de mapeamento
        // public virtual ICollection<Centrocusto> Centrocustos { get; set; }
    }
}
