using System.Threading.Tasks;
using Akka.Actor;
using Akka.DI.Core;
using bankka.Commands.Customers;
using bankka.Core.Entities;
using bankka.Db;
using Serilog;

namespace bankka.Actors
{
    public class ClerkActor : ReceiveActor
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ActorSystem _system;
        private readonly ILogger _logger;

        public ClerkActor(IDbContextFactory dbContextFactory, ActorSystem system, ILogger logger)
        {
            _dbContextFactory = dbContextFactory;
            _system = system;
            _logger = logger;

            ReceiveAsync<NewCustomerCommand>(NewCustomerAsync);
            Receive<OpenAccountCommand>(m => OpenAccount(m));
        }

        public void OpenAccount(OpenAccountCommand openAccountCommand)
        {
            var customer = _system.ActorOf(_system.DI().Props<CustomerActor>());
            customer.Forward(openAccountCommand);
        }


        private async Task NewCustomerAsync(NewCustomerCommand newCustomerCommand)
        {
            using (var db = _dbContextFactory.Create())
            {
                var customer = new Customer
                {
                    Name = newCustomerCommand.Name,
                    PhoneNumber = newCustomerCommand.PhoneNumber
                };

                await db.Customers.AddAsync(customer);

                await db.SaveChangesAsync();

                _logger.Information("Created new customer with id {customerId}, {name}, {phone}", customer.Id, customer.Name, customer.PhoneNumber);

                Sender.Tell(new NewCustomerResponse(customer.Id));
            }
        }
    }
}