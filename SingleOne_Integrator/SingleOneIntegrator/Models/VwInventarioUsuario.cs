using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Models
{
    public class VwInventarioUsuario
    {
        public string NomeCompleto { get; set; }
        public string NomeDeUsuario { get; set; }
        public string CentroDeCusto { get; set; }
        public string TxtCentroDeCusto { get; set; }
        public string Cargo { get; set; }
        public string Matricula { get; set; }
        public DateTime? DataDeAdmissao { get; set; }
        public DateTime? DataDeDemissao { get; set; }
        public string Empresa { get; set; }
        public string Cpf { get; set; }
        public string Cnpj { get; set; }
        public string TipoDeColaborador { get; set; }
        public string Status { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string NomeFantasia { get; set; }
        public string EmailCorporativo { get; set; }
        public string Superior { get; set; }
    }
}
