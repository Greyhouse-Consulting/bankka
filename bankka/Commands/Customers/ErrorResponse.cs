using System;
using System.Collections.Generic;
using System.Text;

namespace bankka.Commands.Customers
{
    public class ErrorResponse
    {
        public string Message { get; }

        public ErrorResponse(string message)
        {
            Message = message;
        }



    }
}
