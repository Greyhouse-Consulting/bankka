using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using Akka.DI.Core;
using bankka.Commands;
using bankka.Commands.Accounts;

namespace bankka.Actors
{
    public class AccountClerkActor : ReceiveActor
    {
        private IDictionary<long, IActorRef> _managedAccounts;
        public AccountClerkActor()
        {
            _managedAccounts = new  ConcurrentDictionary<long, IActorRef>();

            Receive<DepositCommand>(m => Deposit(m));
            Receive<AccountOpenedCommand>(m => AccountOpened(m));
        }

        private void AccountOpened(AccountOpenedCommand accountOpenedCommand)
        {
            _managedAccounts.Add(accountOpenedCommand.AccountId,
                Context.ActorOf(Context.System.DI().Props<AccountActor>(), accountOpenedCommand.AccountId.ToString()));
        }

        private void Deposit(DepositCommand depositCommand)
        {

        }
    }
}