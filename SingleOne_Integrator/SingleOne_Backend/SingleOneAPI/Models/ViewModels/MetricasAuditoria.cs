using System.Collections.Generic;

namespace SingleOneAPI.Models.ViewModels
{
    /// <summary>
    /// Métricas consolidadas da Auditoria de Acessos
    /// </summary>
    public class MetricasAuditoria
    {
        public int AcessosHoje { get; set; }
        public int AcessosOntem { get; set; }
        public int TentativasFalhas { get; set; }
        public int UsuariosAtivosHoje { get; set; }
        public int ConsultasCPFHoje { get; set; }
        
        // Histórico de acessos (valores diários)
        public List<int> AcessosUltimos7Dias { get; set; }
        
        // Top usuários mais ativos
        public List<UsuarioAtivo> TopUsuariosAtivos { get; set; }
        
        public MetricasAuditoria()
        {
            AcessosUltimos7Dias = new List<int>();
            TopUsuariosAtivos = new List<UsuarioAtivo>();
        }
    }
    
    /// <summary>
    /// Informações de um usuário ativo
    /// </summary>
    public class UsuarioAtivo
    {
        public string Nome { get; set; }
        public int Acessos { get; set; }
    }
}

