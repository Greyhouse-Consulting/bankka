using System;
using System.Threading.Tasks;
using Akka.Actor;
using bankka.Commands.Customers;
using bankka.Core;
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
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {


            //var c = await _system.ActorSelection($"/user/123456").ResolveOne(TimeSpan.FromSeconds(3));
            //var c = await _system.ActorSelection($"akka.tcp://bankka@127.0.0.1:16001/user/123456").ResolveOne(TimeSpan.FromSeconds(3));

            //if (c == null)
            //    return NotFound(id);

            //var accounts = await SystemActors.CommandActor.Ask(new CommandProcessor.CreateAccount(id.ToString()));
            var accounts = await SystemActors.CommandActor.Ask(new OpenAccountCommand());

            return Ok(accounts);
        }
        
    }


    public static class SystemActors
    {
        public static IActorRef CommandActor = ActorRefs.Nobody;

    }
}