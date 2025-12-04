using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    public class RelatorioNaoConformidadeVM
    {
        // Informações do Colaborador
        public int ColaboradorId { get; set; }
        public string ColaboradorNome { get; set; } = string.Empty;
        public string ColaboradorCpf { get; set; } = string.Empty;
        public string ColaboradorEmail { get; set; } = string.Empty;
        public string ColaboradorCargo { get; set; } = string.Empty;
        public string TipoColaborador { get; set; } = string.Empty;
        public string TipoColaboradorDescricao { get; set; } = string.Empty;
        
        // Informações da Empresa/Localização
        public string EmpresaNome { get; set; } = string.Empty;
        public string CentroCusto { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        
        // Informações do Equipamento
        public int EquipamentoId { get; set; }
        public string EquipamentoPatrimonio { get; set; } = string.Empty;
        public string EquipamentoSerie { get; set; } = string.Empty;
        public int TipoEquipamentoId { get; set; }
        public string TipoEquipamentoDescricao { get; set; } = string.Empty;
        public string? CategoriaEquipamento { get; set; }
        public string? Fabricante { get; set; }
        public string? Modelo { get; set; }
        public string? EquipamentoStatus { get; set; }
        
        // Informações da Política
        public int? PoliticaId { get; set; }
        public bool PermiteAcesso { get; set; }
        public int? QuantidadeMaxima { get; set; }
        public string? PoliticaObservacoes { get; set; }
        public int QuantidadeAtual { get; set; }
        
        // Motivo da Não Conformidade
        public string MotivoNaoConformidade { get; set; } = string.Empty;
        
        // Data de Geração do Relatório
        public DateTime DtGeracaoRelatorio { get; set; }
    }

    public class RelatorioNaoConformidadeFiltroVM
    {
        public int Cliente { get; set; }
        public string? TipoColaborador { get; set; }
        public int? TipoEquipamentoId { get; set; }
        public int? EmpresaId { get; set; }
        public int? CentroCustoId { get; set; }
        public string? ColaboradorNome { get; set; }
    }

    public class RelatorioNaoConformidadeResultVM
    {
        public List<RelatorioNaoConformidadeVM> Registros { get; set; } = new List<RelatorioNaoConformidadeVM>();
        public int TotalRegistros { get; set; }
        public int TotalColaboradores { get; set; }
        public int TotalEquipamentos { get; set; }
        public Dictionary<string, int> PorTipoColaborador { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PorTipoEquipamento { get; set; } = new Dictionary<string, int>();
    }
}

