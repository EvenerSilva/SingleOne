using System.Data;

namespace SingleOneIntegrator.Data
{
    internal interface IDbProvider
    {
        IDbConnection GetDbConnection(string connectionString);
    }
}
