using System.Collections.Generic;
using SingleOne.Models;

namespace SingleOneAPI.Models
{
    public partial class Cliente
    {
        public Cliente()
        {
            Colaboradores = new HashSet<Colaboradore>();
            Descartecargos = new HashSet<Descartecargo>();
            Empresas = new HashSet<Empresa>();
            Equipamentos = new HashSet<Equipamento>();
            Fabricantes = new HashSet<Fabricante>();
            Fornecedores = new HashSet<Fornecedore>();
            Laudos = new HashSet<Laudo>();
            Localidades = new HashSet<Localidade>();
            Modelos = new HashSet<Modelo>();
            Notasfiscais = new HashSet<Notasfiscai>();
            Parametros = new HashSet<Parametro>();
            Requisicos = new HashSet<Requisico>();
            Telefoniacontratos = new HashSet<Telefoniacontrato>();
            Templates = new HashSet<Template>();
            Tipoequipamentosclientes = new HashSet<Tipoequipamentoscliente>();
            Usuarios = new HashSet<Usuario>();
        }

        public int Id { get; set; }
        public string Razaosocial { get; set; }
        public string Cnpj { get; set; }
        public bool Ativo { get; set; }
        public string Logo { get; set; }
        public string SiteUrl { get; set; }

        public virtual ICollection<Colaboradore> Colaboradores { get; set; }
        public virtual ICollection<Descartecargo> Descartecargos { get; set; }
        public virtual ICollection<CargoConfianca> CargosConfianca { get; set; }
        public virtual ICollection<Empresa> Empresas { get; set; }
        public virtual ICollection<Equipamento> Equipamentos { get; set; }
        public virtual ICollection<Fabricante> Fabricantes { get; set; }
        public virtual ICollection<Fornecedore> Fornecedores { get; set; }
        public virtual ICollection<Laudo> Laudos { get; set; }
        public virtual ICollection<Localidade> Localidades { get; set; }
        public virtual ICollection<Modelo> Modelos { get; set; }
        public virtual ICollection<Notasfiscai> Notasfiscais { get; set; }
        public virtual ICollection<Parametro> Parametros { get; set; }
        public virtual ICollection<Requisico> Requisicos { get; set; }
        public virtual ICollection<Telefoniacontrato> Telefoniacontratos { get; set; }
        public virtual ICollection<Template> Templates { get; set; }
        public virtual ICollection<Tipoequipamentoscliente> Tipoequipamentosclientes { get; set; }
        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
