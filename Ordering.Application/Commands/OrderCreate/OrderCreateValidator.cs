using FluentValidation;

namespace Ordering.Application.Commands.OrderCreate
{
    public class OrderCreateValidator : AbstractValidator<OrderCreateCommand>
    {
        public OrderCreateValidator()
        {
            RuleFor(k => k.SellerUserName)
                .EmailAddress()
                .NotEmpty();

            RuleFor(k => k.ProductId)
                .NotEmpty();
        }
    }
}