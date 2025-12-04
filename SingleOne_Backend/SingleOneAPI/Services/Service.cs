using AutoMapper;
using FluentValidation;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Services.Interface;
using System;
using System.Linq;

namespace SingleOneAPI.Services
{
    public class Service<TEntity> : IService<TEntity> where TEntity : class
    {
        private readonly IRepository<TEntity> _repository;
        private readonly IMapper _mapper;
        public Service(IRepository<TEntity> repository, IMapper mapper) 
        {
            _repository = repository;
            _mapper = mapper;
        }

        public void Add<TInputModel, TValidator>(TInputModel inputModel)
            where TInputModel : class
            where TValidator : AbstractValidator<TEntity>
        {
            Console.WriteLine($"[SERVICE] 🔍 Iniciando Add para tipo: {typeof(TEntity).Name}");
            Console.WriteLine($"[SERVICE] 🔍 InputModel: {inputModel}");
            
            Console.WriteLine("[SERVICE] 🔍 Mapeando inputModel para entidade...");
            TEntity entity = _mapper.Map<TEntity>(inputModel);
            Console.WriteLine($"[SERVICE] 🔍 Entidade mapeada: {entity}");
            
            Console.WriteLine("[SERVICE] 🔍 Validando entidade...");
            Validate(entity, Activator.CreateInstance<TValidator>());
            
            Console.WriteLine("[SERVICE] 🔍 Adicionando entidade ao repositório...");
            _repository.Adicionar(entity);
            
            Console.WriteLine("[SERVICE] ✅ Add concluído com sucesso");
        }

        public void Delete(int id) => 
            _repository.Remover(id);

        public void Delete(TEntity entity) => 
            _repository.Remover(entity);

        public void Update<TInputModel, TValidator>(TInputModel inputModel)
            where TInputModel : class
            where TValidator : AbstractValidator<TEntity>
        {
            TEntity entity = _mapper.Map<TEntity>(inputModel);
            Validate(entity, Activator.CreateInstance<TValidator>());
            _repository.Atualizar(entity);
        }

        private void Validate(TEntity obj, AbstractValidator<TEntity> validator)
        {
            Console.WriteLine($"[SERVICE] 🔍 Validando entidade do tipo: {typeof(TEntity).Name}");
            Console.WriteLine($"[SERVICE] 🔍 Entidade: {obj}");
            
            if (obj == null)
            {
                Console.WriteLine("[SERVICE] ❌ Entidade é nula");
                throw new ArgumentNullException("Registros não detectados");
            }

            Console.WriteLine("[SERVICE] 🔍 Executando validação...");
            var validationResult = validator.Validate(obj);
            
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"[SERVICE] ❌ Validação falhou. Erros: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
                validator.ValidateAndThrow(obj);
            }
            
            Console.WriteLine("[SERVICE] ✅ Validação passou com sucesso");
        }
    }
}
