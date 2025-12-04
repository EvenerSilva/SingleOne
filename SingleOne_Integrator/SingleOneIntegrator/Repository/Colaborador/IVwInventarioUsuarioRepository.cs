using SingleOneIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace SingleOneIntegrator.Repository.Colaborador
{
    public interface IVwInventarioUsuarioRepository : IRepositoryBase<VwInventarioUsuario>
    {
        IEnumerable<VwInventarioUsuario> GetByQuery(string sqlQuery);
    }
}
