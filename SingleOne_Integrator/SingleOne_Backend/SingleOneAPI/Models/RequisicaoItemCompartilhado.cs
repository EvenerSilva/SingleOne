using System;

namespace SingleOneAPI.Models
{
    public class RequisicaoItemCompartilhado
    {
        public int Id { get; set; }

        public int RequisicaoItemId { get; set; }
        public int ColaboradorId { get; set; }

        public string TipoAcesso { get; set; } = "usuario_compartilhado";
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? Observacao { get; set; }
        public bool Ativo { get; set; }

        public int CriadoPor { get; set; }
        public DateTime CriadoEm { get; set; }

        // Navegações
        public virtual Requisicoesiten RequisicaoItem { get; set; } = null!;
        public virtual Colaboradore Colaborador { get; set; } = null!;
        public virtual Usuario CriadoPorUsuario { get; set; } = null!;
    }
}


