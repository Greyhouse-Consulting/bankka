using Akka.Routing;

namespace bankka.Commands.Accounts
{
    public class RetreieveTransactionCommand : IConsistentHashable
    {
        public long AccountId { get; }

        public RetreieveTransactionCommand(long accountId)
        {
            AccountId = accountId;
        }

        public object ConsistentHashKey => AccountId;
    }
}