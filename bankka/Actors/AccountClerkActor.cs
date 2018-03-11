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
        private readonly ActorSystem _system;
        private IDictionary<long, IActorRef> _managedAccounts;
        public AccountClerkActor(ActorSystem system)
        {
            _system = system;
            _managedAccounts = new  ConcurrentDictionary<long, IActorRef>();

            Receive<DepositCommand>(m => Deposit(m));
            Receive<WithdrawCommand>(m => Withdraw(m));
            Receive<AccountOpenedCommand>(m => AccountOpened(m));
            Receive<BalanceCommand>(m => Balance(m));
            Receive<RetreieveTransactionCommand>(m => RetreieveTransactions(m));
        }

        private void RetreieveTransactions(RetreieveTransactionCommand retreieveTransactionCommand)
        {
            if(!_managedAccounts.TryGetValue(retreieveTransactionCommand.AccountId, out var account))
            {
                account = Context.ActorOf(_system.DI().Props<CustomerActor>(), retreieveTransactionCommand.AccountId.ToString());
            }

            account.Forward(retreieveTransactionCommand);
        }

        private void Balance(BalanceCommand balanceCommand)
        {
            if(!_managedAccounts.TryGetValue(balanceCommand.AccountId, out var account))
            {
                account = Context.ActorOf(_system.DI().Props<CustomerActor>(), balanceCommand.AccountId.ToString());
            }

            account.Forward(balanceCommand);
        }

        private void AccountOpened(AccountOpenedCommand accountOpenedCommand)
        {
            _managedAccounts.Add(accountOpenedCommand.AccountId,
                Context.ActorOf(Context.System.DI().Props<AccountActor>(), accountOpenedCommand.AccountId.ToString()));
        }

        private void Deposit(DepositCommand depositCommand)
        {
            if(!_managedAccounts.TryGetValue(depositCommand.TransactionToAccountId, out var account))
            {
                account = Context.ActorOf(_system.DI().Props<CustomerActor>(), depositCommand.TransactionToAccountId.ToString());
            }

            account.Tell(depositCommand);
        }
        private void Withdraw(WithdrawCommand withdrawCommand)
        {
            if(!_managedAccounts.TryGetValue(withdrawCommand.TransactionToAccountId, out var account))
            {
                account = Context.ActorOf(_system.DI().Props<CustomerActor>(), withdrawCommand.TransactionToAccountId.ToString());
            }

            account.Tell(withdrawCommand);
        }

    }
}