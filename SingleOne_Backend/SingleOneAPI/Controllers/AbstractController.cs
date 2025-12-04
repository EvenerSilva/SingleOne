using Microsoft.AspNetCore.Mvc;
using SingleOne;
using System;
using System.Collections.Generic;

namespace SingleOneAPI.Controllers
{
    public abstract class AbstractController<TController> : ControllerBase
        where TController : AbstractController<TController>
    {
        protected AbstractController() { }
        protected IActionResult Execute(Func<object> func)
        {
            try
            {
                var result = func();
                return Ok(result);
            }
            catch (EntidadeNaoEncontradaEx ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (EntidadeJaExisteEx ex)
            {
                return Conflict(new { ex.Message });
            }
            catch (DomainException ex)
            {
                return UnprocessableEntity(new { ex.Message });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { Message = MapFluentValidationError(ex) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }

        private Dictionary<string, List<string>> MapFluentValidationErrors(FluentValidation.ValidationException validationException)
        {
            Dictionary<string, List<string>> mappedErrors = new Dictionary<string, List<string>>();
            foreach (var failure in validationException.Errors)
            {
                if (!mappedErrors.ContainsKey(failure.PropertyName))
                {
                    mappedErrors[failure.PropertyName] = new List<string>();
                }
                mappedErrors[failure.PropertyName].Add(failure.ErrorMessage);
            }
            return mappedErrors;
        }

        private string MapFluentValidationError(FluentValidation.ValidationException validationException)
        {
            foreach (var failure in validationException.Errors)
            {
                return failure.ErrorMessage;
            }

            return string.Empty;
        }
    }
}
