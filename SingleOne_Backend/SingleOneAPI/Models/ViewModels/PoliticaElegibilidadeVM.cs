using System;

namespace SingleOneAPI.Models.ViewModels
{
    public class PoliticaElegibilidadeVM
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string TipoColaborador { get; set; } = string.Empty;
        public string TipoColaboradorDescricao { get; set; } = string.Empty;
        public string? Cargo { get; set; }
        public bool UsarPadrao { get; set; } = true; // Se true, usa LIKE '%cargo%'; se false, usa match exato
        public int TipoEquipamentoId { get; set; }
        public string TipoEquipamentoDescricao { get; set; } = string.Empty;
        public bool PermiteAcesso { get; set; }
        public int? QuantidadeMaxima { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; }
        public DateTime? DtCadastro { get; set; }
        public DateTime? DtAtualizacao { get; set; }
        public int? UsuarioCadastro { get; set; }
        public string? UsuarioCadastroNome { get; set; }
    }

    public class PoliticaElegibilidadeFiltroVM
    {
        public int Cliente { get; set; }
        public string? TipoColaborador { get; set; }
        public int? TipoEquipamentoId { get; set; }
        public bool? Ativo { get; set; }
    }
}

