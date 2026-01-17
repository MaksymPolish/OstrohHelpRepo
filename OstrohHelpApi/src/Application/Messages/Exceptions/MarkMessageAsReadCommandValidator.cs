
using FluentValidation;

namespace Application.Messages.Commands;

public class MarkMessageAsReadCommandValidator : AbstractValidator<MarkMessageAsReadCommand>
{
    public MarkMessageAsReadCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().WithMessage("MessageId is required");
    }
}
