using FluentValidation;
using SingleOneAPI.Models;
using System;

namespace SingleOneAPI.Validators
{
    public class ContratoValidator : AbstractValidator<Contrato>
    {
        public ContratoValidator() 
        {
            RuleFor(r => r.Numero)
                .NotNull()
                .WithMessage("Número do contrato não pode ser nulo");

            RuleFor(r => r.Aditivo)
                .NotNull()
                .WithMessage("Número do aditivo não pode ser nulo");

            //RuleFor(r => r.DTInicioVigencia)
            //    .Must(d => d.Year >= (DateTime.Now.Year - 1))
            //    .WithMessage($"O ano do início da Vigencia do contrato deve ser no mínimo em {(DateTime.Now.Year - 1)}");
        }
    }
}
