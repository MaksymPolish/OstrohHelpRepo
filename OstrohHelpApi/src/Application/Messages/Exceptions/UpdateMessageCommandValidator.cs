using FluentValidation;

namespace Application.Messages.Commands;

public class UpdateMessageCommandValidator : AbstractValidator<UpdateMessageCommand>
{
    public UpdateMessageCommandValidator()
    {
        RuleFor(x => x.id).NotEmpty().WithMessage("id is required");
        RuleFor(x => x.encryptedContent)
            .NotNull().WithMessage("encrypted_content is required")
            .Must(x => x.Length > 0).WithMessage("encrypted_content must not be empty");

        RuleFor(x => x.iv)
            .NotNull().WithMessage("iv is required")
            .Must(x => x.Length == 12).WithMessage("iv must be exactly 12 bytes for AES-GCM");

        RuleFor(x => x.authTag)
            .NotNull().WithMessage("auth_tag is required")
            .Must(x => x.Length == 16).WithMessage("auth_tag must be exactly 16 bytes for AES-GCM");
    }
}
