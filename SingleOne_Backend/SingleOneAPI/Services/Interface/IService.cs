using FluentValidation;

namespace SingleOneAPI.Services.Interface
{
    public interface IService<TEntity> where TEntity : class
    {
        void Add<TInputModel, TValidator>(TInputModel inputModel)
            where TValidator : AbstractValidator<TEntity>
            where TInputModel : class;

        void Delete(int id);
        void Delete(TEntity entity);

        void Update<TInputModel, TValidator>(TInputModel inputModel)
            where TValidator : AbstractValidator<TEntity>
            where TInputModel : class;
    }
}
