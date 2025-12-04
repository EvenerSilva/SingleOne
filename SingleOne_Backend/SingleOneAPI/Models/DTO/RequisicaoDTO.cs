using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.DTO
{
    public class RequisicaoDTO
    {
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
        public DateTime? Dtenviotermo { get; set; }
        public string Hashrequisicao { get; set; }
        public int? Migrateid { get; set; }
        public List<RequisicaoItemDTO> Requisicoesitens { get; set; } = new List<RequisicaoItemDTO>();
    }

    public class RequisicaoItemDTO
    {
        public int Id { get; set; }
        public int Requisicao { get; set; }
        public int? Equipamento { get; set; }
        public int? Linhatelefonica { get; set; }
        public int? Usuarioentrega { get; set; }
        public int? Usuariodevolucao { get; set; }
        public DateTime? Dtentrega { get; set; }
        public DateTime? Dtdevolucao { get; set; }
        public string Observacaoentrega { get; set; }
        public DateTime? Dtprogramadaretorno { get; set; }

        // Co-responsáveis informados no ato da entrega
        public List<CoResponsavelDTO> CoResponsaveis { get; set; } = new List<CoResponsavelDTO>();
    }

    public class CoResponsavelDTO
    {
        public int ColaboradorId { get; set; }
        public string TipoAcesso { get; set; } = "usuario_compartilhado";
        public DateTime? DataFim { get; set; }
        public string Observacao { get; set; }
    }
}
