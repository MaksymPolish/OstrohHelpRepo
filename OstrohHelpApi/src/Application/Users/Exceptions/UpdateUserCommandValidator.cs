using FluentValidation;

namespace Application.Users.Commands;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.userId).NotEmpty().WithMessage("userId is required");
        RuleFor(x => x.roleId).NotEmpty().WithMessage("roleId is required");
    }
}
