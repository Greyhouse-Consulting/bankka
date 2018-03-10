using System;
using System.Threading.Tasks;
using Akka.Actor;
using bankka.Commands;
using bankka.Commands.Accounts;
using bankka.Core.Entities;
using bankka.Db;
using Serilog;

namespace bankka.Actors
{
    internal class AccountActor : ReceiveActor
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public AccountActor(IDbContextFactory dbContextFactory, ILogger logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;

            RegisterReceivers();
        }

        private void RegisterReceivers()
        {
            Receive<WithdrawCommand>(b => Withdraw(b));
            ReceiveAsync<DepositCommand>(DepositAsync);
            Receive<BalanceCommand>(a => ReplySaldo(a));
        }


        private void ReplySaldo(BalanceCommand balanceCommand)
        {
            _logger.Information("Returning Balance for  account with id {accountId}", Self.Path.Name);

            using (var db = _dbContextFactory.Create())
            {
                var account = db.Accounts.Find(balanceCommand.AccountId);
                Sender.Tell(account.Balance);
            }
        }

        private async Task DepositAsync(AccountCommand depositCommand)
        {
            _logger.Information("Depositing {amount} to account with no {accountId}", depositCommand.Amount, Self.Path.Name);

            using (var db = _dbContextFactory.Create())
            {
                var account = db.Accounts.Find(Convert.ToInt64(Self.Path.Name));

                account.Balance += depositCommand.Amount;
                account.Transactions.Add(new Transaction
                {
                    Amount = depositCommand.Amount,
                    DateTime = DateTime.Now
                });
                await db.SaveChangesAsync();
            }
        }

        private void Withdraw(AccountCommand withdrawCommand)
        {
            _logger.Information("Withdrawing {amount} to account with no {accountId}", withdrawCommand.Amount, Self.Path.Name);

            using (var db = _dbContextFactory.Create())
            {
                var account = db.Accounts.Find(Self.Path.Name);

                account.Balance -= withdrawCommand.Amount;
                account.Transactions.Add(new Transaction
                {
                    Amount = -withdrawCommand.Amount,
                    DateTime = DateTime.Now
                });
                db.SaveChanges();
            }
        }
    }
}