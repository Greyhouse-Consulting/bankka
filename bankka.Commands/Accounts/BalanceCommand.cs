using Akka.Routing;

namespace bankka.Commands.Accounts
{
    public class BalanceCommand : IConsistentHashable
    {
        public long AccountId { get; }

        public BalanceCommand(long accountId)
        {
            AccountId = accountId;
        }

        public object ConsistentHashKey => AccountId;
    }
}