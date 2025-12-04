using Dapper;
using Dapper.Contrib.Extensions;
using SingleOneIntegrator.Data;
using SingleOneIntegrator.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Repository
{
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
    {
        protected IDbConnection DbConnection { get; private set; }
        private readonly DatabaseOptions _dbOptions;

        public RepositoryBase(DatabaseOptions dbOptions)
        {
            _dbOptions = dbOptions;
            DbConnection = new DbContext(_dbOptions.ProviderName)
                .GetDbContext(_dbOptions.ConnectionString);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync()
        {
            DbConnection.Open();

            try
            {
                var results = await DbConnection
                    .GetAllAsync<TEntity>();

                return results
                    .AsEnumerable();
            }
            finally { DbConnection.Close(); }
        }

        public async Task<TEntity> FindByIdAsync(object id)
        {
            DbConnection.Open();

            try
            {
                return await DbConnection
                    .GetAsync<TEntity>(id);
            }
            finally { DbConnection.Close(); }
        }

        public async Task<IEnumerable<TEntity>> FindByQueryAsync(string sqlQuery)
        {
            DbConnection.Open();

            try
            {
                var results = await DbConnection
                    .QueryAsync<TEntity>(sqlQuery);

                return results
                    .AsEnumerable();
            }
            finally { DbConnection.Close(); }
        }
    }
}
