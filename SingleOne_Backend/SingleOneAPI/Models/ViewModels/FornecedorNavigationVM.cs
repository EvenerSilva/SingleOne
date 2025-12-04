using System;

namespace SingleOneAPI.Models.ViewModels
{
    public class FornecedorNavigationVM
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cnpj { get; set; }
        public int Cliente { get; set; }
        public bool Ativo { get; set; }
    }
}
