using SingleOneIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace SingleOneIntegrator.Helpers
{
    public class VwInventarioUsuarioComparer : IEqualityComparer<VwInventarioUsuario>
    {
        public bool Equals(VwInventarioUsuario x, VwInventarioUsuario y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Cpf == y.Cpf && 
                x.Status == y.Status && 
                x.Empresa == y.Empresa && 
                x.CentroDeCusto == y.CentroDeCusto && 
                x.Cidade == y.Cidade &&
                x.Estado == y.Estado;
        }

        public int GetHashCode(VwInventarioUsuario obj)
        {
            int hash = 17;
            hash = hash * 23 + obj.Cpf.GetHashCode();
            hash = hash * 23 + (obj.Status?.GetHashCode() ?? 0);
            hash = hash * 23 + obj.Cargo.GetHashCode();
            hash = hash * 23 + obj.Empresa.GetHashCode();
            hash = hash * 23 + obj.CentroDeCusto.GetHashCode();
            hash = hash * 23 + obj.Cidade.GetHashCode();
            hash = hash * 23 + obj.Estado.GetHashCode();
            return hash;
        }
    }
}
