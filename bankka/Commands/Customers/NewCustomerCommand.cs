namespace bankka.Commands.Customers
{
    public class NewCustomerCommand
    {
        public string Name { get; }
        public string PhoneNumber { get; }

        public NewCustomerCommand(string name, string phoneNumber)
        {
            Name = name;
            PhoneNumber = phoneNumber;
        }
    }
}