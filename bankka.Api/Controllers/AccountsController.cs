using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using bankka.Api.Models;
using bankka.Commands;
using bankka.Commands.Customers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace bankka.Api.Controllers
{

    [Route("api/[controller]")]
    public class AccountsController  : Controller
    {
        private readonly ActorSystem _system;
        private readonly IValidator<CreateAccountModel> _validator;

        public AccountsController(ActorSystem system, IValidator<CreateAccountModel> validator)
        {
            _system = system;
            _validator = validator;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateAccountModel createAccountModel)
        {
            var validationResult = _validator.Validate(createAccountModel);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToErrorModel("2000", "input validation error"));
            }

            var newAccount = await SystemActors.CommandActor.Ask(new OpenAccountCommand(createAccountModel.Id, createAccountModel.Name));

            if(newAccount is OpenAccountResponse response)
                return Created($"/accounts/{response.AccountId}", newAccount);

            return BadRequest();
        }
        
    }



    public static class ModelStateExtensions
    {
        public static ErrorModel ToErrorModel(this ModelStateDictionary modelState, string code, string message)
        {
            var model = new ErrorModel(code, message);
            foreach (var state in modelState)
            {
                model.Properties.Add(new ErrorModelProperty
                {
                    Code = state.Key,
                    Field = state.Key,
                });
            }

            return model; 
        }

        public static ErrorModel ToErrorModel(this ValidationResult validationResult, string code, string message)
        {
            var model = new ErrorModel(code, message);
            
            foreach (var error in validationResult.Errors)
            {
                model.Properties.Add(new ErrorModelProperty
                {
                    Code = error.ErrorCode,
                    Field = error.PropertyName,
                });
            }

            return model; 
        }
    }

    public class ErrorModel
    {
        public ErrorModel(string code, string message)
        {
            Code = code;
            Message = message;
            Properties = new List<ErrorModelProperty>();
        }
        public string Code { get; }
        public string Message { get; }

        public IList<ErrorModelProperty> Properties { get; private set; }
    }

    public class ErrorModelProperty
    {
        public string  Code { get; set; }
        public string Field { get; set; }
    }

    public static class SystemActors
    {
        public static IActorRef CommandActor = ActorRefs.Nobody;
    }
}