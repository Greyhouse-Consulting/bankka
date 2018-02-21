namespace bankka.Commands.Customers
{
    public class OpenAccountResponse
    {
        public long AccountId { get; }

        public string OwerName { get;}
        public OpenAccountResponse(long accountId, string owerName)
        {
            AccountId = accountId;

            OwerName = owerName;

        }
    }
}