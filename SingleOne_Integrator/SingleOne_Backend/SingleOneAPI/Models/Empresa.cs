using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SingleOneAPI.Models
{
    public partial class Empresa
    {
        public Empresa()
        {
            Centrocustos = new HashSet<Centrocusto>();
            Colaboradores = new HashSet<Colaboradore>();
            Equipamentos = new HashSet<Equipamento>();
            Filiais = new HashSet<Filial>();
        }

        public int Id { get; set; }
        public int Cliente { get; set; }
        public string Nome { get; set; } = null!;
        public string Cnpj { get; set; } = null!;
        [Column("localidade_id")]
        [JsonProperty("localidade_id")]
        public int? LocalidadeId { get; set; }
        
        [Column("migrateid")]
        public int? Migrateid { get; set; }
        
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
        
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public virtual Localidade? Localidade { get; set; }
        public virtual Cliente ClienteNavigation { get; set; } = null!;
        public virtual ICollection<Filial> Filiais { get; set; }
        public virtual ICollection<Centrocusto> Centrocustos { get; set; }
        public virtual ICollection<Colaboradore> Colaboradores { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
        
        // Propriedade para total de filiais (não mapeada no banco)
        [NotMapped]
        public int TotalFiliais { get; set; }
    }
}
