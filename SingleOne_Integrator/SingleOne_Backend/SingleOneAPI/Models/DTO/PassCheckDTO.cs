using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.DTO
{
    /// <summary>
    /// DTO para consulta do PassCheck (Portal da Portaria)
    /// </summary>
    public class PassCheckDTO
    {
        public string Cpf { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta do PassCheck
    /// </summary>
    public class PassCheckResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public PassCheckColaboradorDTO? Colaborador { get; set; }
        public List<PassCheckEquipamentoDTO> Equipamentos { get; set; } = new List<PassCheckEquipamentoDTO>();
        public string StatusLiberacao { get; set; } = string.Empty; // "Liberado" ou "Pendências"
        public List<string> MotivosPendencia { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO para dados do colaborador no PassCheck
    /// </summary>
    public class PassCheckColaboradorDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
        public string EmpresaNome { get; set; } = string.Empty;
        public string CentroCusto { get; set; } = string.Empty;
        public string CentroCustoNome { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public string LocalidadeNome { get; set; } = string.Empty;
        public string Situacao { get; set; } = string.Empty;
        public DateTime DtAdmissao { get; set; }
        public DateTime? DtDemissao { get; set; }
        public string SuperiorImediato { get; set; } = string.Empty;
        public string SuperiorImediatoNome { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para equipamentos no PassCheck
    /// </summary>
    public class PassCheckEquipamentoDTO
    {
        public int Id { get; set; }
        public string Patrimonio { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string TipoEquipamento { get; set; } = string.Empty;
        public bool TipoEquipamentoTransitoLivre { get; set; } // Novo campo para trânsito livre
        public string Fabricante { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DtEntrega { get; set; }
        public string Observacao { get; set; } = string.Empty;
        public string TipoAquisicao { get; set; } = string.Empty; // ✅ NOVO: Tipo de aquisição
        public string CategoriaRecurso { get; set; } = string.Empty; // ✅ NOVO: Categoria do recurso
        public bool IsHistorico { get; set; } // ✅ NOVO: Se é histórico (inativo/devolvido)
        public bool IsRecursoParticular { get; set; } // ✅ NOVO: Se é recurso particular (BYOD)
    }
}
