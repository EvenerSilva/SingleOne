namespace SingleOneAPI.Models.TinOne
{
    /// <summary>
    /// Configuração do assistente TinOne
    /// Obtida através dos parâmetros do sistema
    /// </summary>
    public class TinOneConfig
    {
        public bool Habilitado { get; set; }
        public bool ChatHabilitado { get; set; }
        public bool TooltipsHabilitado { get; set; }
        public bool GuiasHabilitado { get; set; }
        public bool SugestoesProativas { get; set; }
        public bool IaHabilitada { get; set; }
        public bool Analytics { get; set; }
        public bool DebugMode { get; set; }
        public string Posicao { get; set; } = "bottom-right";
        public string CorPrimaria { get; set; } = "#4a90e2";
    }
}

