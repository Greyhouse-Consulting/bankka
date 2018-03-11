using Akka.Routing;

namespace bankka.Commands
{
    public class WithdrawCommand : AccountCommand, IConsistentHashable
    {
        public long TransactionToAccountId { get; }

        public WithdrawCommand(long transactionToAccountId, decimal amount) : base(amount)
        {
            TransactionToAccountId = transactionToAccountId;
        }

        public object ConsistentHashKey => TransactionToAccountId;
    }
}