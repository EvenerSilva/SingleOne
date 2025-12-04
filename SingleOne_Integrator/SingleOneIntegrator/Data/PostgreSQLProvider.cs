using Npgsql;
using System.Data;

namespace SingleOneIntegrator.Data
{
    internal class PostgreSQLProvider : IDbProvider
    {
        public IDbConnection GetDbConnection(string connectionString)
            => new NpgsqlConnection(connectionString);
    }
}
