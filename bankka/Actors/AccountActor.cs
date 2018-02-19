using System;
using Akka.Actor;
using bankka.Commands;
using bankka.Db;
using Serilog;

namespace bankka.Actors
{
    internal class AccountActor : ReceiveActor
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger _logger;
        private  long _accountId;

        public AccountActor(IDbContextFactory dbContextFactory, ILogger logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;

            RegisterReceivers();
        }

        private void RegisterReceivers()
        {
            Receive<InitateAccountCommand>(b => SetAccountNo(b));
            Receive<WithdrawCommand>(b => Withdraw(b));
            Receive<DepositCommand>(a => Deposit(a));
            Receive<BalanceCommand>(a => ReplySaldo(a));
        }

        private void SetAccountNo(InitateAccountCommand initateAccountCommand)
        {

            _logger.Information("Creating account wiht no {accountNo}", initateAccountCommand.CustomerId);
            using (var db = _dbContextFactory.Create())
            {
                var account = new Account
                {
                    AccountNo = Self.Path.Name,
                    Balance = 0
                };

                db.Accounts.Add(account);

                _accountId = account.Id;
                db.SaveChanges();
            }
        }

        private void ReplySaldo(BalanceCommand balanceCommand)
        {
            _logger.Information("Returning Saldo for  account with id {accountId}", _accountId);

            using (var db = _dbContextFactory.Create())
            {
                var account = db.Accounts.Find(_accountId);
                Sender.Tell(account.Balance);
            }
        }

        private void Deposit(AccountCommand depositCommand)
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
                db.SaveChanges();
            }
        }

        private void Withdraw(AccountCommand withdrawCommand)
        {
            _logger.Information("Withdrawing {amount} to account with no {accountId}", withdrawCommand.Amount, _accountId);

            using (var db = _dbContextFactory.Create())
            {
                var account = db.Accounts.Find(_accountId);

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