namespace bankka.Commands
{
    public class WithdrawCommand : AccountCommand
    {
        public WithdrawCommand(decimal amount) : base(amount)
        {
        }
    }
}