using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Template
    {
        public int Id { get; set; }
        public int Tipo { get; set; }
        public int Cliente { get; set; }
        public string Titulo { get; set; }
        public string Conteudo { get; set; }
        public bool Ativo { get; set; }
        public int Versao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAlteracao { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Templatetipo TipoNavigation { get; set; }
    }
}
