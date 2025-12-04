using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.DTO
{
    /// <summary>
    /// DTO para autenticação do Meu Patrimônio
    /// </summary>
    public class MeuPatrimonioAuthDTO
    {
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta do Meu Patrimônio
    /// </summary>
    public class MeuPatrimonioResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public MeuPatrimonioColaboradorDTO? Colaborador { get; set; }
        public List<MeuPatrimonioEquipamentoDTO> Equipamentos { get; set; } = new List<MeuPatrimonioEquipamentoDTO>();
        public List<MeuPatrimonioContestacaoDTO> Contestoes { get; set; } = new List<MeuPatrimonioContestacaoDTO>();
    }

    /// <summary>
    /// DTO para dados do colaborador no Meu Patrimônio
    /// </summary>
    public class MeuPatrimonioColaboradorDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public string Situacao { get; set; } = string.Empty;
        public DateTime DtAdmissao { get; set; }
    }

    /// <summary>
    /// DTO para equipamentos no Meu Patrimônio
    /// </summary>
    public class MeuPatrimonioEquipamentoDTO
    {
        public int Id { get; set; }
        public string Patrimonio { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string TipoEquipamento { get; set; } = string.Empty;
        public bool TipoEquipamentoTransitoLivre { get; set; }
        public string Fabricante { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DtEntrega { get; set; }
        public DateTime? DtDevolucao { get; set; }
        public string Observacao { get; set; } = string.Empty;
        public bool PodeContestar { get; set; } = true;
        public bool TemContestacao { get; set; } = false;
        public int? ContestacaoId { get; set; }
        public string ContestacaoStatus { get; set; } = string.Empty;
        public string ContestacaoData { get; set; } = string.Empty;
        public string ContestacaoMotivo { get; set; } = string.Empty;
        public bool Assinado { get; set; } = false;
        public DateTime? DataAssinatura { get; set; }
        public string HashRequisicao { get; set; } = string.Empty;
        public bool IsByod { get; set; } = false;
        public bool IsHistorico { get; set; } = false;
        public bool IsRecursoParticular { get; set; } = false;
        public string TipoAquisicao { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para contestação de patrimônio
    /// </summary>
    public class MeuPatrimonioContestacaoDTO
    {
        public int Id { get; set; }
        public int EquipamentoId { get; set; }
        public string EquipamentoPatrimonio { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string EvidenciaUrl { get; set; } = string.Empty;
        public DateTime DataContestacao { get; set; }
        public DateTime? DataResolucao { get; set; }
        public string ObservacaoResolucao { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para criar nova contestação
    /// </summary>
    public class CriarContestacaoDTO
    {
        public int ColaboradorId { get; set; }
        public int EquipamentoId { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string EvidenciaUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para cancelar contestação
    /// </summary>
    public class CancelarContestacaoDTO
    {
        public int ColaboradorId { get; set; }
        public int EquipamentoId { get; set; }
        public int? ContestacaoId { get; set; }
        public string? Justificativa { get; set; }
    }

    /// <summary>
    /// DTO para solicitar devolução
    /// </summary>
    public class SolicitarDevolucaoDTO
    {
        public int ColaboradorId { get; set; }
        public int EquipamentoId { get; set; }
        public string Justificativa { get; set; } = string.Empty;
        public DateTime DataDesejada { get; set; }
    }
}
