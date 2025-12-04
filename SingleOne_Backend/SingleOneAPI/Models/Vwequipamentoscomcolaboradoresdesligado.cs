using System;

namespace SingleOne.Models
{
    public partial class Vwequipamentoscomcolaboradoresdesligado
    {
        public int? Cliente { get; set; }
        public string Nome { get; set; }
        public string Matricula { get; set; }  // 🆕 Matrícula do colaborador
        public DateTime? Dtdemissao { get; set; }
        public long? Qtde { get; set; }
        public int? ColaboradorId { get; set; }
        public string Equipamento { get; set; }
        public int? EquipamentoId { get; set; }
    }
}
