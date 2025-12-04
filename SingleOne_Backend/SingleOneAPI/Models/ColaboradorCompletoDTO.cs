using System;

namespace SingleOneAPI.Models
{
    public class ColaboradorCompletoDTO
    {
        public int Id { get; set; }
        public int? Cliente { get; set; }
        public int Usuario { get; set; }
        
        // Empresa
        public string Empresa { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        
        // Centro de Custo
        public string NomeCentroCusto { get; set; } = string.Empty;
        public string CodigoCentroCusto { get; set; } = string.Empty;
        public int Centrocusto { get; set; }
        public int CentrocustoId { get; set; }
        
        // Localidade
        public string Localidade { get; set; } = string.Empty;
        public int LocalidadeId { get; set; }
        
        // Dados básicos
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Dados profissionais
        public string Cargo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public string Dtadmissao { get; set; } = string.Empty;
        public string Dtdemissao { get; set; } = string.Empty;
        public string Tipocolaborador { get; set; } = string.Empty;
        public string Situacao { get; set; } = string.Empty;
        
        // Dados adicionais
        public string Matriculasuperior { get; set; } = string.Empty;
        public int? FilialId { get; set; }
        public string Dtcadastro { get; set; } = string.Empty;
        public string Dtatualizacao { get; set; } = string.Empty;
        
        // Campos históricos
        public int? Antigaempresa { get; set; }
        public int? Antigocentrocusto { get; set; }
        public int? Antigalocalidade { get; set; }
        public string Situacaoantiga { get; set; } = string.Empty;
        public int? Migrateid { get; set; }
    }
}
