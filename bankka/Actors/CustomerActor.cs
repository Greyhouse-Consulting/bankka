﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Akka.Actor;
using Akka.DI.Core;
using bankka.Commands;
using bankka.Commands.Accounts;
using bankka.Commands.Customers;
using bankka.Core;
using bankka.Db;

namespace bankka.Actors
{
    public class CustomerActor : ReceiveActor
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ActorSystem _system;

        private readonly IList<IActorRef> _accounts;

        public CustomerActor(IDbContextFactory dbContextFactory, ActorSystem system)
        {
            _dbContextFactory = dbContextFactory;
            _system = system;
            _accounts = new List<IActorRef>();
            Receive<OpenAccountCommand>(a => OpenAccount(a));
            Receive<CustomerDepositCommand>(a => ForwardToAccount(a));
            Receive<CustomerAccountsCommand>(a => CustomerAccounts(a));

            BootAccountsActors();
        }

        private bool ShouldHandle(OpenAccountCommand openAccountCommand)
        {
            return true;
        }

        private void CustomerAccounts(CustomerAccountsCommand customerAccountsCommand)
        {
            Sender.Tell(_accounts.Select(a => a.Path.Name));
        }

        private void ForwardToAccount(CustomerDepositCommand depositCommand)
        {
            var account = _accounts.First(a => a.Path.Name == depositCommand.AccountNo.ToString());
            account.Tell(new DepositCommand(depositCommand.Amount));
        }

        private void BootAccountsActors()
        {
            using (var c = _dbContextFactory.Create())
            {
                foreach (var a in c.Accounts.Where(k => k.Customer.Id == Convert.ToInt64( Self.Path.Name)))
                {
                    var newAccount = _system.ActorOf(_system.DI().Props<AccountActor>(), a.AccountNo);
                    _accounts.Add(newAccount);
                }
            }
        }

        private void OpenAccount(OpenAccountCommand openAccountCommand)
        {
            using (var c = _dbContextFactory.Create())
            {
                var account = new Account
                {
                    Balance = 0,
                    Customer = c.Customers.Find(Convert.ToInt64( Self.Path.Name)),
                };
                c.Accounts.Add(account);
                c.SaveChanges();

                var newAccount = _system.ActorOf(_system.DI().Props<AccountActor>(), account.Id.ToString());
                _accounts.Add(newAccount);

                Sender.Tell(account.Id);
            }


        }
    }
}