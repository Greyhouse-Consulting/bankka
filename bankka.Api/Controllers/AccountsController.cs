using System.Threading.Tasks;
using Akka.Actor;
using bankka.Api.Extensions;
using bankka.Api.Models;
using bankka.Commands.Customers;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace bankka.Api.Controllers
{

    [Route("api/[controller]")]
    public class AccountsController : Controller
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
                return BadRequest(validationResult.ToErrorModel("2000", "input validation error"));

            var response = await SystemActors.CommandActor.Ask(new OpenAccountCommand(createAccountModel.Id, createAccountModel.Name));

            switch (response)
            {
                case OpenAccountResponse newAccount:
                    return Created($"/accounts/{newAccount.AccountId}", newAccount);
                case ErrorResponse errorResponse:
                    return BadRequest(new ErrorModel("2001", errorResponse.Message));
            }

            return BadRequest(new ErrorModel("2002", "Unknown response"));
        }

    }


    public static class SystemActors
    {
        public static IActorRef CommandActor = ActorRefs.Nobody;
    }
}