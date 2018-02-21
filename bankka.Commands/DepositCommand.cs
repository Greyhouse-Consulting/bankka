namespace bankka.Commands
{
    public class DepositCommand  : AccountCommand
    {
        public long TransactionToAccountId { get; }

        public DepositCommand(long transactionToAccountId, decimal amount) : base(amount)
        {
            TransactionToAccountId = transactionToAccountId;
        }
    }
}