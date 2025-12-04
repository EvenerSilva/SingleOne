using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    /// <summary>
    /// DTO para KPI (Indicador Chave de Performance) com comparação de períodos
    /// </summary>
    public class KPIPrincipal
    {
        /// <summary>
        /// Valor atual do KPI
        /// </summary>
        public int Valor { get; set; }
        
        /// <summary>
        /// Valor do período anterior para comparação
        /// </summary>
        public int ValorAnterior { get; set; }
        
        /// <summary>
        /// Variação em relação ao período anterior (percentual ou absoluto)
        /// </summary>
        public decimal Variacao { get; set; }
        
        /// <summary>
        /// Tipo de variação: "percentual" ou "absoluto"
        /// </summary>
        public string TipoVariacao { get; set; }
        
        /// <summary>
        /// Tendência: "alta", "baixa" ou "estavel"
        /// </summary>
        public string Tendencia { get; set; }
        
        /// <summary>
        /// Valores dos últimos 7 dias para criar mini gráfico (sparkline)
        /// </summary>
        public List<int> SparklineUltimos7Dias { get; set; }
        
        public KPIPrincipal()
        {
            TipoVariacao = "percentual";
            Tendencia = "estavel";
            SparklineUltimos7Dias = new List<int>();
        }
    }
}

