namespace bankka.Commands
{
    public class AccountCommand 
    {
        public AccountCommand(decimal amount)
        {
            Amount = amount;
        }

        public decimal Amount { get; }
    }
}