using System.Collections.Generic;

namespace bankka.Core.Entities
{
    public class Customer
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public IList<Account> Accounts { get; set; }
        public string PhoneNumber { get; set; }
    }
}