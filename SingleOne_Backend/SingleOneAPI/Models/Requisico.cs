using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models
{
    public partial class Requisico : ICloneable
    {
        public Requisico()
        {
            Requisicoesitens = new HashSet<Requisicoesiten>();
        }

        public int Id { get; set; }
        public int Cliente { get; set; }
        public int Usuariorequisicao { get; set; }
        public int Tecnicoresponsavel { get; set; }
        public int Requisicaostatus { get; set; }
        public int? Colaboradorfinal { get; set; }
        public DateTime? Dtsolicitacao { get; set; }
        public DateTime? Dtprocessamento { get; set; }
        public bool Assinaturaeletronica { get; set; }
        public DateTime? Dtassinaturaeletronica { get; set; }
        public string ConteudoTemplateAssinado { get; set; }
        public int? TipoTermoAssinado { get; set; }
        public int? VersaoTemplateAssinado { get; set; }
        public DateTime? Dtenviotermo { get; set; }
        public string Hashrequisicao { get; set; } = string.Empty;
        public int? Migrateid { get; set; }

        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Requisicoesstatus RequisicaostatusNavigation { get; set; }
        public virtual Usuario TecnicoresponsavelNavigation { get; set; }
        public virtual Usuario UsuariorequisicaoNavigation { get; set; }
        public virtual ICollection<Requisicoesiten> Requisicoesitens { get; set; }

        public object Clone()
        {
            // Serializa o objeto em JSON
            string json = JsonConvert.SerializeObject(this);

            // Desserializa o JSON em um novo objeto
            return JsonConvert.DeserializeObject<Requisico>(json);
        }
    }
}
