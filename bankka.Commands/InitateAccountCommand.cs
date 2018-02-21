namespace bankka.Commands
{
    public class InitateAccountCommand
    {
        public long CustomerId { get; }
        public InitateAccountCommand(long customerId)
        {
            CustomerId = customerId;
        }
    }
}