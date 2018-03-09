using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using bankka.Commands.Customers;
using Microsoft.AspNetCore.Mvc;

namespace bankka.Api.Controllers
{
    
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]AccountModel account)
        {
            var customer = await SystemActors.CommandActor.Ask(new NewCustomerCommand(account.Name, account.PhoneNumber));

            if(customer is NewCustomerResponse response)
                return Created($"/customer/{response.CustomerId}", response.CustomerId);

            return BadRequest();
        }
    }

    public class AccountModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; } 
    }
}

