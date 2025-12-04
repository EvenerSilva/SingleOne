using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    /// <summary>
    /// ViewModel para representar um nó da árvore hierárquica do Mapa de Recursos
    /// </summary>
    public class MapaRecursosNoVM
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Tipo { get; set; } // "empresa", "localidade", "filial", "centroCusto", "colaborador", "recurso"
        public string Descricao { get; set; }
        public int? ParentId { get; set; }
        public int TotalColaboradores { get; set; }
        public int TotalRecursos { get; set; }
        public int TotalRecursosEntregues { get; set; }
        public int TotalRecursosDisponiveis { get; set; }
        public string Icone { get; set; }
        public string Cor { get; set; }
        public bool TemFilhos { get; set; }
        public bool IsExpandido { get; set; }
        public List<MapaRecursosNoVM> Filhos { get; set; }
        public Dictionary<string, object> Metadados { get; set; } // Dados extras específicos por tipo

        public MapaRecursosNoVM()
        {
            Filhos = new List<MapaRecursosNoVM>();
            Metadados = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// ViewModel para representar um recurso individual no mapa
    /// </summary>
    public class MapaRecursoDetalheVM
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public string Modelo { get; set; }
        public string NumeroSerie { get; set; }
        public string Patrimonio { get; set; }
        public string Status { get; set; }
        public string StatusDescricao { get; set; }
        public DateTime? DataEntrega { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public string Cor { get; set; }
        public string Icone { get; set; }
    }

    /// <summary>
    /// ViewModel para representar um colaborador no mapa
    /// </summary>
    public class MapaColaboradorVM
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cpf { get; set; }
        public string Cargo { get; set; }
        public string TipoColaborador { get; set; }
        public string Empresa { get; set; }
        public string Localidade { get; set; }
        public string Filial { get; set; }
        public string CentroCusto { get; set; }
        public int TotalRecursos { get; set; }
        public int TotalRecursosAtivos { get; set; }
        public List<MapaRecursoDetalheVM> Recursos { get; set; }
        public List<MapaRecursoDetalheVM> HistoricoRecursos { get; set; }

        public MapaColaboradorVM()
        {
            Recursos = new List<MapaRecursoDetalheVM>();
            HistoricoRecursos = new List<MapaRecursoDetalheVM>();
        }
    }

    /// <summary>
    /// ViewModel principal para o Mapa de Recursos
    /// </summary>
    public class MapaRecursosVM
    {
        public MapaRecursosNoVM RaizHierarquia { get; set; }
        public List<MapaColaboradorVM> Colaboradores { get; set; }
        public List<MapaColaboradorVM> ColaboradoresSemRecursos { get; set; }
        public MapaRecursosMetricasVM Metricas { get; set; }

        public MapaRecursosVM()
        {
            Colaboradores = new List<MapaColaboradorVM>();
            ColaboradoresSemRecursos = new List<MapaColaboradorVM>();
            Metricas = new MapaRecursosMetricasVM();
        }
    }

    /// <summary>
    /// ViewModel para métricas agregadas do Mapa de Recursos
    /// </summary>
    public class MapaRecursosMetricasVM
    {
        public int TotalEmpresas { get; set; }
        public int TotalLocalidades { get; set; }
        public int TotalFiliais { get; set; }
        public int TotalCentrosCusto { get; set; }
        public int TotalColaboradores { get; set; }
        public int TotalColaboradoresComRecursos { get; set; }
        public int TotalColaboradoresSemRecursos { get; set; }
        public int TotalRecursos { get; set; }
        public int TotalRecursosEntregues { get; set; }
        public int TotalRecursosDisponiveis { get; set; }
        public decimal MediaRecursosPorColaborador { get; set; }
        public MapaColaboradorVM ColaboradorComMaisRecursos { get; set; }
        public string CentroCustoComMaisRecursos { get; set; }
    }

    /// <summary>
    /// ViewModel para filtros do Mapa de Recursos
    /// </summary>
    public class MapaRecursosFiltroVM
    {
        public int ClienteId { get; set; }
        public int? EmpresaId { get; set; }
        public int? LocalidadeId { get; set; }
        public int? FilialId { get; set; }
        public int? CentroCustoId { get; set; }
        public bool IncluirColaboradoresSemRecursos { get; set; }
        public bool IncluirHistoricoRecursos { get; set; }
        public string NivelDrilldown { get; set; } // "empresa", "localidade", "filial", "centroCusto", "colaboradores"
    }
}

