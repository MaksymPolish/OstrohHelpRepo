using FluentValidation;

namespace Application.Consultations.Commands;

public class DeleteConsultationCommandValidator : AbstractValidator<DeleteConsultationCommand>
{
    public DeleteConsultationCommandValidator()
    {
        RuleFor(x => x.id).NotEmpty().WithMessage("id is required");
    }
}
