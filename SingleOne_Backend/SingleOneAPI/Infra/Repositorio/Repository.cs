using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SingleOneAPI.Infra.Contexto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SingleOneAPI.Infra.Repositorio
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SingleOneDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public Repository(SingleOneDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public void Adicionar(T entity)
        {
            Console.WriteLine($"[REPOSITORY] 🔍 Adicionando entidade do tipo: {typeof(T).Name}");
            Console.WriteLine($"[REPOSITORY] 🔍 Entidade: {entity}");

            try 
            {
                // Desanexar entidades relacionadas que podem estar sendo rastreadas
                var entry = _context.Entry(entity);
                if (entry.State != EntityState.Detached)
                {
                    entry.State = EntityState.Detached;
                }

                // Desanexar todas as entidades de navegação sem tentar rastrear coleções como entidades
                foreach (var navigation in entry.Navigations)
                {
                    if (navigation.CurrentValue == null)
                        continue;

                    // Se for uma coleção (ex: HashSet<Equipamento>), iterar nos itens
                    if (navigation.Metadata.IsCollection &&
                        navigation.CurrentValue is IEnumerable collection &&
                        navigation.CurrentValue is not string)
                    {
                        foreach (var relatedEntity in collection)
                        {
                            if (relatedEntity == null) continue;

                            var relatedEntry = _context.Entry(relatedEntity);
                            if (relatedEntry.State != EntityState.Detached)
                            {
                                relatedEntry.State = EntityState.Detached;
                            }
                        }
                    }
                    else
                    {
                        // Navegação de referência única
                        var relatedEntry = _context.Entry(navigation.CurrentValue);
                        if (relatedEntry.State != EntityState.Detached)
                        {
                            relatedEntry.State = EntityState.Detached;
                        }
                    }
                }

                // Adicionar a entidade principal
                _context.Set<T>().Add(entity);
                Console.WriteLine($"[REPOSITORY] ✅ Entidade adicionada ao contexto");

                // ✅ CORREÇÃO: Executar SaveChanges sob ExecutionStrategy (compatível com NpgsqlRetryingExecutionStrategy)
                var strategy = _context.Database.CreateExecutionStrategy();
                var result = 0;
                strategy.Execute(() => { result = _context.SaveChanges(); });
                Console.WriteLine($"[REPOSITORY] ✅ SaveChanges executado. Entidades afetadas: {result}");
                
                if (result > 0)
                {
                    Console.WriteLine($"[REPOSITORY] ✅ SUCESSO: {result} entidade(s) persistida(s) no banco");
                }
                else
                {
                    Console.WriteLine($"[REPOSITORY] ⚠️ AVISO: SaveChanges retornou 0 - nenhuma entidade foi persistida");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REPOSITORY] ❌ ERRO ao adicionar entidade: {ex.Message}");
                Console.WriteLine($"[REPOSITORY] ❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        // ✅ NOVO MÉTODO: Adicionar sem salvar (para uso em transações)
        public void AdicionarSemSalvar(T entity)
        {
            Console.WriteLine($"[REPOSITORY] 🔍 Adicionando entidade do tipo: {typeof(T).Name} (sem salvar)");
            Console.WriteLine($"[REPOSITORY] 🔍 Entidade: {entity}");

            try 
            {
                _context.Set<T>().Add(entity);
                Console.WriteLine($"[REPOSITORY] ✅ Entidade adicionada ao contexto (sem SaveChanges)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REPOSITORY] ❌ ERRO ao adicionar entidade: {ex.Message}");
                Console.WriteLine($"[REPOSITORY] ❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public void Atualizar(T entity)
        {
            Console.WriteLine($"[REPOSITORY] 🔍 Atualizando entidade do tipo: {typeof(T).Name}");
            Console.WriteLine($"[REPOSITORY] 🔍 Entidade: {entity}");
            
            // ✅ Adicionar stack trace para identificar origem da chamada
            var stackTrace = Environment.StackTrace;
            var caller = stackTrace.Split('\n').Skip(1).Take(3).ToArray();
            Console.WriteLine($"[REPOSITORY] 🔍 Chamado de:");
            foreach (var line in caller)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    Console.WriteLine($"[REPOSITORY]    {line.Trim()}");
            }
            
            try
            {
                var entityType = typeof(T);
                var idProperty = entityType.GetProperty("Id");
                
                if (idProperty != null)
                {
                    var entityId = idProperty.GetValue(entity);
                    Console.WriteLine($"[REPOSITORY] 🔍 ID da entidade: {entityId}");
                    
                    // ✅ Log específico para Requisicoesiten com Dtprogramadaretorno
                    if (entityType.Name == "Requisicoesiten")
                    {
                        var dtProgramadoProperty = entityType.GetProperty("Dtprogramadaretorno");
                        if (dtProgramadoProperty != null)
                        {
                            var dtProgramadoValue = dtProgramadoProperty.GetValue(entity);
                            Console.WriteLine($"[REPOSITORY] 🔍 Requisicoesiten.Dtprogramadaretorno: {(dtProgramadoValue?.ToString() ?? "NULL")}");
                        }
                    }
                    
                    // Verificar se a entidade já está sendo rastreada
                    var trackedEntity = _context.Entry(entity);
                    Console.WriteLine($"[REPOSITORY] 🔍 Estado da entidade: {trackedEntity.State}");
                    
                    if (trackedEntity.State == EntityState.Detached)
                    {
                        // Entidade não está sendo rastreada, verificar se existe no contexto local
                        var existingLocalEntity = _context.Set<T>().Local.FirstOrDefault(e => 
                            idProperty.GetValue(e).Equals(entityId));
                        
                        if (existingLocalEntity != null)
                        {
                            Console.WriteLine("[REPOSITORY] 🔍 Encontrada entidade no contexto local, desanexando...");
                            _context.Entry(existingLocalEntity).State = EntityState.Detached;
                        }
                        
                        // ✅ CORREÇÃO: Usar Update() para garantir que TODOS os campos sejam atualizados
                        Console.WriteLine("[REPOSITORY] 🔍 Usando Update() para atualizar todos os campos...");
                        _context.Set<T>().Update(entity);
                    }
                    else
                    {
                        Console.WriteLine("[REPOSITORY] 🔍 Entidade já está sendo rastreada, marcando como modificada...");
                        trackedEntity.State = EntityState.Modified;
                    }
                }
                else
                {
                    Console.WriteLine("[REPOSITORY] 🔍 Propriedade Id não encontrada, usando Update...");
                    _context.Set<T>().Update(entity);
                }
                
                Console.WriteLine("[REPOSITORY] 🔍 Chamando SaveChanges sob ExecutionStrategy...");
                var strategy = _context.Database.CreateExecutionStrategy();
                var result = 0;
                strategy.Execute(() => { result = _context.SaveChanges(); });
                Console.WriteLine($"[REPOSITORY] ✅ SaveChanges executado. Entidades afetadas: {result}");
                
                if (result > 0)
                {
                    Console.WriteLine($"[REPOSITORY] ✅ SUCESSO: {result} entidade(s) atualizada(s) no banco");
                }
                else
                {
                    Console.WriteLine($"[REPOSITORY] ⚠️ AVISO: SaveChanges retornou 0 - nenhuma entidade foi atualizada");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REPOSITORY] ❌ ERRO ao atualizar entidade: {ex.Message}");
                Console.WriteLine($"[REPOSITORY] ❌ StackTrace: {ex.StackTrace}");
                
                // Log mais detalhado do erro interno
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[REPOSITORY] ❌ Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[REPOSITORY] ❌ Inner StackTrace: {ex.InnerException.StackTrace}");
                }
                
                throw;
            }
        }

        // ✅ NOVO MÉTODO: Salvar alterações manualmente
                public void SalvarAlteracoes()
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            strategy.Execute(() => { _context.SaveChanges(); });
        }

        public IDbContextTransaction BeginTransaction()
        {
            // ✅ CORREÇÃO: Usar CreateExecutionStrategy para transações com PostgreSQL
            var strategy = _context.Database.CreateExecutionStrategy();
            return strategy.Execute(() => _context.Database.BeginTransaction());
        }

        // ✅ NOVO MÉTODO: Executar operações em transação com retry automático
        public void ExecuteInTransaction(Action action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        action();
                        // ✅ IMPORTANTE: Salvar alterações antes do commit (sob ExecutionStrategy)
                        var saveStrategy = _context.Database.CreateExecutionStrategy();
                        saveStrategy.Execute(() => { _context.SaveChanges(); });
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        // ✅ NOVO MÉTODO: Executar operações em transação com retorno
        public TResult ExecuteInTransaction<TResult>(Func<TResult> func)
        {
            Console.WriteLine($"[REPOSITORY] 🔍 Iniciando ExecuteInTransaction para tipo: {typeof(TResult).Name}");
            
            var strategy = _context.Database.CreateExecutionStrategy();
            return strategy.Execute(() =>
            {
                Console.WriteLine("[REPOSITORY] 🔍 Criando transação...");
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        Console.WriteLine("[REPOSITORY] 🔍 Executando função dentro da transação...");
                        var result = func();
                        Console.WriteLine($"[REPOSITORY] 🔍 Função executada. Resultado: {result}");
                        
                        // ✅ IMPORTANTE: Salvar alterações antes do commit
                        Console.WriteLine("[REPOSITORY] 🔍 Chamando SaveChanges sob ExecutionStrategy...");
                        var saveStrategy = _context.Database.CreateExecutionStrategy();
                        var saveResult = 0;
                        saveStrategy.Execute(() => { saveResult = _context.SaveChanges(); });
                        Console.WriteLine($"[REPOSITORY] ✅ SaveChanges executado. Entidades afetadas: {saveResult}");
                        
                        Console.WriteLine("[REPOSITORY] 🔍 Fazendo commit da transação...");
                        transaction.Commit();
                        Console.WriteLine("[REPOSITORY] ✅ Transação commitada com sucesso!");
                        
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[REPOSITORY] ❌ ERRO na transação: {ex.Message}");
                        Console.WriteLine($"[REPOSITORY] ❌ Fazendo rollback...");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        public IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }
        public IQueryable<T> IncludeWithThenInclude(params Expression<Func<IQueryable<T>, IQueryable<T>>>[] includeExpressions)
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeExpression in includeExpressions)
            {
                query = includeExpression.Compile().Invoke(query);
            }

            return query;
        }


        public T ObterPorId(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public IEnumerable<T> ObterTodos()
        {
            return _context.Set<T>().ToList();
        }

        public IQueryable<T> Query()
        {
            return _dbSet;
        }

        public void Remover(int id)
        {
            var entity = _context.Set<T>().Find(id);
            if (entity != null)
            {
            _context.Set<T>().Remove(entity);
            var strategy = _context.Database.CreateExecutionStrategy();
            strategy.Execute(() => { _context.SaveChanges(); });
            }
        }

        public void Remover(T TEntity)
        {
            _context.Remove(TEntity);
            var strategy = _context.Database.CreateExecutionStrategy();
            strategy.Execute(() => { _context.SaveChanges(); });
        }

        public void AtualizarMuitos(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _context.Set<T>().Update(entity);
            }
            var strategy = _context.Database.CreateExecutionStrategy();
            strategy.Execute(() => { _context.SaveChanges(); });
        }

        public IQueryable<T> Buscar(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }
    }
}
