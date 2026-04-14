using FluentValidation;

namespace Application.Messages.Commands;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConsultationId).NotEmpty().WithMessage("ConsultationId is required");
        RuleFor(x => x.SenderId).NotEmpty().WithMessage("SenderId is required");
        RuleFor(x => x.EncryptedContent).NotEmpty().WithMessage("EncryptedContent is required");
        RuleFor(x => x.Iv).NotEmpty().WithMessage("Initialization vector (Iv) is required");
        RuleFor(x => x.AuthTag).NotEmpty().WithMessage("Authentication tag is required");
        
        // Validate encryption field sizes
        RuleFor(x => x.Iv).Must(iv => iv.Length == 12)
            .WithMessage("Initialization vector must be 12 bytes (96 bits)");
        RuleFor(x => x.AuthTag).Must(tag => tag.Length == 16)
            .WithMessage("Authentication tag must be 16 bytes (128 bits)");
    }
}
