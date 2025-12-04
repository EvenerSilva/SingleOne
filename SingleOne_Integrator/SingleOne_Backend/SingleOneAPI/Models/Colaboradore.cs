using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOneAPI.Models
{
    public partial class Colaboradore
    {
        public int Id { get; set; }
        
        // Campo cliente agora é opcional (herda da empresa automaticamente)
        public int? Cliente { get; set; }
        
        public int Usuario { get; set; }
        public int Empresa { get; set; }
        public int Centrocusto { get; set; }
        public int Localidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public DateTime Dtadmissao { get; set; }
        public char Tipocolaborador { get; set; }
        public DateTime? Dtcadastro { get; set; }
        public DateTime? Dtatualizacao { get; set; }
        public string Situacao { get; set; } = string.Empty;
        public int? Antigaempresa { get; set; }
        public int? Antigocentrocusto { get; set; }
        public int? Antigalocalidade { get; set; }
        
        // Campos opcionais para flexibilidade
        [Column("filial_id")]
        public int? FilialId { get; set; }
        
        [Column("localidade_id")]
        public int? LocalidadeId { get; set; }
        
        public string Matriculasuperior { get; set; } = string.Empty;
        public DateTime? Dtatualizacaolocalidade { get; set; }
        public DateTime? Dtatualizacaoempresa { get; set; }
        public DateTime? Dtatualizacaocentrocusto { get; set; }
        public char? Situacaoantiga { get; set; }
        
        [Column("migrateid")]
        public int? Migrateid { get; set; }
        
        public DateTime? Dtdemissao { get; set; }

        // Propriedades de navegação
        public virtual Centrocusto CentrocustoNavigation { get; set; } = null!;
        public virtual Cliente? ClienteNavigation { get; set; } // Agora nullable
        public virtual Empresa EmpresaNavigation { get; set; } = null!;
        public virtual Localidade? LocalidadeNavigation { get; set; } // Agora nullable
        public virtual Usuario UsuarioNavigation { get; set; } = null!;
        public virtual Filial? Filial { get; set; } // Agora nullable
    }
}
