using FluentValidation;

namespace Application.Consultations.Commands;

public class UpdateConsultationCommandValidator : AbstractValidator<UpdateConsultationCommand>
{
    public UpdateConsultationCommandValidator()
    {
        RuleFor(x => x.consultationId).NotEmpty().WithMessage("consultationId is required");
        RuleFor(x => x.statusId).NotEmpty().WithMessage("statusId is required");
        RuleFor(x => x.studentId).NotEmpty().WithMessage("studentId is required");
        RuleFor(x => x.psyhologistId).NotEmpty().WithMessage("psyhologistId is required");
        RuleFor(x => x.scheduledTime).NotEmpty().WithMessage("scheduledTime is required");
    }
}
