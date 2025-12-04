using System;
using System.Collections.Generic;

namespace SingleOne.Models
{
    public partial class Requisicoesvm
    {
        public int? Id { get; set; }
        public int? Usuariorequisicaoid { get; set; }
        public string Usuariorequisicao { get; set; }
        public int? Tecnicoresponsavelid { get; set; }
        public string Tecnicoresponsavel { get; set; }
        public int? Colaboradorfinalid { get; set; }
        public string Colaboradorfinal { get; set; }
        public DateTime? Dtsolicitacao { get; set; }
        public DateTime? Dtprocessamento { get; set; }
        public int? Requisicaostatusid { get; set; }
        public string Requisicaostatus { get; set; }
        public bool? Assinaturaeletronica { get; set; }
        public DateTime? Dtassinaturaeletronica { get; set; }
        public DateTime? Dtenviotermo { get; set; }
        public string Hashrequisicao { get; set; }
        public long? Equipamentospendentes { get; set; }
        public int? Cliente { get; set; }
    }
}
