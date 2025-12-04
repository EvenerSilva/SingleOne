using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    /// <summary>
    /// Métricas consolidadas das Sinalizações de Suspeitas
    /// </summary>
    public class MetricasSinalizacoes
    {
        // Totais por status
        public int Total { get; set; }
        public int Pendentes { get; set; }
        public int EmInvestigacao { get; set; }
        public int Resolvidas { get; set; }
        public int ResolvidasHoje { get; set; }
        public int Arquivadas { get; set; }
        
        // Totais por prioridade
        public int Criticas { get; set; }
        public int Altas { get; set; }
        public int Medias { get; set; }
        public int Baixas { get; set; }
        
        // Histórico (valores diários)
        public List<int> Ultimos7Dias { get; set; }
        public List<int> Ultimos30Dias { get; set; }
        
        // Alertas (situações que requerem atenção)
        public int PendentesHaMaisDe7Dias { get; set; }
        public int CriticasNaoAtendidas { get; set; }
        
        public MetricasSinalizacoes()
        {
            Ultimos7Dias = new List<int>();
            Ultimos30Dias = new List<int>();
        }
    }
}

