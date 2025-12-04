using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Models.ViewModels
{
    public class DashboardMobileVM
    {
        public List<TotalEquipamentosVM> TotalEquipamentos { get; set; }
        public TotalLaudosVM TotalLaudos { get; set; }
    }
}
