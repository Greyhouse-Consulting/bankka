using System.Collections.Generic;

namespace bankka.Api.Models
{
    public class ErrorModel
    {
        public ErrorModel(string code, string message)
        {
            Code = code;
            Message = message;
            Properties = new List<ErrorModelProperty>();
        }
        public string Code { get; }
        public string Message { get; }

        public IList<ErrorModelProperty> Properties { get; private set; }
    }
}