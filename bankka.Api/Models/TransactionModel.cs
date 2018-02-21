namespace bankka.Api.Models
{
    public class TransactionModel
    {
        public TransactionType TransactionType { get; set; }
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public decimal Amount { get; set; }
    }

    public enum TransactionType
    {
        Deposit,
        Withdraw
    }
}