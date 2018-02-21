using System;
using System.Collections.Generic;
using System.Text;

namespace bankka.Commands.Customers
{
    public class NewCustomerResponse
    {
        public long CustomerId { get; }

        public NewCustomerResponse(long customerId)
        {
            CustomerId = customerId;
        }
    }
}
