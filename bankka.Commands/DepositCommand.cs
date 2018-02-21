namespace bankka.Commands
{
    public class DepositCommand  : AccountCommand
    {
        public DepositCommand(decimal amount) : base(amount)
        {
        }
    }
}