using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Data
{
    public class DbContext
    {
        private IDbProvider _dbProvider;

        public DbContext(string providerType) => _dbProvider = providerType switch
        {
            "SQLServer" => _dbProvider = new SqlServerProvider(),
            "Npgsql" => _dbProvider = new PostgreSQLProvider(),
            "MySql" => _dbProvider = new MySqlProvider(),
            _ => null
        };

        public IDbConnection GetDbContext(string connectionString)
            => _dbProvider.GetDbConnection(connectionString);
    }
}
