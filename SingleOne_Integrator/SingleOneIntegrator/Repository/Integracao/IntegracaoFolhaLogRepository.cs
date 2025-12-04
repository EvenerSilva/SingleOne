using Dapper;
using SingleOneIntegrator.Models;
using SingleOneIntegrator.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Repository.Integracao
{
    /// <summary>
    /// Repositório para IntegracaoFolhaLog
    /// </summary>
    public class IntegracaoFolhaLogRepository : RepositoryBase<IntegracaoFolhaLog>, IIntegracaoFolhaLogRepository
    {
        public IntegracaoFolhaLogRepository(DatabaseOptions dbOptions) 
            : base(dbOptions)
        {
        }

        /// <summary>
        /// Cria novo log
        /// </summary>
        public async Task<IntegracaoFolhaLog> CreateAsync(IntegracaoFolhaLog entity)
        {
            DbConnection.Open();
            try
            {
                var sql = @"
                    INSERT INTO ""IntegracaoFolhaLog"" 
                    (""IntegracaoId"", ""ClienteId"", ""DataHora"", ""IpOrigem"", ""ApiKey"", ""TipoOperacao"",
                     ""ColaboradoresEnviados"", ""ColaboradoresProcessados"", ""ColaboradoresErro"", 
                     ""Erros"", ""TempoProcessamento"", ""StatusCode"", ""Sucesso"", ""Mensagem"")
                    VALUES 
                    (@IntegracaoId, @ClienteId, @DataHora, @IpOrigem, @ApiKey, @TipoOperacao,
                     @ColaboradoresEnviados, @ColaboradoresProcessados, @ColaboradoresErro,
                     @Erros, @TempoProcessamento, @StatusCode, @Sucesso, @Mensagem)
                    RETURNING ""Id""";

                var id = await DbConnection.ExecuteScalarAsync<int>(sql, entity);
                entity.Id = id;
                return entity;
            }
            finally 
            { 
                DbConnection.Close(); 
            }
        }

        /// <summary>
        /// Busca logs por ClienteId
        /// </summary>
        public async Task<IEnumerable<IntegracaoFolhaLog>> GetByClienteIdAsync(int clienteId, int limit = 100)
        {
            DbConnection.Open();
            try
            {
                var sql = @"
                    SELECT * FROM ""IntegracaoFolhaLog"" 
                    WHERE ""ClienteId"" = @ClienteId 
                    ORDER BY ""DataHora"" DESC 
                    LIMIT @Limit";

                return await DbConnection.QueryAsync<IntegracaoFolhaLog>(sql, new { ClienteId = clienteId, Limit = limit });
            }
            finally 
            { 
                DbConnection.Close(); 
            }
        }

        /// <summary>
        /// Busca log por IntegracaoId
        /// </summary>
        public async Task<IntegracaoFolhaLog?> GetByIntegracaoIdAsync(string integracaoId)
        {
            DbConnection.Open();
            try
            {
                var sql = @"
                    SELECT * FROM ""IntegracaoFolhaLog"" 
                    WHERE ""IntegracaoId"" = @IntegracaoId 
                    LIMIT 1";

                return await DbConnection.QueryFirstOrDefaultAsync<IntegracaoFolhaLog>(sql, new { IntegracaoId = integracaoId });
            }
            finally 
            { 
                DbConnection.Close(); 
            }
        }
    }
}


