using System;

namespace bankka.Core.Entities
{
    public class Transaction
    {
        public  long Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime DateTime { get; set; }
    }
}