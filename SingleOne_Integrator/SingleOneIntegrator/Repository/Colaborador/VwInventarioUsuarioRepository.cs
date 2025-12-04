using SingleOneIntegrator.Models;
using SingleOneIntegrator.Options;

namespace SingleOneIntegrator.Repository.Colaborador
{
    internal class VwInventarioUsuarioRepository : RepositoryBase<VwInventarioUsuario>, IVwInventarioUsuarioRepository
    {
        public VwInventarioUsuarioRepository(DatabaseOptions dbOptions) 
            : base(dbOptions)
        {
        }

        public IEnumerable<VwInventarioUsuario> GetByQuery(string sqlQuery)
        {
            var list = new List<VwInventarioUsuario>();
            DbConnection.Open();

            try
            {
                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    command.CommandTimeout = 180;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new VwInventarioUsuario
                            {
                                NomeCompleto = reader.GetString(0),
                                NomeDeUsuario = reader.GetString(1),
                                CentroDeCusto = reader.GetString(2),
                                TxtCentroDeCusto = reader.GetString(3),
                                Cargo = reader.GetString(4),
                                Matricula = reader.GetString(5),
                                DataDeAdmissao = reader.GetDateTime(6),
                                DataDeDemissao = (reader[7] == DBNull.Value ? null : reader.GetDateTime(7)),
                                Empresa = reader.GetString(8),
                                Cpf = reader.GetString(9),
                                Cnpj = reader.GetString(10),
                                TipoDeColaborador = reader.GetString(11),
                                Status = reader.GetString(12),
                                Cidade = reader.GetString(13),
                                Estado = reader.GetString(14),
                                NomeFantasia = reader.GetString(15),
                                EmailCorporativo = reader.GetString(16),
                                Superior = reader.GetString(17)
                            });
                        }
                    }
                }
            }
            finally { DbConnection.Close(); }
            return list;
        }
    }
}
