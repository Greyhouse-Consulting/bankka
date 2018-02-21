namespace bankka.Commands.Customers
{
    public class CustomerAccountsCommand
    {
        public CustomerAccountsCommand(int accountId)
        {
            AccountId = accountId;
        }
        public int AccountId { get; private set; }
    }
}
