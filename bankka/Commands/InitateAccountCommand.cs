namespace bankka.Commands
{
    internal class InitateAccountCommand
    {
        public long CustomerId { get; }
        public InitateAccountCommand(long customerId)
        {
            CustomerId = customerId;
        }
    }
}