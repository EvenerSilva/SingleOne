using System;
using System.Collections.Generic;

namespace SingleOne.Models.ViewModels
{
    /// <summary>
    /// ViewModel para filtros de consulta de logs de acesso
    /// </summary>
    public class LogAcessoFiltroVM
    {
        public string DataInicio { get; set; }
        public string DataFim { get; set; }
        public string TipoAcesso { get; set; } // "passcheck" ou "patrimonio"
        public string CpfConsultado { get; set; }
        public int ClienteId { get; set; }
    }

    /// <summary>
    /// ViewModel para exibição de logs de acesso
    /// </summary>
    public class LogAcessoVM
    {
        public int Id { get; set; }
        public string TipoAcesso { get; set; }
        public int? ColaboradorId { get; set; }
        public string ColaboradorNome { get; set; }
        public string CpfConsultado { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string DadosConsultados { get; set; }
        public bool Sucesso { get; set; }
        public string MensagemErro { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
