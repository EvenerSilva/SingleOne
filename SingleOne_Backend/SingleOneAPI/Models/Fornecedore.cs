using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Fornecedore
    {
        public Fornecedore()
        {
            Notasfiscais = new HashSet<Notasfiscai>();
        }

        public int Id { get; set; }
        public int Cliente { get; set; }
        public string Nome { get; set; }
        public string Cnpj { get; set; }
        public bool Ativo { get; set; }
        public int? Migrateid { get; set; }
        
        /// <summary>
        /// Indica se este fornecedor é destinador de resíduos (para protocolos de descarte)
        /// </summary>
        public bool DestinadorResiduos { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual ICollection<Notasfiscai> Notasfiscais { get; set; }
    }
}
