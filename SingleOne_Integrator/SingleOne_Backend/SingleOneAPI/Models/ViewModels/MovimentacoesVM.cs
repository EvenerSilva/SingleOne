using SingleOne.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Models.ViewModels
{
    public class MovimentacoesVM
    {
        public PagedResult<Colaboradorhistoricovm> StatusColaborador { get; set; }
        public PagedResult<Colaboradorhistoricovm> EmpresaColaborador { get; set; }
        public PagedResult<Colaboradorhistoricovm> LocalidadeColaborador { get; set; }
        public PagedResult<Colaboradorhistoricovm> CentroCustoColaborador { get; set; }
    }
}
