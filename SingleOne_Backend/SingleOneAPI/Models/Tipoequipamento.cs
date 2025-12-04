using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SingleOneAPI.Models
{
    public partial class Tipoequipamento
    {
        public Tipoequipamento()
        {
            Equipamentos = new HashSet<Equipamento>();
            Fabricantes = new HashSet<Fabricante>();
            Notasfiscaisitens = new HashSet<Notasfiscaisiten>();
            Tipoequipamentosclientes = new HashSet<Tipoequipamentoscliente>();
        }

        public int Id { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
        public int? CategoriaId { get; set; }
        [JsonProperty("transitolivre")]
        public bool TransitoLivre { get; set; }

        public virtual ICollection<Equipamento> Equipamentos { get; set; }
        public virtual ICollection<Fabricante> Fabricantes { get; set; }
        public virtual ICollection<Notasfiscaisiten> Notasfiscaisitens { get; set; }
        public virtual ICollection<Tipoequipamentoscliente> Tipoequipamentosclientes { get; set; }
        public virtual Categoria Categoria { get; set; }
    }
}
