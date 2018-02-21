using System.Threading.Tasks;
using Akka.Actor;
using bankka.Api.Models;
using bankka.Commands;
using bankka.Commands.Customers;
using Microsoft.AspNetCore.Mvc;

namespace bankka.Api.Controllers
{

    [Route("api/[controller]")]
    public class AccountsController  : Controller
    {
        private readonly ActorSystem _system;

        public AccountsController(ActorSystem system)
        {
            _system = system;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateAccountModel createAccountModel)
        {
            var newAccount = await SystemActors.CommandActor.Ask(new OpenAccountCommand(createAccountModel.Id, createAccountModel.Name));

            if(newAccount is OpenAccountResponse response)
                return Created($"/accounts/{response.AccountId}", newAccount);

            return BadRequest();
        }
        
    }


    public static class SystemActors
    {
        public static IActorRef CommandActor = ActorRefs.Nobody;
    }
}