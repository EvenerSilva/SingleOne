using Dapper;
using SingleOneIntegrator.Models;
using SingleOneIntegrator.Options;
using System;
using System.Threading.Tasks;

namespace SingleOneIntegrator.Repository.Integracao
{
    /// <summary>
    /// Repositório para ClienteIntegracao
    /// </summary>
    public class ClienteIntegracaoRepository : RepositoryBase<ClienteIntegracao>, IClienteIntegracaoRepository
    {
        public ClienteIntegracaoRepository(DatabaseOptions dbOptions) 
            : base(dbOptions)
        {
        }

        /// <summary>
        /// Busca integração por API Key
        /// </summary>
        public async Task<ClienteIntegracao?> GetByApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            DbConnection.Open();
            try
            {
                var sql = @"SELECT * FROM ""ClienteIntegracao"" WHERE ""ApiKey"" = @ApiKey LIMIT 1";
                return await DbConnection.QueryFirstOrDefaultAsync<ClienteIntegracao>(sql, new { ApiKey = apiKey });
            }
            finally 
            { 
                DbConnection.Close(); 
            }
        }

        /// <summary>
        /// Busca integração por ClienteId
        /// </summary>
        public async Task<ClienteIntegracao?> GetByClienteIdAsync(int clienteId)
        {
            DbConnection.Open();
            try
            {
                var sql = @"SELECT * FROM ""ClienteIntegracao"" WHERE ""ClienteId"" = @ClienteId LIMIT 1";
                return await DbConnection.QueryFirstOrDefaultAsync<ClienteIntegracao>(sql, new { ClienteId = clienteId });
            }
            finally 
            { 
                DbConnection.Close(); 
            }
        }

        /// <summary>
        /// Cria nova integração
        /// </summary>
        public async Task<ClienteIntegracao> CreateAsync(ClienteIntegracao entity)
        {
            DbConnection.Open();
            try
            {
                var sql = @"
                    INSERT INTO ""ClienteIntegracao"" 
                    (""ClienteId"", ""ApiKey"", ""ApiSecret"", ""IpWhitelist"", ""WebhookUrl"", ""Ativo"", ""DataCriacao"", ""Observacoes"")
                    VALUES 
                    (@ClienteId, @ApiKey, @ApiSecret, @IpWhitelist, @WebhookUrl, @Ativo, @DataCriacao, @Observacoes)
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
        /// Atualiza integração
        /// </summary>
        public async Task UpdateAsync(ClienteIntegracao entity)
        {
            DbConnection.Open();
            try
            {
                var sql = @"
                    UPDATE ""ClienteIntegracao"" 
                    SET 
                        ""ApiKey"" = @ApiKey,
                        ""ApiSecret"" = @ApiSecret,
                        ""IpWhitelist"" = @IpWhitelist,
                        ""WebhookUrl"" = @WebhookUrl,
                        ""Ativo"" = @Ativo,
                        ""DataAtualizacao"" = @DataAtualizacao,
                        ""Observacoes"" = @Observacoes
                    WHERE ""Id"" = @Id";

                entity.DataAtualizacao = DateTime.UtcNow;
                await DbConnection.ExecuteAsync(sql, entity);
            }
            finally 
            { 
                DbConnection.Close(); 
            }
        }

        /// <summary>
        /// Atualiza data da última sincronização
        /// </summary>
        public async Task UpdateUltimaSincronizacaoAsync(int id)
        {
            DbConnection.Open();
            try
            {
                var sql = @"
                    UPDATE ""ClienteIntegracao"" 
                    SET ""UltimaSincronizacao"" = @Now
                    WHERE ""Id"" = @Id";

                await DbConnection.ExecuteAsync(sql, new { Id = id, Now = DateTime.UtcNow });
            }
            finally 
            { 
                DbConnection.Close(); 
            }
        }
    }
}


