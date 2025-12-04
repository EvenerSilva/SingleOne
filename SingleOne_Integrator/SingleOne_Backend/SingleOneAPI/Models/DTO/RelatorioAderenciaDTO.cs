using System;
using System.Collections.Generic;

namespace SingleOneAPI.Models.DTO
{
    public class RelatorioAderenciaDTO
    {
        public int CampanhaId { get; set; }
        public string CampanhaNome { get; set; }
        public DateTime DataCriacao { get; set; }
        public int TotalColaboradores { get; set; }
        public int TotalEnviados { get; set; }
        public int TotalAssinados { get; set; }
        public int TotalPendentes { get; set; }
        public int TotalRecusados { get; set; }
        public decimal PercentualAdesao { get; set; }
        public decimal PercentualPendente { get; set; }
        public decimal PercentualRecusado { get; set; }
        
        // Estatísticas por Empresa
        public List<AderenciaPorEmpresaDTO> AderenciaPorEmpresa { get; set; }
        
        // Estatísticas por Localidade
        public List<AderenciaPorLocalidadeDTO> AderenciaPorLocalidade { get; set; }
        
        // Estatísticas por Tipo de Colaborador
        public List<AderenciaPorTipoDTO> AderenciaPorTipo { get; set; }
        
        // Timeline de envios
        public List<EnvioPorDiaDTO> TimelineEnvios { get; set; }
    }
    
    public class AderenciaPorEmpresaDTO
    {
        public string EmpresaNome { get; set; }
        public int Total { get; set; }
        public int Assinados { get; set; }
        public int Pendentes { get; set; }
        public decimal PercentualAdesao { get; set; }
    }
    
    public class AderenciaPorLocalidadeDTO
    {
        public string LocalidadeNome { get; set; }
        public int Total { get; set; }
        public int Assinados { get; set; }
        public int Pendentes { get; set; }
        public decimal PercentualAdesao { get; set; }
    }
    
    public class AderenciaPorTipoDTO
    {
        public string TipoColaborador { get; set; }
        public int Total { get; set; }
        public int Assinados { get; set; }
        public int Pendentes { get; set; }
        public decimal PercentualAdesao { get; set; }
    }
    
    public class EnvioPorDiaDTO
    {
        public DateTime Data { get; set; }
        public int TotalEnvios { get; set; }
        public int TotalAssinaturas { get; set; }
    }
}

