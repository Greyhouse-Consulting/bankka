namespace bankka.Commands
{
    public class WithdrawCommand : AccountCommand
    {
        public long TransactionToAccountId { get; }

        public WithdrawCommand(long transactionToAccountId, decimal amount) : base(amount)
        {
            TransactionToAccountId = transactionToAccountId;
        }
    }
}