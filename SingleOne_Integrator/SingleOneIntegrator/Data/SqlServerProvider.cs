using System.Data;
using System.Data.SqlClient;

namespace SingleOneIntegrator.Data
{
    internal class SqlServerProvider : IDbProvider
    {
        public IDbConnection GetDbConnection(string connectionString)
            => new SqlConnection(connectionString);
    }
}
