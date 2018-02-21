namespace bankka.Commands.Accounts
{
    public class CustomerDepositCommand
    {
        public long AccountNo { get; }
        public decimal Amount { get; }

        public CustomerDepositCommand(long accountNo, decimal amount)
        {
            AccountNo = accountNo;
            Amount = amount;
        }
    }
}