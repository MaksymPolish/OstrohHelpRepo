using FluentValidation;

namespace Application.Messages.Commands;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConsultationId).NotEmpty().WithMessage("ConsultationId is required");
        RuleFor(x => x.SenderId).NotEmpty().WithMessage("SenderId is required");
        RuleFor(x => x.ReceiverId).NotEmpty().WithMessage("ReceiverId is required");
        RuleFor(x => x.Text).NotEmpty().WithMessage("Text is required");
    }
}
