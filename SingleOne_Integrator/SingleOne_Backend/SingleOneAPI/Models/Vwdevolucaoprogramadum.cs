using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingleOne.Models
{
    public partial class Vwdevolucaoprogramadum
    {
        // ⚠️ ATENÇÃO: A view vwdevolucaoprogramada no banco possui APENAS 3 colunas:
        // - cliente (integer)
        // - nomecolaborador (character varying(300))
        // - dtprogramadaretorno (timestamp without time zone)
        // 
        // 🆕 Os campos adicionais abaixo são populados via LINQ no RelatorioNegocio.cs
        
        public int? Cliente { get; set; }
        public string Nomecolaborador { get; set; }
        public DateTime? Dtprogramadaretorno { get; set; }
        
        // 🆕 Campos adicionais enriquecidos (não estão na view do banco)
        [NotMapped]
        public string Matricula { get; set; }
        
        [NotMapped]
        public int? ColaboradorId { get; set; }
        
        [NotMapped]
        public string Equipamento { get; set; }
        
        [NotMapped]
        public string Serial { get; set; }
        
        [NotMapped]
        public string Patrimonio { get; set; }
        
        [NotMapped]
        public int? EquipamentoId { get; set; }
        
        [NotMapped]
        public int? RequisicaoId { get; set; }
        
        [NotMapped]
        public int? RequisicoesItemId { get; set; }
    }
}
