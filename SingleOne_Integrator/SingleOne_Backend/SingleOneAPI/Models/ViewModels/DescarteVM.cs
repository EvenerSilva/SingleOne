using SingleOneAPI.Models;
using System.Collections.Generic;

namespace SingleOne.Models.ViewModels
{
    public class DescarteVM
    {
        public Vwexportacaoexcel Equipamento { get; set; }
        public Equipamentohistorico Historico { get; set; }
        public string Cargo { get; set; }
        public int UsuarioDescarte { get; set; }
        
        // Processos obrigatórios baseados nos cargos de confiança
        public bool ProcessosObrigatorios { get; set; }
        public bool ObrigarSanitizacao { get; set; }
        public bool ObrigarDescaracterizacao { get; set; }
        public bool ObrigarPerfuracaoDisco { get; set; }
        public bool ObrigarEvidencias { get; set; }
        
        // Processos que foram executados (marcados pelo usuário)
        public bool SanitizacaoExecutada { get; set; }
        public bool DescaracterizacaoExecutada { get; set; }
        public bool PerfuracaoDiscoExecutada { get; set; }
        public bool EvidenciasExecutadas { get; set; }
        
        // Lista de cargos de confiança que o equipamento passou
        public List<string> CargosConfiancaEncontrados { get; set; }
        public string NivelCriticidade { get; set; }
        
        // Evidências anexadas
        public List<DescarteEvidencia> Evidencias { get; set; }
        
        public DescarteVM()
        {
            CargosConfiancaEncontrados = new List<string>();
            Evidencias = new List<DescarteEvidencia>();
        }
    }
}
