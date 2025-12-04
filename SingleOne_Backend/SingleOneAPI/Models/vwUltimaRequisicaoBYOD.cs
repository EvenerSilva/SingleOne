using System;

namespace SingleOne.Models
{
    public class VwUltimasRequisicaoBYOD
    {
        // Propriedades da tabela Requisicoes
        public int RequisicaoId { get; set; }
        public int Cliente { get; set; }
        public int UsuarioRequisicao { get; set; }
        public int TecnicoResponsavel { get; set; }
        public int RequisicaoStatus { get; set; }
        public int? ColaboradorFinal { get; set; }
        public string NomeColaboradorFinal { get; set; }
        public DateTime DtSolicitacao { get; set; }
        public DateTime? DtProcessamento { get; set; }
        public bool AssinaturaEletronica { get; set; }
        public DateTime? DtAssinaturaEletronica { get; set; }
        public DateTime? DtEnvioTermo { get; set; }
        public string HashRequisicao { get; set; }

        // Propriedades da tabela RequisicoesItens
        public int RequisicaoItemId { get; set; }
        public int Equipamento { get; set; }
        public int? LinhaTelefonica { get; set; }
        public int? UsuarioEntrega { get; set; }
        public int? UsuarioDevolucao { get; set; }
        public DateTime? DtEntrega { get; set; }
        public DateTime? DtDevolucao { get; set; }
        public string ObservacaoEntrega { get; set; }
        public DateTime? DtProgramadaRetorno { get; set; }

        // Propriedades da tabela Equipamentos
        public int EquipamentoId { get; set; }
        public int TipoAquisicao { get; set; }
        public int EquipamentoStatus { get; set; }
        public string NumeroSerie { get; set; }
        public string Patrimonio { get; set; }
    }
}
