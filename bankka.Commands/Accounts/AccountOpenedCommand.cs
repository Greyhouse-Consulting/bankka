using Akka.Routing;

namespace bankka.Commands.Accounts
{
    public class AccountOpenedCommand : IConsistentHashable
    {
        public long AccountId { get; }

        public AccountOpenedCommand(long accountId)
        {
            AccountId = accountId;
        }

        public object ConsistentHashKey => AccountId;
    }
}