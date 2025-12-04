using System;

namespace SingleOneAPI.Models.DTO
{
    public class UltimaRequisicaoDTO
    {
        // Propriedades da tabela Requisicoes
        public int RequisicaoId { get; set; }
        public int UsuarioRequisicao { get; set; }
        public int TecnicoResponsavel { get; set; }
        public DateTime DtSolicitacao { get; set; }
        public DateTime? DtProcessamento { get; set; }
        public bool AssinaturaEletronica { get; set; }
    }
}
