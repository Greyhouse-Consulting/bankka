using Akka.Routing;

namespace bankka.Commands.Customers
{
    public class OpenAccountCommand
    {
        public long CustomerId { get; }
        public string Name { get; }

        public OpenAccountCommand(long customerId, string name)
        {
            CustomerId = customerId;
            Name = name;
        }

        public object ConsistentHashKey => CustomerId;
    }
}