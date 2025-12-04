using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Models.ViewModels
{
    public class TransferenciaEqpVM
    {
        public int RequisicaoID { get; set; }
        public int EquipamentoID { get; set; }
        public int ColaboradorDestinoID { get; set; }
        public int Usuario { get; set; }
    }
}
