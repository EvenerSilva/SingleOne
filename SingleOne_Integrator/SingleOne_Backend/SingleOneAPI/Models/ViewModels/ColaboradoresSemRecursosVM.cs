using System;

namespace SingleOne.Models.ViewModels
{
    /// <summary>
    /// ViewModel para filtros de consulta de colaboradores sem recursos
    /// </summary>
    public class ColaboradoresSemRecursosFiltroVM
    {
        public string Cargo { get; set; } // Campo de texto livre
        public string TipoColaborador { get; set; } // 'F' (Funcionário), 'T' (Terceiro), 'C' (Consultor)
        public int? Empresa { get; set; }
        public int? Localidade { get; set; }
        public int? CentroCusto { get; set; }
        public string Nome { get; set; }
        public int ClienteId { get; set; }
    }

    /// <summary>
    /// ViewModel para exibição de colaboradores sem recursos
    /// </summary>
    public class ColaboradoresSemRecursosVM
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Matricula { get; set; }
        public string CargoDescricao { get; set; } // Campo de texto livre
        public int? EmpresaId { get; set; }
        public string EmpresaDescricao { get; set; }
        public int? LocalidadeId { get; set; }
        public string LocalidadeDescricao { get; set; }
        public int? CentroCustoId { get; set; }
        public string CentroCustoDescricao { get; set; }
        public char TipoColaboradorId { get; set; } // 'F' (Funcionário), 'T' (Terceiro), 'C' (Consultor)
        public string TipoColaboradorDescricao { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public DateTime? DataDemissao { get; set; } // 🚫 Data de demissão/desligamento
    }
}

