using System;

namespace SingleOne.Models.ViewModels
{
    /// <summary>
    /// ViewModel para filtros de consulta de garantias
    /// </summary>
    public class GarantiaFiltroVM
    {
        public string StatusGarantia { get; set; } // "expiradas", "vence30", "vence90", "vence180", "vigentes", "naoInformado"
        public string TipoEquipamento { get; set; }
        public string Fabricante { get; set; }
        public string Patrimonio { get; set; }
        public int ClienteId { get; set; }
    }

    /// <summary>
    /// ViewModel para exibição de garantias de equipamentos
    /// </summary>
    public class GarantiaVM
    {
        public int Id { get; set; }
        public string Patrimonio { get; set; }
        public string TipoEquipamento { get; set; }
        public string Fabricante { get; set; }
        public string Modelo { get; set; }
        public string NumeroSerie { get; set; }
        public DateTime? DataGarantia { get; set; }
        public int? DiasRestantes { get; set; }
        public string StatusGarantia { get; set; } // "expiradas", "vence30", "vence90", "vence180", "vigentes", "naoInformado"
    }
}
