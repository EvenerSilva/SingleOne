using MySql.Data.MySqlClient;
using System.Data;

namespace SingleOneIntegrator.Data
{
    public class MySqlProvider : IDbProvider
    {
        public IDbConnection GetDbConnection(string connectionString)
            => new MySqlConnection(connectionString);
    }
}