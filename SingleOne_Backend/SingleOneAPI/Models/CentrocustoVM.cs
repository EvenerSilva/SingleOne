using System;

namespace SingleOneAPI.Models
{
    public class CentrocustoVM
    {
        public int Id { get; set; }
        public string Empresa { get; set; }
        public int EmpresaId { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public bool Ativo { get; set; } = true; // ✅ ADICIONADO: Campo Ativo com valor padrão true
        public DateTime? CreatedAt { get; set; } // ✅ ADICIONADO: Campo CreatedAt
        public DateTime? UpdatedAt { get; set; } // ✅ ADICIONADO: Campo UpdatedAt
    }
}
