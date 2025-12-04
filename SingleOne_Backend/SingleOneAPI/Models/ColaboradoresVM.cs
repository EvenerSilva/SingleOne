using System;

namespace SingleOneAPI.Models
{
    public class ColaboradoresVM
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public string Empresa { get; set; }
        public string NomeCentroCusto { get; set; }
        public string CodigoCentroCusto { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Matricula { get; set; }
        public string Email { get; set; }
        public string TipoColaborador { get; set; }
        public string Situacao { get; set; }
        public string Cargo { get; set; }
        public string Setor { get; set; }
        public string LocalidadeDescricao { get; set; }
        public string LocalidadeCidade { get; set; }
        public string LocalidadeEstado { get; set; }
        public DateTime? Dtadmissao { get; set; }
        public DateTime? Dtdemissao { get; set; }
        public DateTime? Dtcadastro { get; set; }
        public string MatriculaSuperior { get; set; }
    }
}
