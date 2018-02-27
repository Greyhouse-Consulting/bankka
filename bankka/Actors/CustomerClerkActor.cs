using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.DI.Core;
using bankka.Commands;
using bankka.Commands.Customers;
using bankka.Core.Entities;
using bankka.Db;
using Serilog;

namespace bankka.Actors
{
    public class CustomerClerkActor : ReceiveActor
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ActorSystem _system;
        private readonly ILogger _logger;

        private readonly IDictionary<long, IActorRef> _customers;
        private readonly IDictionary<long, IActorRef> _cachedAccounts;

        public CustomerClerkActor(IDbContextFactory dbContextFactory, ActorSystem system, ILogger logger)
        {
            _dbContextFactory = dbContextFactory;
            _system = system;
            _logger = logger;

            _customers = new ConcurrentDictionary<long, IActorRef>();
            _cachedAccounts = new ConcurrentDictionary<long, IActorRef>();

            ReceiveAsync<NewCustomerCommand>(NewCustomerAsync);
            Receive<OpenAccountCommand>(m => OpenAccount(m));
            ReceiveAsync<DepositCommand>(DepositAsync);
            ReceiveAsync<WithdrawCommand>(WithdrawAsync);
        }

        private async Task WithdrawAsync(WithdrawCommand withdrawCommand)
        {
            var account = await Context.ActorSelection("./Customer/").ResolveOne(TimeSpan.FromSeconds(10));

            if (account == null)
            {

            }

        }

        private async Task DepositAsync(DepositCommand depositCommand)
        {
            IActorRef account;
            if (!_cachedAccounts.ContainsKey(depositCommand.TransactionToAccountId))
                account = await Context.ActorSelection($"../*/*/{depositCommand.TransactionToAccountId}").ResolveOne(TimeSpan.FromSeconds(10));
            else
                account = _cachedAccounts[depositCommand.TransactionToAccountId];

            account?.Tell(depositCommand);
        }

        public void OpenAccount(OpenAccountCommand openAccountCommand)
        {
            var customer = Context.ActorOf(_system.DI().Props<CustomerActor>(), openAccountCommand.CustomerId.ToString());

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

                var customerActor = Context.ActorOf(_system.DI().Props<CustomerActor>(), customer.Id.ToString());

                _logger.Information("Created new customer with id {customerId}, {name}, {phone}", customer.Id, customer.Name, customer.PhoneNumber);

                Sender.Tell(new NewCustomerResponse(customer.Id));

                Context.Stop(customerActor);
            }
        }
    }
}