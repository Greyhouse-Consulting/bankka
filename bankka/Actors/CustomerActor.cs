using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Akka.Actor;
using Akka.DI.Core;
using bankka.Commands;
using bankka.Commands.Accounts;
using bankka.Commands.Customers;
using bankka.Core.Entities;
using bankka.Db;
using Serilog;

namespace bankka.Actors
{
    public class CustomerActor : ReceiveActor
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ActorSystem _system;
        private readonly ILogger _logger;

        private readonly IDictionary<long, IActorRef> _accounts;

        public CustomerActor(IDbContextFactory dbContextFactory, ActorSystem system, ILogger logger)
        {
            _dbContextFactory = dbContextFactory;
            _system = system;
            _logger = logger;
            _accounts = new  ConcurrentDictionary<long, IActorRef>();
            ReceiveAsync<OpenAccountCommand>(a => OpenAccount(a));
            Receive<CustomerDepositCommand>(a => ForwardToAccount(a));
            Receive<CustomerAccountsCommand>(a => CustomerAccounts(a));

          //  BootAccountsActors();
        }

        private bool ShouldHandle(OpenAccountCommand openAccountCommand)
        {
            return true;
        }

        private void CustomerAccounts(CustomerAccountsCommand customerAccountsCommand)
        {
            //Sender.Tell(_accounts.Select(a => a.Path.Name));
        }

        private void ForwardToAccount(CustomerDepositCommand depositCommand)
        {
            //var account = _accounts.First(a => a.Path.Name == depositCommand.AccountNo.ToString());
            //account.Tell(new DepositCommand(depositCommand.AccountNo, depositCommand.Amount));
        }

        private async Task OpenAccount(OpenAccountCommand openAccountCommand)
        {
            _logger.Information("Creating account for customer {0}", openAccountCommand.CustomerId);
            using (var c = _dbContextFactory.Create())
            {
                var customer = c.Customers.FirstOrDefault(x => x.Id == openAccountCommand.CustomerId);
                if (customer == null)
                {
                    _logger.Warning("No customer found for id {customerId} ", openAccountCommand.CustomerId);
                    Sender.Tell(new ErrorResponse($"No customer found for id {openAccountCommand.CustomerId} "));
                    return;
                }

                var account = new Account
                {
                    Balance = 0,
                    Customer = customer,
                    Name = openAccountCommand.Name
                };

                await c.Accounts.AddAsync(account);
                await c.SaveChangesAsync();

                var accountActor = Context.ActorOf(_system.DI().Props<AccountActor>(), account.Id.ToString());

                _accounts.Add(account.Id, accountActor);

                _logger.Information("Account with id '{0}' and name '{accountName}' created", account.Id, account.Name);

                Sender.Tell(new OpenAccountResponse(account.Id, customer.Name));

                var accountClerks = await Context.ActorSelection("/user/accountClerks").ResolveOne(TimeSpan.FromSeconds(1));
                accountClerks.Tell(new AccountOpenedCommand(account.Id));
            }
        }
    }
}