using bankka.Api.Models;
using FluentValidation;

namespace bankka.Api.Validators
{
    public class TransactionModelValidator : AbstractValidator<TransactionModel>
    {
        public TransactionModelValidator()
        {
            RuleFor(p => p.Amount).GreaterThan(0).WithErrorCode("4000").WithMessage("Amount has to be greater that 0");
            RuleFor(p => p.ToAccountId).NotEmpty().WithErrorCode("4001").WithMessage("To accont cannot be empty");
            RuleFor(p => p.FromAccountId).NotEmpty().WithErrorCode("4002").WithMessage("From Account cannot be empty");
        }
    }
}