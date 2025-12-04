using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;

namespace SingleOneAPI.Infra.Repositorio
{
    public interface IRepository<T> : IReadOnlyRepository<T> where T : class
    {   
        void Adicionar(T entity);
        void AdicionarSemSalvar(T entity);
        void Atualizar(T entity);
        void AtualizarMuitos(IEnumerable<T> entities);
        void Remover(int id);
        void Remover(T Entity);
        IDbContextTransaction BeginTransaction();
        
        // ✅ NOVOS MÉTODOS: Transações seguras com retry automático
        void ExecuteInTransaction(Action action);
        TResult ExecuteInTransaction<TResult>(Func<TResult> func);
        
        // ✅ MÉTODO: Salvar alterações manualmente
        void SalvarAlteracoes();
    }
}
