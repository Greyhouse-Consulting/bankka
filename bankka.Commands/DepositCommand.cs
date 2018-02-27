using Akka.Routing;

namespace bankka.Commands
{
    public class DepositCommand  : AccountCommand, IConsistentHashable
    {
        public long TransactionToAccountId { get; }

        public DepositCommand(long transactionToAccountId, decimal amount) : base(amount)
        {
            TransactionToAccountId = transactionToAccountId;
        }

        public object ConsistentHashKey => TransactionToAccountId;
    }
}