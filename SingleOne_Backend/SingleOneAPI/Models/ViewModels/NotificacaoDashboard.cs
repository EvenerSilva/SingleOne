using System;

namespace SingleOneAPI.Models.ViewModels
{
    /// <summary>
    /// Notificação/Alerta do Dashboard
    /// </summary>
    public class NotificacaoDashboard
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Tipo: "critico", "atencao" ou "info"
        /// </summary>
        public string Tipo { get; set; }
        
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public DateTime DataHora { get; set; }
        public bool Lida { get; set; }
        
        /// <summary>
        /// URL para navegar ao clicar na notificação
        /// </summary>
        public string Link { get; set; }
        
        /// <summary>
        /// Ícone do Material Icons
        /// </summary>
        public string Icone { get; set; }
        
        public NotificacaoDashboard()
        {
            DataHora = DateTime.Now;
            Lida = false;
        }
    }
}

