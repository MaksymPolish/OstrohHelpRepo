using FluentValidation;

namespace Application.Messages.Commands;

public class UpdateMessageCommandValidator : AbstractValidator<UpdateMessageCommand>
{
    public UpdateMessageCommandValidator()
    {
        RuleFor(x => x.id).NotEmpty().WithMessage("id is required");
        RuleFor(x => x.text).NotEmpty().WithMessage("text is required");
    }
}
