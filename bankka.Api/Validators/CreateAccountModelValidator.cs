using bankka.Api.Models;
using FluentValidation;

namespace bankka.Api.Validators
{
    public class CreateAccountModelValidator : AbstractValidator<CreateAccountModel>
    {
        public CreateAccountModelValidator()
        {
            RuleFor(x => x.Id).NotNull().WithErrorCode("3000");
            RuleFor(x => x.Name).Length(0, 20).WithErrorCode("3001");
        }
    }
}