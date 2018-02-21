using System.Collections.Generic;

namespace bankka.Core.Entities
{
    public class Account
    {

        public Account()
        {
            Transactions = new List<Transaction>();
        }
        public long Id { get; set; }

        public decimal Balance { get; set; }

        public IList<Transaction> Transactions { get; set; }
        public Customer Customer { get; set; }
        public string Name { get; set; }
    }
}