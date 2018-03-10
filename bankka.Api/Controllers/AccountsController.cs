using System.Threading.Tasks;
using Akka.Actor;
using bankka.Api.Extensions;
using bankka.Api.Models;
using bankka.Commands;
using bankka.Commands.Accounts;
using bankka.Commands.Customers;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace bankka.Api.Controllers
{
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
        [Route("api/[controller]")]
        public async Task<ActionResult> Post([FromBody] CreateAccountModel createAccountModel)
        {
            var validationResult = _validator.Validate(createAccountModel);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.ToErrorModel("2000", "input validation error"));

            var response = await SystemActors.CustomerActor.Ask(new OpenAccountCommand(createAccountModel.Id, createAccountModel.Name));

            switch (response)
            {
                case OpenAccountResponse newAccount:
                    return Created($"/accounts/{newAccount.AccountId}", newAccount);
                case ErrorResponse errorResponse:
                    return BadRequest(new ErrorModel("2001", errorResponse.Message));
            }

            return BadRequest(new ErrorModel("2002", "Unknown response"));
        }

        [HttpPost]
        [Route("api/[controller]/{accountId}/transactions")]
        public IActionResult Transfer(long accountId, [FromBody] TransactionModel transaction)
        {
            if (transaction.TransactionType == TransactionType.Deposit)
            {
                SystemActors.AccountClerks.Tell(new DepositCommand(accountId, transaction.Amount));
            }

            return Ok();
        }

        [HttpGet]
        [Route("api/[controller]/{accountId}/transactions")]
        public async Task<IActionResult> GetTransctions(long accountId)
        {
            var transactions = await SystemActors.AccountClerks.Ask(new RetreieveTransactionCommand(accountId));

            return Ok(transactions);
        }

        [HttpGet]
        [Route("api/[controller]/{accountId}/balance")]
        public async Task<IActionResult> Balance(long accountId)
        {
            var balance  = await SystemActors.AccountClerks.Ask(new BalanceCommand(accountId));
            return Ok(balance);
        }
    }


    public static class SystemActors
    {
        public static IActorRef CustomerActor = ActorRefs.Nobody;
        public static IActorRef AccountClerks = ActorRefs.Nobody;
    }
}